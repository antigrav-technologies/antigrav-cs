using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antigrav {/*
    internal class Types {
        public enum JsonValueType {
            Object,
            Array,
            String,
            Number,
            Boolean,
            Null
        }

        public abstract class JSONValue {
            public virtual JsonValueType Type { get; }
        }

        public class JsonObject : JSONValue {
            public override JsonValueType Type => JsonValueType.Object;
            public Dictionary<string, JSONValue> Properties { get; set; }
        }

        public class JsonArray : JSONValue {
            public override JsonValueType Type => JsonValueType.Array;
            public List<JSONValue> Items { get; set; }
        }

        public class JsonString : JSONValue {
            public override JsonValueType Type => JsonValueType.String;
        }

        public class JsonNumber : JSONValue {
            public override JsonValueType Type => JsonValueType.Number;
            public object Value { get; set; }
        }

        public class JsonBoolean : JSONValue {
            public override JsonValueType Type => JsonValueType.Boolean;
            public bool Value { get; set; }
        }

        public class JsonNull : JSONValue {
            public override JsonValueType Type => JsonValueType.Null;
        }

        public class DynamicJsonObject : JSONValue {
            public override JsonValueType Type { get; }

            public DynamicJsonObject() {}

            public DynamicJsonObject(JsonValueType type) {
                Type = type;
            }
            private JsonValueType GetJsonValueType(object value) {
                if (value is string) return JsonValueType.String;
                if (value is int || value is long) return JsonValueType.Number;
                if (value is float || value is double) return JsonValueType.Number;
                if (value is bool) return JsonValueType.Boolean;
                if (value is null) return JsonValueType.Null;
                if (value is IDictionary<string, object>) return JsonValueType.Object;
                if (value is IList<object>) return JsonValueType.Array;
                throw new ArgumentException("Unsupported type");
            }
        }
    }*/
}
