using System.Numerics;

namespace EncoderTest {
    [TestClass]
    public class AntigravEncoderTest {
        [TestMethod]
        public void Encode_Sbyte() {
            sbyte value = 123;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("123b", antigrav);
        }

        [TestMethod]
        public void Encode_Byte() {
            byte value = 234;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("234B", antigrav);
        }

        [TestMethod]
        public void Encode_Short() {
            short value = 4325;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("4325s", antigrav);
        }

        [TestMethod]
        public void Encode_Ushort() {
            ushort value = 43553;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("43553S", antigrav);
        }

        [TestMethod]
        public void Encode_Integer() {
            int value = 42;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("42", antigrav);
        }

        [TestMethod]
        public void Encode_Uint() {
            uint value = 42;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("42I", antigrav);
        }

        [TestMethod]
        public void Encode_Long() {
            long value = 243974364379348;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("243974364379348l", antigrav);
        }

        [TestMethod]
        public void Encode_Ulong() {
            ulong value = 243974364379348;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("243974364379348L", antigrav);
        }

        [TestMethod]
        public void Encode_LLong() {
            Int128 value = 974364379348;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("974364379348ll", antigrav);
        }

        [TestMethod]
        public void Encode_ULLong() {
            UInt128 value = 974364379348;
            string antigrav = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("974364379348LL", antigrav);
        }

        [TestMethod]
        public void Encode_Float() {
            float value = 3.14f;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("3.14F", json);
        }

        [TestMethod]
        public void Encode_Double() {
            double value = 3.14;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("3.14", json);
        }

        [TestMethod]
        public void Encode_Float1() {
            float value = 1.0f;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("1F", json);
        }

        [TestMethod]
        public void Encode_Decimal() {
            decimal value = 2.694102949283958052M;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("2.694102949283958052M", json);
        }

        [TestMethod]
        public void Encode_Decimal1() {
            decimal value = 1M;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("1M", json);
        }

        [TestMethod]
        public void Encode_Null() {
            object? value = null;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("null", json);
        }
        [TestMethod]
        public void Encode_Bool_True() {
            bool value = true;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("true", json);
        }

        [TestMethod]
        public void Encode_Bool_False() {
            bool value = false;
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("false", json);
        }

        [TestMethod]
        public void Encode_Complex1_2() {
            Complex value = new Complex(1.0, 2.0);
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("1+2i", json);
        }

        [TestMethod]
        public void Encode_ComplexPI_PHI() {
            Complex value = new Complex(3.14, 1.618);
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("3.14+1.618i", json);
        }

        [TestMethod]
        public void Encode_List_Int() {
            List<int> value = new List<int> { 1, 2, 3 };
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("[1, 2, 3]", json);
        }

        [TestMethod]
        public void Encode_List_String() {
            List<string> value = new List<string> { "a", "b", "c" };
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("[\"a\", \"b\", \"c\"]", json);
        }

        [TestMethod]
        public void Encode_Dictionary_String_String() {
            Dictionary<string, string> value = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } };
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("{\"a\": \"b\", \"c\": \"d\"}", json);
        }

        [TestMethod]
        public void Encode_Dictionary_Int_String() {
            Dictionary<int, string> value = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
            string json = Antigrav.Main.DumpToString(value);
            Assert.AreEqual("{\"1\": \"a\", \"2\": \"b\"}", json);
        }

        [TestMethod]
        public void Encode_Dictionary_Everything() {
            List<object> empty_list = [];
            List<short> list = [3, 4, 5];
            Dictionary<object, object> empty_dict = new Dictionary<object, object>();
            Dictionary<string, decimal> dict = new Dictionary<string, decimal> { { "1", 3M }, { "2", 31.45M } };
            Dictionary<string, object?> value = new Dictionary<string, object?> {
                {"string", "жизнь и смерть в scheel 🦈🦈🦈"},
                {"null", null},
                {"true", true},
                {"false", false},
                {"sbyte", (sbyte)-73},
                {"byte", (byte)234},
                {"short", (short)-4892},
                {"ushort", (ushort)4839},
                {"int", 32},
                {"uint", (uint)23},
                {"long", 2348429482858735},
                {"ulong", (ulong)3287534753486978},
                {"Int128", (Int128)21437492358347},
                {"UInt128", (UInt128)248073232487},
                {"float", 3.14f},
                {"double", 3.14},
                {"decimal", 3.14M},
                {"сomplex", new Complex(3.142, 84)}, // russian с here btw
                {"list", list},
                {"empty list", empty_list},
                {"dict", dict},
                {"empty dict", empty_dict}
            };
            string antigrav = Antigrav.Main.DumpToString(value, sortKeys: true, indent: 4);
            Assert.AreEqual("{\n    \"string\": \"\\u0436\\u0438\\u0437\\u043d\\u044c \\u0438 \\u0441\\u043c\\u0435\\u0440\\u0442\\u044c \\u0432 scheel \\ud83e\\udd88\\ud83e\\udd88\\ud83e\\udd88\",\n    \"null\": null,\n    \"true\": true,\n    \"false\": false,\n    \"sbyte\": -73b,\n    \"byte\": 234B,\n    \"short\": -4892s,\n    \"ushort\": 4839S,\n    \"int\": 32,\n    \"uint\": 23I,\n    \"long\": 2348429482858735l,\n    \"ulong\": 3287534753486978L,\n    \"Int128\": 21437492358347ll,\n    \"UInt128\": 248073232487LL,\n    \"float\": 3.14F,\n    \"double\": 3.14,\n    \"decimal\": 3.14M,\n    \"\\u0441omplex\": 3.142+84i,\n    \"list\": [\n        3s,\n        4s,\n        5s\n    ],\n    \"empty list\": [],\n    \"dict\": {\n        \"1\": 3M,\n        \"2\": 31.45M\n    },\n    \"empty dict\": {}\n}",
                antigrav);
        }
    }
}