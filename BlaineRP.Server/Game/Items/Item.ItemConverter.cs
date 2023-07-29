using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BlaineRP.Server.Game.Items
{
    public abstract partial class Item
    {
        public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
        {
            protected override JsonConverter ResolveContractConverter(Type objectType)
            {
                if (typeof(Item).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                    return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
                return base.ResolveContractConverter(objectType);
            }
        }
        
        public class ItemConverter : JsonConverter
        {
            public static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

            public override bool CanConvert(Type objectType) => objectType == typeof(Item);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                JObject jo = JObject.Load(reader);

                var type = Stuff.GetType(jo["I"].Value<string>());

                if (type == null)
                    return null;

                return JsonConvert.DeserializeObject(jo.ToString(), type, SpecifiedSubclassConversion);
            }

            public override bool CanWrite => false;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
        }
    }
}