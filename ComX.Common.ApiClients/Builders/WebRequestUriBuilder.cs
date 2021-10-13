using ComX.Common.ApiClients.Helpers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

namespace ComX.Common.ApiClients.Builders
{
    public sealed class WebRequestUriBuilder
    {
        private string Path { get; set; }

        private Dictionary<string, string> Query { get; set; }

        private string Fragment { get; set; }

        public Uri Uri => BuildUri();

        public WebRequestUriBuilder()
        {
            this.Path = string.Empty;
            this.Query = new Dictionary<string, string>();
            this.Fragment = string.Empty;
        }

        /// <summary>
        /// Add each path component or in one piece
        /// E.G.SetPath("a/b/c")
        /// E.G.SetPath("a/b", "c")
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment
        /// </summary>
        public void SetPath(params string[] path)
        {
            this.Path = string.Join("/", path.Select(x =>
            {
                if (x.EndsWith("/"))
                {
                    x = x.Substring(0, x.Length - 1);
                }

                if (x.StartsWith("/"))
                {
                    x = x.Substring(1, x.Length - 1);
                }

                return x;
            }));
        }
       
        /// <summary>
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment.
        /// <para>Adding a query parameter where the type is enum</para>
        /// <para>The query pair is composed of the query key and the value of the enum. The value depends on valueAsString</para>
        /// </summary>
        /// <param name="queryKey">The key name</param>
        /// <param name="value">The enum value</param>
        /// <param name="valueAsString">If true the value will be the field name instead of numeric value</param>
        public void AddEnumQueryParameter<TEnum>(string queryKey, TEnum value, bool valueAsString = false)
            where TEnum : Enum
        {
            if (valueAsString)
            {
                this.Query.Add(queryKey, value.ToString());
            }
            else
            {
                var type = typeof(TEnum);
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                this.Query.Add(queryKey, underlyingValue.ToString());
            }
        }

        /// <summary>
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment.
        /// <para>Adding a query parameter where the type is enum</para>
        /// <para>The query pair is composed of the query key and the value of the enum. The value is retrieved from an attribute decorating
        /// the enum field</para>
        /// </summary>
        /// <param name="queryKey">The key name</param>
        /// <param name="value">The enum value</param>
        /// <param name="getValue">Factory method to get the query value from the attribute</param>
        public void AddEnumQueryParameter<TEnum, TAttribute>(string queryKey, TEnum value, Func<TAttribute, string> getValue)
            where TEnum : Enum
            where TAttribute : Attribute
        {
            var enumType = typeof(TEnum);
            string enumFieldName = Enum.GetName(enumType, value);
            var fInfo = enumType.GetField(enumFieldName, BindingFlags.Public | BindingFlags.Static);

            var attribute = fInfo.GetCustomAttribute<TAttribute>();

            if (attribute is null)
            {
                throw new Exception($"Could not find the attribute \'{typeof(TAttribute).FullName}\' decorating the field \'{enumFieldName}\'");
            }

            string queryValue = getValue(attribute);
            this.Query.Add(queryKey, queryValue);
        }

        /// <summary>
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment.
        /// <para>Adding a query parameter where the type is enum</para>
        /// <para>The query pair is composed of the query key and the value of the enum. The value is retrieved from an attribute decorating
        /// the enum field</para>
        /// </summary>
        /// <param name="queryKey">The key name</param>
        /// <param name="value">The enum value</param>
        /// <param name="getValue">Factory method to get the query value from the attribute</param>
        public void AddEnumQueryParameter<TModel, TEnum, TAttributeKey, TAttributeValue>(
            Expression<Func<TModel, TEnum>> property,TEnum value, 
            Func<TAttributeKey, string> getKey,  Func<TAttributeValue, string> getValue)
            where TEnum : Enum
            where TAttributeKey : Attribute
            where TAttributeValue : Attribute
        {
            // getting the key
            string propertyName = PropertyPathHelper.GetLastPropertyName(property);
            Type modelType = typeof(TModel);

            var propertyInfo = modelType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo is null)
            {
                throw new Exception($"Could not find property \'{propertyName}\' on model \'{modelType.FullName}\'");
            }

            TAttributeKey attribute = propertyInfo.GetCustomAttribute<TAttributeKey>();

            if (attribute is null)
            {
                throw new Exception($"Property \'{propertyName}\' is not decorated with \'{typeof(TAttributeKey).FullName}\'");
            }

            string queryKey = getKey(attribute);

            //getting the value
            var enumType = typeof(TEnum);
            string enumFieldName = Enum.GetName(enumType, value);
            var fInfo = enumType.GetField(enumFieldName, BindingFlags.Public | BindingFlags.Static);

            var attributeValue = fInfo.GetCustomAttribute<TAttributeValue>();

            if (attributeValue is null)
            {
                throw new Exception($"Could not find the attribute \'{typeof(TAttributeValue).FullName}\' decorating the field \'{enumFieldName}\'");
            }

            string queryValue = getValue(attributeValue);
            this.Query.Add(queryKey, queryValue);
        }

        /// <summary>
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment
        /// <para>Adds a query key and value to the uri. The query pair is made of the queryKey and the value</para>
        /// <para>TProperty is treated as string, int, short, double, long, Guid, bool using a simple ToString</para>
        /// <para>TProperty is treated as enum using a simple ToString()</para>
        /// <para>TProperty is serialized using Newtonsoft for complex types</para>
        /// </summary>
        public void AddQueryParameter<TProperty>(string queryKey, TProperty value)
        {
            AddQueryValueInternal<TProperty>(value, queryKey);
        }

        /// <summary>
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment.
        /// <para>Adds a query key and value. The property is retrieved by the proeprty name and the key is retrieved
        /// using the attribute and the getKey method</para>
        /// <para>TProperty is treated as string, int, short, double, long, Guid, bool using a simple ToString</para>
        /// <para>TProperty is treated as enum using a simple ToString()</para>
        /// <para>TProperty is serialized using Newtonsoft for complex types</para>
        /// </summary>
        /// <param name="propertyName">The property name is the query key</param>
        /// <param name="value">The value</param>
        /// <param name="getKey">The method used to determine the query key being in the attribute decorating the property</param>
        public void AddQueryParameter<TProperty, TAttribute>(string propertyName, TProperty value,
            Func<TAttribute, string> getKey) where TAttribute : Attribute
        {
            Type modelType = typeof(TProperty);

            var propertyInfo = modelType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo is null)
            {
                throw new Exception($"Could not find property \'{propertyName}\' on model \'{modelType.FullName}\'");
            }

            TAttribute attribute = propertyInfo.GetCustomAttribute<TAttribute>();

            if (attribute is null)
            {
                throw new Exception($"Property \'{propertyName}\' is not decorated with \'{typeof(TAttribute).FullName}\'");
            }

            string queryKey = getKey(attribute);

            AddQueryValueInternal(value, queryKey);
        }

        /// <summary>
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment.
        /// <para>TProperty is treated as string, int, short, double, long, Guid, bool using a simple ToString</para>
        /// <para>TProperty is treated as enum using it's numerical value</para>
        /// <para>TProperty is serialized using Newtonsoft for complex types</para>
        /// </summary>
        /// <param name="property">The property name is the query key</param>
        /// <param name="value">The value</param>
        public void AddQueryParameter<TModel, TProperty>(Expression<Func<TModel, TProperty>> property, TProperty value)
        {
            var propertyName = PropertyPathHelper.GetLastPropertyName(property);
            AddQueryValueInternal<TProperty>(value, propertyName);
        }

        /// <summary>
        /// Extracts the property from a model. With the <paramref name="getKey"/> method it extracts the query key value for 
        /// that property
        /// <para>TProperty is treated as string, int, short, double, long, Guid, bool using a simple ToString</para>
        /// <para>TProperty is treated as enum using it's numerical value</para>
        /// <para>TProperty is serialized using Newtonsoft for complex types</para>
        /// </summary>
        /// <param name="value">The type of the property</param>
        /// <param name="getKey">The model for which the proeprty belongs</param>
        public void AddQueryParameter<TModel, TProperty, TAttribute>(Expression<Func<TModel, TProperty>> property,
            TProperty value, Func<TAttribute, string> getKey) where TAttribute : Attribute
        {
            string propertyName = PropertyPathHelper.GetLastPropertyName(property);
            Type modelType = typeof(TModel);

            var propertyInfo = modelType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo is null)
            {
                throw new Exception($"Could not find property \'{propertyName}\' on model \'{modelType.FullName}\'");
            }

            TAttribute attribute = propertyInfo.GetCustomAttribute<TAttribute>();

            if (attribute is null)
            {
                throw new Exception($"Property \'{propertyName}\' is not decorated with \'{typeof(TAttribute).FullName}\'");
            }

            string queryKey = getKey(attribute);

            AddQueryValueInternal<TProperty>(value, queryKey);
        }

        private void AddQueryValueInternal<TProperty>(TProperty value, string queryKey)
        {
            switch (value)
            {
                case int _:
                case short _:
                case double _:
                case long _:
                case Guid _:
                case string _:
                    this.Query.Add(queryKey, value.ToString());
                    return;
                case bool _:
                    this.Query.Add(queryKey, value.ToString().ToLower());
                    return;
            }

            var type = typeof(TProperty);
            if (type.IsEnum)
            {
                object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                this.Query.Add(queryKey, underlyingValue.ToString());
                return;
            }
            string queryValue = HttpUtility.UrlEncode(JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));

            this.Query.Add(queryKey, queryValue);
        }

        /// <summary>
        /// Add fragment name. # is added internally.
        /// E.G. foo://host:port/path1/path2?queryKey=queryValue#fragment
        /// </summary>
        public void SetFragment(string fragment)
        {
            if (!fragment.StartsWith("#"))
            {
                this.Fragment = $"#{fragment}";
            }
            else
            {
                this.Fragment = fragment;
            }
        }

        internal Uri BuildUri()
        {
            //solve query
            QueryString queryString = new QueryString();

            foreach (var queryItem in Query)
            {
                queryString = queryString.Add(queryItem.Key, queryItem.Value);
            }

            StringBuilder uri = new StringBuilder();

            uri.Append(this.Path);

            if (queryString.HasValue)
            {
                uri.Append(queryString.Value);
            }

            if (!string.IsNullOrEmpty(Fragment))
            {
                uri.Append(Fragment);
            }

            return new Uri(uri.ToString(), UriKind.Relative);
        }
    }
}
