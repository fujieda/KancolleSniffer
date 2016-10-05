using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KancolleSniffer
{
    public class JsonParser
    {
        private readonly string _source;
        private int _position;

        public static JsonObject Parse(string json)
        {
            return new JsonParser(json).Parse();
        }

        private JsonParser(string json)
        {
            _source = json;
        }

        private JsonObject Parse()
        {
            var ch = NextChar();
            if ('0' <= ch && ch <= '9' || ch == '-')
                return ParseNumber();
            switch (ch)
            {
                case 'n':
                    if (IsMatch("null"))
                        return null;
                    goto invalid;
                case 't':
                    if (IsMatch("true"))
                        return new JsonObject(true);
                    goto invalid;
                case 'f':
                    if (IsMatch("false"))
                        return new JsonObject(false);
                    goto invalid;
                case '"':
                    return ParseString();
                case '[':
                    return ParseArray();
                case '{':
                    return ParseObject();
            }
            invalid:
            throw new JsonParserException($"'{ch}'不正な文字です。 at {_position}");
        }

        private bool IsMatch(string s)
        {
            foreach (char ch in s)
            {
                var src = LookAhead();
                if (ch != src)
                    return false;
                Consume();
            }
            return true;
        }

        private JsonObject ParseNumber()
        {
            return new JsonObject(GetNumber());
        }

        private double GetNumber()
        {
            var result = 0d;
            var ch = LookAhead();
            var sign = 1d;
            if (ch == '-')
            {
                sign = -1;
                Consume();
                ch = LookAhead();
            }
            while ('0' <= ch && ch <= '9')
            {
                result = (result * 10.0) + (ch - '0');
                Consume();
                ch = LookAhead();
            }
            if (ch != '.')
                return sign * result;
            var exp = 0.1;
            Consume();
            ch = LookAhead();
            while ('0' <= ch && ch <= '9')
            {
                result += (ch - '0') * exp;
                exp *= 0.1;
                Consume();
                ch = LookAhead();
            }
            return sign * result;
        }

        private JsonObject ParseString()
        {
            return new JsonObject(GetString());
        }

        private string GetString()
        {
            Consume();
            var len = 0;
            while (true)
            {
                var ch = LookAhead();
                if (ch == '\\')
                {
                    Consume();
                    Consume();
                    len += 2;
                    continue;
                }
                if (ch == '"')
                {
                    Consume();
                    break;
                }
                len++;
                Consume();
            }
            return Unescape(_source.Substring(_position - len - 1, len));
        }

        private static readonly Regex EscapeRegex =
            new Regex(@"\\[^u]|\\u(?:[0-9A-Fa-f]{4})", RegexOptions.Compiled);

        private string Unescape(string s)
        {
            return EscapeRegex.Replace(s, m =>
            {
                switch (m.Value[1])
                {
                    case '/':
                        return '/'.ToString();
                    case '"':
                    case '\\':
                    case 'b':
                    case 'f':
                    case 'n':
                    case 'r':
                    case 't':
                    case 'u':
                        return Regex.Unescape(m.Value).ToString();
                    default:
                        throw new JsonParserException("不正なエスケープシーケンスです。 at {");
                }
            });
        }

        private JsonObject ParseArray()
        {
            Consume();
            var ary = new List<JsonObject>();
            while (true)
            {
                var ch = NextChar();
                if (ch == ']')
                {
                    Consume();
                    return new JsonObject(ary);
                }
                ary.Add(Parse());
                ch = NextChar();
                if (ch != ',' && ch != ']')
                    throw new JsonParserException($"','か']'が必要です。 at {_position}");
                if (ch == ']')
                {
                    Consume();
                    return new JsonObject(ary);
                }
                Consume();
            }
        }

        private JsonObject ParseObject()
        {
            Consume();
            var dict = new Dictionary<string, JsonObject>();
            while (true)
            {
                var ch = NextChar();
                if (ch == '}')
                {
                    Consume();
                    return new JsonObject(dict);
                }
                if (ch != '"')
                    throw new JsonParserException($"文字列が必要です。 at {_position}");
                var key = GetString();
                ch = NextChar();
                if (ch != ':')
                    throw new JsonParserException($"':'が必要です。 at {_position}");
                Consume();
                var value = Parse();
                dict.Add(key, value);
                ch = NextChar();
                if (ch != ',' && ch != '}')
                    throw new JsonParserException($"','か'}}'が必要です。 at {_position}");
                if (ch == '}')
                {
                    Consume();
                    return new JsonObject(dict);
                }
                Consume();
            }
        }

        private char LookAhead()
        {
            if (_source.Length == _position)
                return '\0';
            return _source[_position];
        }

        private void Consume()
        {
            if (_source.Length == _position)
                throw new JsonParserException($"入力が途切れています。 at {_position}");
            _position++;
        }

        private char NextChar()
        {
            while (true)
            {
                var ch = LookAhead();
                if (!(ch == '\r' || ch == '\n' || ch == '\t' || ch == ' '))
                    return ch;
                Consume();
            }
        }
    }

    public class JsonObject : DynamicObject
    {
        private readonly JsonType _type;
        private readonly bool _bool;
        private readonly double _number;
        private readonly string _string;
        private readonly List<JsonObject> _array;
        private readonly Dictionary<string, JsonObject> _dict;

        public bool IsArray => _type == JsonType.Array;
        public bool IsObject => _type == JsonType.Object;
        public bool IsDefined(string attr) => IsObject && _dict.ContainsKey(attr);

        public JsonObject(bool b)
        {
            _type = JsonType.Bool;
            _bool = b;
        }

        public JsonObject(double n)
        {
            _type = JsonType.Number;
            _number = n;
        }

        public JsonObject(string s)
        {
            _type = JsonType.String;
            _string = s;
        }

        public JsonObject(List<JsonObject> ary)
        {
            _type = JsonType.Array;
            _array = ary;
        }

        public JsonObject(Dictionary<string, JsonObject> dict)
        {
            _type = JsonType.Object;
            _dict = dict;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (_type != JsonType.Object)
                return false;
            JsonObject dict;
            if (!_dict.TryGetValue(binder.Name, out dict))
                return false;
            result = dict?.Value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = _type == JsonType.Object && _dict.ContainsKey(binder.Name);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            switch (_type)
            {
                case JsonType.Array:
                    result = _array[(int)indexes[0]]?.Value;
                    return true;
                case JsonType.Object:
                    result = _dict[(string)indexes[0]]?.Value;
                    return true;
            }
            result = null;
            return false;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(IEnumerable))
            {
                switch (_type)
                {
                    case JsonType.Array:
                        result = _array.Select(x => x.Value);
                        return true;
                    case JsonType.Object:
                        result = _dict.Select(x => new KeyValuePair<string, dynamic>(x.Key, x.Value));
                        return true;
                    default:
                        result = null;
                        return false;
                }
            }
            if (_type != JsonType.Array)
                return ConvertPrivateType(binder.Type, out result);
            if (binder.Type == typeof(int[]))
            {
                result = _array.Select(x => (int)x._number).ToArray();
                return true;
            }
            if (binder.Type.IsArray)
            {
                return ConvertArray(binder.Type.GetElementType(), _array, out result);
            }
            result = null;
            return false;
        }

        private bool ConvertPrivateType(Type type, out object result)
        {
            if (type == typeof(bool) && _type == JsonType.Bool)
            {
                result = _bool;
                return true;
            }
            if (type == typeof(int) && _type == JsonType.Number)
            {
                result = (int)_number;
                return true;
            }
            if (type == typeof(double) && _type == JsonType.Number)
            {
                result = _number;
                return true;
            }
            if (type == typeof(string) && _type == JsonType.String)
            {
                result = _string;
                return true;
            }
            if (type == typeof(object))
            {
                result = Value;
                return true;
            }
            result = null;
            return false;
        }

        private bool ConvertArray(Type type, List<JsonObject> values, out object result)
        {
            result = null;
            var array = Array.CreateInstance(type, values.Count);
            for (var i = 0; i < array.Length; i++)
            {
                if (type.IsArray)
                {
                    object one;
                    if (!values[i].IsArray || !ConvertArray(type.GetElementType(), values[i]._array, out one))
                        return false;
                    array.SetValue((dynamic)one, i);
                }
                else
                {
                    object one;
                    if (!values[i].ConvertPrivateType(type, out one))
                        return false;
                    array.SetValue((dynamic)one, i);
                }
            }
            result = array;
            return true;
        }

        private object Value
        {
            get
            {
                switch (_type)
                {
                    case JsonType.Bool:
                        return _bool;
                    case JsonType.Number:
                        return _number;
                    case JsonType.String:
                        return _string;
                }
                return this;
            }
        }

        private enum JsonType
        {
            Bool,
            Number,
            String,
            Array,
            Object
        }
    }

    public class JsonParserException : Exception
    {
        public JsonParserException()
        {
        }

        public JsonParserException(string message) : base(message)
        {
        }
    }
}