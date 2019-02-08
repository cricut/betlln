using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Betlln.Data.Integration.Collections;
using Betlln.Data.Integration.Core;
using Newtonsoft.Json.Linq;

namespace Betlln.Data.Integration.Json
{
    public class JsonDocument
    {
        private static readonly Regex ArrayMatcher = new Regex(@"^(?'name'.+?)\[\d+\]", RegexOptions.Compiled);

        public JsonDocument(string rawContent)
        {
            if (string.IsNullOrWhiteSpace(rawContent))
            {
                throw new ArgumentNullException();
            }

            RawContent = rawContent;
            Properties = new ListOf<JsonProperty>();

            JToken parentElement = JToken.Parse(RawContent);
            FillProperties(parentElement);
        }

        public string RawContent { get; }
        public ListOf<JsonProperty> Properties { get; }

        private void FillProperties(JToken parentElement)
        {
            foreach (JValue jValue in GetLeafValues(parentElement))
            {
                Match match = ArrayMatcher.Match(jValue.Path);
                bool isArray = match.Success;
                string propertyName = isArray ? match.Groups["name"].Value : jValue.Path;

                JsonProperty property = Properties.Find(x => x.Name == propertyName);
                if (property == null)
                {
                    property = new JsonProperty(propertyName, isArray ? "[" : null);
                    Properties.Add(property);
                }

                string propertyValue = jValue.Value?.ToString();
                if (isArray)
                {
                    property.Value += propertyValue + ",";
                }
                else
                {
                    property.Value = propertyValue;
                }
            }

            IEnumerable<JsonProperty> jsonProperties = Properties.Where(IsArray);
            foreach (JsonProperty property in jsonProperties)
            {
                property.Value = $"{property.Value.Substring(0, property.Value.Length - 1)}]";
            }
        }

        //http://anotherdevblog.net/posts/flattening-json-in-json-net
        private static IEnumerable<JValue> GetLeafValues(JToken jToken)
        {
            if (jToken is JValue jValue)
            {
                yield return jValue;
            }
            else if (jToken is JArray jArray)
            {
                foreach (JValue result in GetLeafValuesFromJArray(jArray))
                {
                    yield return result;
                }
            }
            else if (jToken is JProperty jProperty)
            {
                foreach (JValue result in GetLeafValuesFromJProperty(jProperty))
                {
                    yield return result;
                }
            }
            else if (jToken is JObject jObject)
            {
                foreach (JValue result in GetLeafValuesFromJObject(jObject))
                {
                    yield return result;
                }
            }
        }

        private static IEnumerable<JValue> GetLeafValuesFromJArray(JArray jArray)
        {
            foreach (JToken token in jArray)
            {
                foreach (JValue result in GetLeafValues(token))
                {
                    yield return result;
                }
            }
        }

        private static IEnumerable<JValue> GetLeafValuesFromJProperty(JProperty jProperty)
        {
            foreach (JValue result in GetLeafValues(jProperty.Value))
            {
                yield return result;
            }
        }

        private static IEnumerable<JValue> GetLeafValuesFromJObject(JObject jObject)
        {
            foreach (JToken jToken in jObject.Children())
            {
                foreach (JValue result in GetLeafValues(jToken))
                {
                    yield return result;
                }
            }
        }

        private static bool IsArray(JsonProperty property)
        {
            return property.Value != null && property.Value.StartsWith("[") && property.Value.EndsWith(",");
        }

        public DataRecord ToRecord()
        {
            DataRecord record = new DataRecord();

            foreach (JsonProperty property in Properties)
            {
                record[property.Name] = property.Value;
            }

            return record;
        }

        public override string ToString()
        {
            return RawContent;
        }
    }
}