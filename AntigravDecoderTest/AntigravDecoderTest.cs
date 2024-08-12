using System.Numerics;

namespace AntigravDecoderTest;

[TestClass]
public class AntigravDecoderTest {
    [TestMethod]
    public void Decode_Sbyte() {
        string antigrav = "123b";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((sbyte)123, value);
    }

    [TestMethod]
    public void Decode_Byte() {
        string antigrav = "234B";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((byte)234, value);
    }

    [TestMethod]
    public void Decode_Short() {
        string antigrav = "4325s";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((short)4325, value);
    }

    [TestMethod]
    public void Decode_Ushort() {
        string antigrav = "43553S";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((ushort)43553, value);
    }

    [TestMethod]
    public void Decode_Integer() {
        string antigrav = "42";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void Decode_Uint() {
        string antigrav = "42I";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((uint)42, value);
    }

    [TestMethod]
    public void Decode_Long() {
        string antigrav = "243974364379348l";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(243974364379348, value);
    }

    [TestMethod]
    public void Decode_Ulong() {
        string antigrav = "243974364379348L";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((ulong)243974364379348, value);
    }

    [TestMethod]
    public void Decode_LLong() {
        string antigrav = "974364379348ll";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((Int128)974364379348, value);
    }

    [TestMethod]
    public void Decode_ULLong() {
        string antigrav = "974364379348LL";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual((UInt128)974364379348, value);
    }

    [TestMethod]
    public void Decode_Float() {
        string antigrav = "3.14F";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(3.14f, value);
    }

    [TestMethod]
    public void Decode_Double() {
        string antigrav = "3.14";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(3.14, value);
    }

    [TestMethod]
    public void Decode_Float1() {
        string antigrav = "1.0F";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(1.0f, value);
    }

    [TestMethod]
    public void Decode_Decimal() {
        string antigrav = "2.694102949283958052M";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(2.694102949283958052M, value);
    }

    [TestMethod]
    public void Decode_Decimal1() {
        string antigrav = "1.0M";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(1M, value);
    }

    [TestMethod]
    public void Decode_Null() {
        string antigrav = "null";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(null, value);
    }
    [TestMethod]
    public void Decode_Bool_True() {
        string antigrav = "true";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(true, value);
    }

    [TestMethod]
    public void Decode_Bool_False() {
        string antigrav = "false";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(false, value);
    }

    [TestMethod]
    public void Decode_Complex1_2() {
        string antigrav = "1.0+2.0i";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(new Complex(1.0, 2.0), value);
    }

    [TestMethod]
    public void Decode_ComplexPI_PHI() {
        string antigrav = "3.14+1.618i";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        Assert.AreEqual(new Complex(3.14, 1.618), value);
    }

    [TestMethod]
    public void Decode_List_Int() {
        string antigrav = "[1, 2, 3]";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        List<int> expected = [1, 2, 3];
        Assert.AreEqual(expected, value);
    }

    [TestMethod]
    public void Decode_List_String() {
        string antigrav = "[\"a\", \"b\", \"c\"]";
        object? value = Antigrav.Main.LoadFromString(antigrav);
        List<string> expected = ["a", "b", "c"];
        Assert.AreEqual(expected, value);
    }
/*
 * those below are not finished
    [TestMethod]
    public void Encode_Dictionary_String_String() {
        Dictionary<string, string> value = new() { { "a", "b" }, { "c", "d" } };
        string antigrav = Antigrav.Main.DumpToString(value);
        Assert.AreEqual("{\"a\": \"b\", \"c\": \"d\"}", antigrav);
    }

    [TestMethod]
    public void Encode_Dictionary_Int_String() {
        Dictionary<int, string> value = new() { { 1, "a" }, { 2, "b" } };
        string antigrav = Antigrav.Main.DumpToString(value);
        Assert.AreEqual("{\"1\": \"a\", \"2\": \"b\"}", antigrav);
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
        string antigrav = Antigrav.Main.DumpToString(value, sortKeys: false, indent: 4);
        Assert.AreEqual("{\n    \"string\": \"\\u0436\\u0438\\u0437\\u043d\\u044c \\u0438 \\u0441\\u043c\\u0435\\u0440\\u0442\\u044c \\u0432 scheel \\ud83e\\udd88\\ud83e\\udd88\\ud83e\\udd88\",\n    \"null\": null,\n    \"true\": true,\n    \"false\": false,\n    \"sbyte\": -73b,\n    \"byte\": 234B,\n    \"short\": -4892s,\n    \"ushort\": 4839S,\n    \"int\": 32,\n    \"uint\": 23I,\n    \"long\": 2348429482858735l,\n    \"ulong\": 3287534753486978L,\n    \"Int128\": 21437492358347ll,\n    \"UInt128\": 248073232487LL,\n    \"float\": 3.14F,\n    \"double\": 3.14,\n    \"decimal\": 3.14M,\n    \"\\u0441omplex\": 3.142+84.0i,\n    \"list\": [\n        3s,\n        4s,\n        5s\n    ],\n    \"empty list\": [],\n    \"dict\": {\n        \"1\": 3M,\n        \"2\": 31.45M\n    },\n    \"empty dict\": {}\n}",
            antigrav);
    }*/
}