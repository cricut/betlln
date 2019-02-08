using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Betlln.Data.Integration.Core
{
    internal abstract class Binder
    {
        protected Binder(object targetObject, string name, PropertyInfo[] properties)
        {
            Name = name;
            TargetObject = targetObject;
            Properties = properties;
        }

        private string Name { get; }
        private object TargetObject { get; }
        private PropertyInfo[] Properties { get; }

        public void BindDefaultValues()
        {
            foreach (PropertyInfo property in Properties)
            {
                DefaultValueAttribute defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValueAttribute != null)
                {
                    property.SetValue(TargetObject, defaultValueAttribute.Value);
                }
            }
        }

        public void BindFromConfig()
        {
            foreach (PropertyInfo propertyInfo in Properties)
            {
                string configurationValue = ConfigurationManager.AppSettings[propertyInfo.Name];
                if (!string.IsNullOrWhiteSpace(configurationValue))
                {
                    BindProperty(propertyInfo, configurationValue);
                }
            }
        }

        public void BindProperty(string propertyName, string value)
        {
            PropertyInfo property = GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException($"The property {Name}.{propertyName} does not exist.");
            }

            BindProperty(property, value);
        }

        private void BindProperty(PropertyInfo property, string value)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
            object propertyValue = typeConverter.ConvertFrom(value);
            property.SetValue(TargetObject, propertyValue);
        }

        public object GetPropertyValue(string propertyName)
        {
            PropertyInfo propertyInfo = GetProperty(propertyName);
            return propertyInfo == null ? null : propertyInfo.GetValue(TargetObject);
        }

        private PropertyInfo GetProperty(string propertyName)
        {
            return Properties.FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < Properties.Length; i++)
            {
                PropertyInfo property = Properties[i];

                string propertyValue = (property.GetValue(TargetObject) ?? string.Empty).ToString();

                PasswordPropertyTextAttribute passwordMarker = property.GetCustomAttribute<PasswordPropertyTextAttribute>();
                if (passwordMarker != null)
                {
                    propertyValue = "********";
                }

                output.AppendFormat("{0} = {1}", property.Name, propertyValue);
                if (i != Properties.Length - 1)
                {
                    output.AppendLine();
                }
            }

            return output.ToString();
        }
    }
}