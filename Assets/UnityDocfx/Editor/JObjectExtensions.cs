using Unity.Plastic.Newtonsoft.Json.Linq;

namespace Lustie.UnityDocfx
{
    public static class JObjectExtensions
    {
        /// <summary>
        /// Function to recursively replace values
        /// </summary>
        public static void ReplaceValues(this JToken token, string target, string replacement)
        {
            if (token is JObject)
            {
                foreach (var property in token.Children<JProperty>())
                {
                    ReplaceValues(property.Value, target, replacement);
                }
            }
            else if (token is JArray)
            {
                foreach (var item in token.Children())
                {
                    ReplaceValues(item, target, replacement);
                }
            }
            else if (token is JValue)
            {
                //UnityEngine.Debug.Log(token.Path);
                if (token.Path == target)
                {
                    token.Replace(replacement);
                }
            }
        }

        /// <summary>
        /// Get property as JObject
        /// </summary>
        public static JObject GetJObject(this JObject jobject, string propertyName)
        {
            return jobject.GetValue(propertyName) as JObject;
        }

        /// <summary>
        /// Get property as JArray
        /// </summary>
        public static JArray GetJArray(this JObject jobject, string propertyName)
        {
            return jobject.GetValue(propertyName) as JArray;
        }

        /// <summary>
        /// Get property as JValue
        /// </summary>
        public static JValue GetJValue(this JObject jobject, string propertyName)
        {
            return jobject.GetValue(propertyName) as JValue;
        }

        /// <summary>
        /// Get property value (JToken)
        /// </summary>
        public static T GetValue<T>(this JObject jobject, string propertyName)
            where T : JToken
        {
            return jobject.GetValue(propertyName) as T;
        }
    }
}
