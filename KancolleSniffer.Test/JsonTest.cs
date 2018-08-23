﻿using System.Collections.Generic;
using System.Linq;
using KancolleSniffer.Util;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class JsonTest
    {
        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void InvalidChar()
        {
            JsonParser.Parse("aaaa");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void InvalidToken()
        {
            JsonParser.Parse("nula");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void ShortToken()
        {
            JsonParser.Parse("tru");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void ShortTokenStartWithT()
        {
            JsonParser.Parse("taa");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void InvalidEscape()
        {
            JsonParser.Parse("\"\\aaaa\"");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void UnexpectedEos()
        {
            JsonParser.Parse("\"aaaa");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void ObjectColonRequired()
        {
            JsonParser.Parse("{\"api_result\",1}");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void ObjectStringRequired()
        {
            JsonParser.Parse("{api_result\":1}");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void ObjectTerminatorRequired()
        {
            JsonParser.Parse("{\"api_result\":1");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonParserException))]
        public void ArrayTerminatorRequired()
        {
            JsonParser.Parse("[1,2,3");
        }

        [TestMethod]
        public void Null()
        {
            Assert.AreEqual(JsonParser.Parse("null"), null);
        }

        [TestMethod]
        public void Bool()
        {
            Assert.IsTrue((bool)(dynamic)JsonParser.Parse("true"));
            Assert.IsFalse((bool)(dynamic)JsonParser.Parse("false"));
        }

        [TestMethod]
        public void Number()
        {
            Assert.AreEqual(123d, (double)(dynamic)JsonParser.Parse("123"));
            Assert.AreEqual(-123d, (double)(dynamic)JsonParser.Parse("-123"));
            Assert.AreEqual(123.456d, (double)(dynamic)JsonParser.Parse("123.456"));
            Assert.AreEqual(123, (int)(dynamic)JsonParser.Parse("123.456"));
        }

        [TestMethod]
        public void String()
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            Assert.AreEqual("api_result", (string)(dynamic)JsonParser.Parse(@"""api_result"""));
            Assert.AreEqual("/\"\\\b\f\n\r\t", (string)(dynamic)JsonParser.Parse(@"""\/\""\\\b\f\n\r\t"""));
            Assert.AreEqual("大和改二", (string)(dynamic)JsonParser.Parse(@"""\u5927\u548c\u6539\u4e8c"""));
            Assert.AreEqual(@"\", (string)(dynamic)JsonParser.Parse(@"""\\"""));
            // ReSharper restore SuspiciousTypeConversion.Global
        }

        [TestMethod]
        public void ObjectEmpty()
        {
            var obj = (dynamic)JsonParser.Parse("{}");
            Assert.IsTrue(obj.IsObject);
        }

        [TestMethod]
        public void CheckProperty()
        {
            Assert.IsFalse(((dynamic)JsonParser.Parse("{}")).api_result());
        }

        [TestMethod]
        public void CheckPropertyOfBool()
        {
            Assert.IsFalse(((dynamic)JsonParser.Parse("true")).api_result());
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void GetIndexOfBool()
        {
            Assert.IsFalse(((dynamic)JsonParser.Parse("true"))[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void PropertyNotExists()
        {
            Assert.IsTrue(((dynamic)JsonParser.Parse("{}")).api_result);
        }

        [TestMethod]
        public void ObjectWithNull()
        {
            var obj = (dynamic)JsonParser.Parse("{\"api_result\":null}");
            Assert.AreEqual(null, obj.api_result);
            Assert.AreEqual(null, obj["api_result"]);
        }

        [TestMethod]
        public void QueryAttribute()
        {
            var obj = (dynamic)JsonParser.Parse("{\"api_result\":null}");
            Assert.IsTrue(obj.api_result());
            Assert.IsTrue(obj.IsDefined("api_result"));
        }

        [TestMethod]
        public void ObjectWithNumber()
        {
            var obj = (dynamic)JsonParser.Parse("{\"api_result\":1}");
            Assert.AreEqual(1d, obj.api_result);
            Assert.AreEqual(1d, obj["api_result"]);
        }

        [TestMethod]
        public void ObjectWithString()
        {
            var obj = (dynamic)JsonParser.Parse("{\"api_result\":1,\"api_result_msg\":\"\u6210\u529f\"}");
            Assert.AreEqual("成功", obj.api_result_msg);
            Assert.AreEqual("成功", obj["api_result_msg"]);
        }

        [TestMethod]
        public void ArrayEmpty()
        {
            var obj = (dynamic)JsonParser.Parse("[]");
            Assert.IsTrue(obj.IsArray);
        }

        [TestMethod]
        public void ArrayWithPrimitives()
        {
            var bAry = (dynamic)JsonParser.Parse("[true,false,true]");
            Assert.IsTrue(bAry[0]);
            var dAry = (dynamic)JsonParser.Parse("[1,2,3]");
            Assert.AreEqual(dAry[0], 1d);
            var sAry = (dynamic)JsonParser.Parse("[\"1\", \"2\", \"3\"]");
            Assert.AreEqual(sAry[0], "1");
        }

        [TestMethod]
        public void WithWhiteSpace()
        {
            var obj = (dynamic)JsonParser.Parse("{\"api_result\":1, \"api_result_msg\" :\"\u6210\u529f\"}");
            Assert.AreEqual("成功", obj.api_result_msg);
            var ary = (double[])(dynamic)JsonParser.Parse("[1, 2, 3]");
            Assert.IsTrue(ary.SequenceEqual(new[] {1d, 2d, 3d}));
        }

        [TestMethod]
        public void EnumerateArray()
        {
            var ary = (dynamic)JsonParser.Parse("[{\"a\":1},{\"a\":2}]");
            var list = new List<double>();
            foreach (var entry in ary)
                list.Add(entry.a);
            Assert.IsTrue(list.SequenceEqual(new[] {1d, 2d}));
        }

        [TestMethod]
        public void ObjectWithArray()
        {
            var obj = (dynamic)JsonParser.Parse("{\"ary\":[]}");
            Assert.IsTrue(obj.ary.IsArray);
        }

        [TestMethod]
        public void EnumerateProperty()
        {
            var obj = (dynamic)JsonParser.Parse("{\"a\":{\"a\":1},\"b\":{\"a\":2}}");
            var list = new List<double>();
            foreach (KeyValuePair<string, dynamic> entry in obj)
                list.Add(entry.Value.a);
            Assert.IsTrue(list.SequenceEqual(new[] {1d, 2d}));
        }

        [TestMethod]
        public void PropertyOrder()
        {
            const int count = 100;
            var json = "{" + string.Join(",", Enumerable.Range(0, count).Select(n => $"\"{"a" + n}\":{n}")) + "}";
            var obj = (dynamic)JsonParser.Parse(json);
            var list = new List<int>();
            foreach (KeyValuePair<string, dynamic> kv in obj)
                list.Add((int)kv.Value);
            Assert.IsTrue(list.SequenceEqual(Enumerable.Range(0, count)));
        }

        [TestMethod]
        public void CastArrayToPrimitiveArray()
        {
            var bAry = (bool[])(dynamic)JsonParser.Parse("[true,false,true]");
            Assert.IsTrue(bAry.SequenceEqual(new[] {true, false, true}));
            var dAry = (double[])(dynamic)JsonParser.Parse("[1,2,3]");
            Assert.IsTrue(dAry.SequenceEqual(new[] {1d, 2d, 3d}));
            var sAry = (string[])(dynamic)JsonParser.Parse("[\"1\", \"2\", \"3\"]");
            Assert.IsTrue(sAry.SequenceEqual(new[] {"1", "2", "3"}));
        }

        [TestMethod]
        public void CastArrayToIntArray()
        {
            var iAry = (int[])(dynamic)JsonParser.Parse("[1,2,3]");
            Assert.IsTrue(iAry.SequenceEqual(new[] {1, 2, 3}));
        }

        [TestMethod]
        public void CastArrayToObjectArray()
        {
            var ary = (object[])(dynamic)JsonParser.Parse("[1,2,3]");
            Assert.IsTrue(ary.Cast<double>().SequenceEqual(new[] {1d, 2d, 3d}));
        }

        [TestMethod]
        public void CastArrayOfArrayToArrayOfIntArray()
        {
            var ary = (double[][])(dynamic)JsonParser.Parse("[[1,2],[3,4],[5,6]]");
            // ReSharper disable once PossibleNullReferenceException
            Assert.IsTrue(ary.GetType().GetElementType().IsArray);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void CastArrayToArrayOfIntArray()
        {
            Assert.IsTrue(((double[][])(dynamic)JsonParser.Parse("[1,2,3]")).GetType().IsArray);
        }

        // ReSharper disable UnusedVariable
        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void CastArrayToArrayOfFloat()
        {
            var dummy = (float[])(dynamic)JsonParser.Parse("[1,2,3]");
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void CastObjectToArray()
        {
            var dummy = (double[])(dynamic)JsonParser.Parse("{}");
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void CastArrayToDouble()
        {
            var dummy = (double)(dynamic)JsonParser.Parse("[1,2,3]");
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void CastBoolToEnumerable()
        {
            var dummy = (dynamic)JsonParser.Parse("false");
            foreach (var entry in dummy)
            {
            }
        }

        // ReSharper restore UnusedVariable

        [TestMethod]
        public void SetMember()
        {
            var obj = (dynamic)JsonParser.Parse("{}");
            obj.is_a = "hoge";
            Assert.AreEqual(obj.is_a, "hoge");
        }

        [TestMethod]
        public void SetMemberArray()
        {
            var obj = (dynamic)JsonParser.Parse("{}");
            var array = new[] {1, 2, 3, 4};
            obj.is_a = array;
            Assert.IsTrue(array.SequenceEqual((int[])obj.is_a));
        }

        [TestMethod]
        public void SetIndex()
        {
            var array = (dynamic)JsonParser.Parse("[1,2,3,4]");
            array[2] = 5;
            Assert.IsTrue(array[2] == 5);

            var obj = (dynamic)JsonParser.Parse("{}");
            obj["is_a"] = "hoge";
            Assert.AreEqual(obj.is_a, "hoge");
        }

        [TestMethod]
        public void CreateObject()
        {
            var json = (dynamic)JsonObject.CreateJsonObject(new {aaa = 1, bbb = new[] {1, 2, 3}});
            Assert.IsTrue(json.aaa == 1);
            Assert.IsTrue(json.bbb[1] == 2);
        }

        [TestMethod]
        public void CreateArray()
        {
            var json = (dynamic)JsonObject.CreateJsonObject(new[] {new {aaa = 1}, new {aaa = 2}});
            Assert.IsTrue(json[0].aaa == 1);
            Assert.IsTrue(json[1].aaa == 2);
        }

        [TestMethod]
        public void Serialize()
        {
            var input =
                "[{\"api_id\":1,\"api_name\":\"睦月\",\"api_maxeq\":[[0,1],0,0,0,0],\"api_test\":true,\"api_null\":null}," +
                "{\"api_id\":2,\"api_name\":\"如月\",\"api_maxeq\":[0,0,0,0,null],\"api_test\":false}]";
            var json = JsonParser.Parse(input);
            Assert.AreEqual(json.ToString(), input);
        }
    }
}