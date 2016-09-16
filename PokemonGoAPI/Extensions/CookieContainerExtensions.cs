using System.Collections.Generic;
using System.Reflection;

namespace System.Net
{

    /// <summary>
    /// 
    /// </summary>
    public static class CookieContainerExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        internal static IEnumerable<Cookie> GetCookies(this CookieContainer cookieContainer, string domain)
        {
            dynamic domainTable = GetInstanceField(cookieContainer, "_domainTable");
            foreach (var entry in domainTable)
            {
                string key = GetPropertyValue<string>(entry, "Key");

                if (key.Contains(domain))
                {
                    dynamic value = GetPropertyValue<dynamic>(entry, "Value");

                    var internalList = (SortedList<string, CookieCollection>)GetInstanceField(value, "_list");
                    foreach (var li in internalList)
                    {
                        foreach (Cookie cookie in li.Value)
                        {
                            yield return cookie;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        internal static object GetInstanceField(object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = instance.GetType().GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal static T GetPropertyValue<T>(object instance, string propertyName)
        {
            var pi = instance.GetType().GetProperty(propertyName);
            return (T)pi.GetValue(instance, null);
        }

    }

}