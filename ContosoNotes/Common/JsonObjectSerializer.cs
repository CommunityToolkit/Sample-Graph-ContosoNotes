// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace ContosoNotes.Common
{
    /// <summary>
    /// This is a Serializer which should mimic the previous functionality of 6.1.1 release of the Toolkit with Newtonsoft.Json.
    /// </summary>
    public class JsonObjectSerializer : IObjectSerializer
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

    public T Deserialize<T>(object value)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            // Note: If you're creating a new app, you could just use the serializer directly.
            // This if/return combo is to maintain compatibility with 6.1.1
            if (typeInfo.IsPrimitive || type == typeof(string))
            {
                return (T)Convert.ChangeType(value, type);
            }

            return JsonConvert.DeserializeObject<T>((string)value, _serializerSettings);
        }

        public object Serialize<T>(T value)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            // Note: If you're creating a new app, you could just use the serializer directly.
            // This if/return combo is to maintain compatibility with 6.1.1
            if (typeInfo.IsPrimitive || type == typeof(string))
            {
                return value;
            }

            return JsonConvert.SerializeObject(value, _serializerSettings);
        }
    }
}
