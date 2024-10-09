using System.Numerics;
using System.Reflection;
using Antigrav;
using static Antigrav.AntigravConvert;

namespace EncoderTest;

[TestClass]
public class AntigravEncoderTest {
    [TestMethod]
    public void Encode_Sbyte() {
        sbyte value = 123;
        string antigrav = DumpToString(value);
        Assert.AreEqual("123b", antigrav);
    }

    [TestMethod]
    public void Encode_Byte() {
        byte value = 234;
        string antigrav = DumpToString(value);
        Assert.AreEqual("234B", antigrav);
    }

    [TestMethod]
    public void Encode_Short() {
        short value = 4325;
        string antigrav = DumpToString(value);
        Assert.AreEqual("4325s", antigrav);
    }

    [TestMethod]
    public void Encode_Ushort() {
        ushort value = 43553;
        string antigrav = DumpToString(value);
        Assert.AreEqual("43553S", antigrav);
    }

    [TestMethod]
    public void Encode_Integer() {
        int value = 42;
        string antigrav = DumpToString(value);
        Assert.AreEqual("42", antigrav);
    }

    [TestMethod]
    public void Encode_Uint() {
        uint value = 42;
        string antigrav = DumpToString(value);
        Assert.AreEqual("42I", antigrav);
    }

    [TestMethod]
    public void Encode_Long() {
        long value = 243974364379348;
        string antigrav = DumpToString(value);
        Assert.AreEqual("243974364379348l", antigrav);
    }

    [TestMethod]
    public void Encode_Ulong() {
        ulong value = 243974364379348;
        string antigrav = DumpToString(value);
        Assert.AreEqual("243974364379348L", antigrav);
    }

    [TestMethod]
    public void Encode_LLong() {
        Int128 value = 974364379348;
        string antigrav = DumpToString(value);
        Assert.AreEqual("974364379348ll", antigrav);
    }

    [TestMethod]
    public void Encode_ULLong() {
        UInt128 value = 974364379348;
        string antigrav = DumpToString(value);
        Assert.AreEqual("974364379348LL", antigrav);
    }

    [TestMethod]
    public void Encode_Float() {
        float value = 3.14f;
        string antigrav = DumpToString(value);
        Assert.AreEqual("3.14F", antigrav);
    }

    [TestMethod]
    public void Encode_Double() {
        double value = 3.14;
        string antigrav = DumpToString(value);
        Assert.AreEqual("3.14", antigrav);
    }

    [TestMethod]
    public void Encode_Float1() {
        float value = 1.0f;
        string antigrav = DumpToString(value);
        Assert.AreEqual("1.0F", antigrav);
    }

    [TestMethod]
    public void Encode_Decimal() {
        decimal value = 2.694102949283958052M;
        string antigrav = DumpToString(value);
        Assert.AreEqual("2.694102949283958052M", antigrav);
    }

    [TestMethod]
    public void Encode_Decimal1() {
        decimal value = 1M;
        string antigrav = DumpToString(value);
        Assert.AreEqual("1.0M", antigrav);
    }

    [TestMethod]
    public void Encode_Null() {
        object? value = null;
        string antigrav = DumpToString(value);
        Assert.AreEqual("null", antigrav);
    }
    [TestMethod]
    public void Encode_Bool_True() {
        bool value = true;
        string antigrav = DumpToString(value);
        Assert.AreEqual("true", antigrav);
    }

    [TestMethod]
    public void Encode_Bool_False() {
        bool value = false;
        string antigrav = DumpToString(value);
        Assert.AreEqual("false", antigrav);
    }

    [TestMethod]
    public void Encode_Complex1_2() {
        Complex value = new(1.0, 2.0);
        string antigrav = DumpToString(value);
        Assert.AreEqual("1.0+2.0i", antigrav);
    }

    [TestMethod]
    public void Encode_ComplexPI_PHI() {
        Complex value = new(3.14, 1.618);
        string antigrav = DumpToString(value);
        Assert.AreEqual("3.14+1.618i", antigrav);
    }

    [TestMethod]
    public void Encode_List_Int() {
        List<int> value = [1, 2, 3];
        string antigrav = DumpToString(value);
        Assert.AreEqual("[1, 2, 3]", antigrav);
    }

    [TestMethod]
    public void Encode_List_String() {
        List<string> value = ["a", "b", "c"];
        string antigrav = DumpToString(value);
        Assert.AreEqual("[\"a\", \"b\", \"c\"]", antigrav);
    }

    [TestMethod]
    public void Encode_Dictionary_String_String() {
        Dictionary<string, string> value = new() { { "a", "b" }, { "c", "d" } };
        string antigrav = DumpToString(value);
        Assert.AreEqual("{\"a\": \"b\", \"c\": \"d\"}", antigrav);
    }

    [TestMethod]
    public void Encode_Dictionary_Int_String() {
        Dictionary<int, string> value = new() { { 1, "a" }, { 2, "b" } };
        string antigrav = DumpToString(value);
        Assert.AreEqual("{1: \"a\", 2: \"b\"}", antigrav);
    }

    [TestMethod]
    public void Encode_Dictionary_Everything() {
        List<object> empty_list = [];
        List<short> list = [3, 4, 5];
        Dictionary<object, object> empty_dict = [];
        Dictionary<string, decimal> dict = new() { { "1", 3M }, { "2", 31.45M } };
        Dictionary<string, object?> value = new() {
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
        string antigrav = DumpToString(value, sortKeys: false, indent: 4);
        Assert.AreEqual("{\n    \"string\": \"\\u0436\\u0438\\u0437\\u043d\\u044c \\u0438 \\u0441\\u043c\\u0435\\u0440\\u0442\\u044c \\u0432 scheel \\U0001f988\\U0001f988\\U0001f988\",\n    \"null\": null,\n    \"true\": true,\n    \"false\": false,\n    \"sbyte\": -73b,\n    \"byte\": 234B,\n    \"short\": -4892s,\n    \"ushort\": 4839S,\n    \"int\": 32,\n    \"uint\": 23I,\n    \"long\": 2348429482858735l,\n    \"ulong\": 3287534753486978L,\n    \"Int128\": 21437492358347ll,\n    \"UInt128\": 248073232487LL,\n    \"float\": 3.14F,\n    \"double\": 3.14,\n    \"decimal\": 3.14M,\n    \"\\u0441omplex\": 3.142+84.0i,\n    \"list\": [\n        3s,\n        4s,\n        5s\n    ],\n    \"empty list\": [],\n    \"dict\": {\n        \"1\": 3.0M,\n        \"2\": 31.45M\n    },\n    \"empty dict\": {}\n}",
            antigrav);
    }

    // due the laziness i just stole head first c# code
    private enum Values {
        Ace = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
    }
    private enum Suits {
        Diamonds,
        Clubs,
        Hearts,
        Spades,
    }
    private class Card {
        public Card() {
            Value = null;
            Suit = null;
        }
        public Card(Values value, Suits suit) {
            Value = value;
            Suit = suit;
        }
        [AntigravSerializable("value")]
        public Values? Value { get; private set; }
        [AntigravSerializable("suit")]
        public Suits? Suit { get; private set; }
        public override string ToString() => $"{Value} of {Suit}";
    }
    [TestMethod]
    public void Encode_Object1() {
        Card value = new(Values.Ace, Suits.Spades);
        string antigrav = DumpToString(value);
        Assert.AreEqual("{\"value\": 1, \"suit\": 3}", antigrav);
    }
    private class ExtensionDataTestClass {
        [AntigravSerializable]
        public Card Card1 = new(Values.Ace, Suits.Spades);
        [AntigravSerializable("card name or not really idk")]
        public Card Card2 { get; private set; } = new Card(Values.Seven, Suits.Diamonds);
        [AntigravExtensionData]
        public Dictionary<string, int> extensionData = new() { { "a", 2 }, { "b", 314 } };
    }
    [TestMethod]
    public void Encode_ObjectWithExtensionData() {
        ExtensionDataTestClass value = new();
        string antigrav = DumpToString(value);
        Assert.AreEqual("{\"card name or not really idk\": {\"value\": 7, \"suit\": 0}, \"Card1\": {\"value\": 1, \"suit\": 3}, \"a\": 2, \"b\": 314}", antigrav);
    }
    [TestMethod]
    public void Encode_TuplesList() {
        List<Tuple<int, int>> value = [new Tuple<int, int>(12, 34), new Tuple<int, int>(34, 45)];
        string antigrav = DumpToString(value);
        Assert.AreEqual("[[12, 34], [34, 45]]", antigrav);
    }

    [TestMethod]
    public void Encode_StringWithEscape() {
        string value = "@everyone у вас спина #FFFFFF 🚜🚜🚜\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f\x20\x30\x40";
        string antigrav = DumpToString(value);
        Assert.AreEqual("\"@everyone \\u0443 \\u0432\\u0430\\u0441 \\u0441\\u043f\\u0438\\u043d\\u0430 #FFFFFF \\U0001f69c\\U0001f69c\\U0001f69c\\0\\x01\\x02\\x03\\x04\\x05\\x06\\a\\b\\t\\n\\v\\f\\r\\x0e\\x0f\\x10\\x11\\x12\\x13\\x14\\x15\\x16\\x17\\x18\\x19\\x1a\\x1b\\x1c\\x1d\\x1e\\x1f 0@\"", antigrav);
    }

    [TestMethod]
    public void Encode_SortedEnumDict() {
        Dictionary<Values, long> value = new() { { Values.Jack, 3 }, { Values.Ace, 6 }, { Values.Six, -634 } };
        string antigrav = DumpToString(value, sortKeys: true, indent: 4);
        Assert.AreEqual("{\n    1: 6l,\n    6: -634l,\n    11: 3l\n}", antigrav);
    }
    /*
    private class Test() {
        [AntigravSerializable]
        public Test? test;
    }
    [TestMethod]
    public void TheMostInformativeVideo() {
        var a = new Test();
        var b = new Test();
        a.test = b;
        b.test = a;
        DumpToString(a);
        Assert.Fail("stack overflow failed");
    }*/
}