using System;
using MongoDB.Bson;

namespace Betlln.Data.Integration.Mongo
{
    internal static class BsonExtensionMethods
    {
        internal static object GetValue(this BsonDocument document, string jsonPath, Type valueType)
        {
            string[] pathParts = jsonPath.Split('.');

            BsonDocument section = document;
            foreach (string pathPart in pathParts)
            {
                if (section.Contains(pathPart))
                {
                    if (section[pathPart].IsBsonDocument)
                    {
                        section = section[pathPart].AsBsonDocument;
                    }
                    else
                    {
                        return section[pathPart].AsBsonValue.To(valueType);
                    }
                }
                else
                {
                    return DBNull.Value;
                }
            }

            return DBNull.Value;
        }

        private static object To(this BsonValue bsonValue, Type targetType)
        {
            if (bsonValue.IsBsonNull)
            {
                return DBNull.Value;
            }
            
            if (targetType == typeof(string))
            {
                if (!bsonValue.IsString)
                {
                    return bsonValue.ToString();
                }

                return bsonValue.AsString;
            }

            if (targetType == typeof(int))
            {
                return ToInt(bsonValue);
            }

            if (targetType == typeof(long))
            {
                return ToLong(bsonValue);
            }

            if (targetType == typeof(DateTime))
            {
                return ToDateTime(bsonValue);
            }

            if (targetType == typeof(decimal))
            {
                return ToDecimal(bsonValue);
            }

            throw new NotSupportedException();
        }

        private static object ToInt(BsonValue bsonValue)
        {
            if (!bsonValue.IsInt32)
            {
                return Convert.ToInt32(bsonValue.ToString());
            }

            return bsonValue.AsInt32;
        }

        private static object ToLong(BsonValue bsonValue)
        {
            if (!bsonValue.IsInt64)
            {
                return Convert.ToInt64(bsonValue.ToString());
            }

            return bsonValue.AsInt64;
        }

        private static object ToDateTime(BsonValue bsonValue)
        {
            if (!bsonValue.IsValidDateTime)
            {
                return Convert.ToDateTime(bsonValue.ToString());
            }

            return bsonValue.ToUniversalTime();
        }

        private static object ToDecimal(BsonValue bsonValue)
        {
            try
            {
                return bsonValue.AsDecimal;
            }
            catch (InvalidCastException)
            {
                return Convert.ToDecimal(bsonValue.ToString());
            }
        }
    }
}