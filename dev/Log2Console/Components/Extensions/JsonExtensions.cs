using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Log2Console.Components.Extensions
{
    public static class JsonExtensions
    {
        private const string TYPE_SEPARATOR_BEGIN = "TYPE:";
        private const string TYPE_SEPARATOR_END = "|";

        static JsonExtensions()
        {
        }

        public static JsonSerializerSettings DefaultSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    Converters = new[] { new StringEnumConverter() }
                };
            }
        }

        public static string ToJson(this object obj, Type sourceType = null, bool useCamelCase = false)
        {
            if (obj == null) { return null; }
            else if (obj is Newtonsoft.Json.Linq.JObject) { return obj.ToString(); }
            else if (obj is Newtonsoft.Json.Linq.JToken) { return obj.ToString(); }
            else if (sourceType != null && sourceType.IsEnum) { return obj.ToString(); }  // handling serialization of enums
            else if (obj is string) { return (string)obj; }
            else
            {
                var settings = DefaultSettings;
                if (useCamelCase) { settings.ContractResolver = new CamelCasePropertyNamesContractResolver(); }

                return JsonConvert.SerializeObject(obj, DefaultSettings);
            }
        }


        public static T FromJson<T>(this string jsonString)
        {
            return (T)FromJsonInternal(jsonString, typeof(T));
        }

        public static object FromJson(this string jsonString, Type type)
        {
            return FromJsonInternal(jsonString, type);
        }

        private static object FromJsonInternal(this string json, Type targetType)
        {
            if (json == null) { return null; }
            var errorList = new List<string>();
            var deserializedSettings = JsonExtensions.DefaultSettings;
            deserializedSettings.Error = (sender, errorArgs) => errorList.Add(errorArgs.ErrorContext.Error.Message);

            var response = targetType.IsEnum
                    ? json.ToEnum(targetType)
                    : JsonConvert.DeserializeObject(json, targetType, deserializedSettings);

            if (errorList.Count > 0) { throw new ArgumentException("Deserialized", string.Join(",", errorList)); }

            return response;
        }
    }
}
