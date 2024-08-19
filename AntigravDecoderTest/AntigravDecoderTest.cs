using System.Numerics;

namespace DecoderTest;

[TestClass]
public class AntigravDecoderTest {
    [TestMethod]
    public void Decode_Sbyte() {
        string antigrav = "123b";
        sbyte value = Antigrav.Main.LoadFromString<sbyte>(antigrav);
        Assert.AreEqual((sbyte)123, value);
    }

    [TestMethod]
    public void Decode_Byte() {
        string antigrav = "234B";
        byte value = Antigrav.Main.LoadFromString<byte>(antigrav);
        Assert.AreEqual((byte)234, value);
    }

    [TestMethod]
    public void Decode_Short() {
        string antigrav = "4325s";
        short value = Antigrav.Main.LoadFromString<short>(antigrav);
        Assert.AreEqual((short)4325, value);
    }

    [TestMethod]
    public void Decode_Ushort() {
        string antigrav = "43553S";
        ushort value = Antigrav.Main.LoadFromString<ushort>(antigrav);
        Assert.AreEqual((ushort)43553, value);
    }

    [TestMethod]
    public void Decode_Integer() {
        string antigrav = "42";
        int value = Antigrav.Main.LoadFromString<int>(antigrav);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void Decode_Uint() {
        string antigrav = "42I";
        uint value = Antigrav.Main.LoadFromString<uint>(antigrav);
        Assert.AreEqual((uint)42, value);
    }

    [TestMethod]
    public void Decode_Long() {
        string antigrav = "243974364379348l";
        long value = Antigrav.Main.LoadFromString<long>(antigrav);
        Assert.AreEqual(243974364379348, value);
    }

    [TestMethod]
    public void Decode_Ulong() {
        string antigrav = "243974364379348L";
        ulong value = Antigrav.Main.LoadFromString<ulong>(antigrav);
        Assert.AreEqual((ulong)243974364379348, value);
    }

    [TestMethod]
    public void Decode_LLong() {
        string antigrav = "974364379348ll";
        Int128 value = Antigrav.Main.LoadFromString<Int128>(antigrav);
        Assert.AreEqual((Int128)974364379348, value);
    }

    [TestMethod]
    public void Decode_ULLong() {
        string antigrav = "974364379348LL";
        UInt128 value = Antigrav.Main.LoadFromString<UInt128>(antigrav);
        Assert.AreEqual((UInt128)974364379348, value);
    }

    [TestMethod]
    public void Decode_Float() {
        string antigrav = "3.14F";
        float value = Antigrav.Main.LoadFromString<float>(antigrav);
        Assert.AreEqual(3.14f, value);
    }

    [TestMethod]
    public void Decode_Double() {
        string antigrav = "3.14";
        double value = Antigrav.Main.LoadFromString<double>(antigrav);
        Assert.AreEqual(3.14, value);
    }

    [TestMethod]
    public void Decode_Float1() {
        string antigrav = "1.0F";
        float value = Antigrav.Main.LoadFromString<float>(antigrav);
        Assert.AreEqual(1.0f, value);
    }

    [TestMethod]
    public void Decode_Decimal() {
        string antigrav = "2.694102949283958052M";
        decimal value = Antigrav.Main.LoadFromString<decimal>(antigrav);
        Assert.AreEqual(2.694102949283958052M, value);
    }

    [TestMethod]
    public void Decode_Decimal1() {
        string antigrav = "1.0M";
        decimal value = Antigrav.Main.LoadFromString<decimal>(antigrav);
        Assert.AreEqual(1M, value);
    }

    [TestMethod]
    public void Decode_Null() {
        string antigrav = "null";
        object? value = Antigrav.Main.LoadFromString<object?>(antigrav);
        Assert.AreEqual(null, value);
    }
    [TestMethod]
    public void Decode_Bool_True() {
        string antigrav = "true";
        bool value = Antigrav.Main.LoadFromString<bool>(antigrav);
        Assert.AreEqual(true, value);
    }

    [TestMethod]
    public void Decode_Bool_False() {
        string antigrav = "false";
        bool value = Antigrav.Main.LoadFromString<bool>(antigrav);
        Assert.AreEqual(false, value);
    }

    [TestMethod]
    public void Decode_Complex1_2() {
        string antigrav = "1.0+2.0i";
        Complex value = Antigrav.Main.LoadFromString<Complex>(antigrav);
        Assert.AreEqual(new Complex(1.0, 2.0), value);
    }

    [TestMethod]
    public void Decode_ComplexPI_PHI() {
        string antigrav = "3.14+1.618i";
        Complex value = Antigrav.Main.LoadFromString<Complex>(antigrav);
        Assert.AreEqual(new Complex(3.14, 1.618), value);
    }

    [TestMethod]
    public void Decode_List_Int() {
        string antigrav = "[1, 2, 3]";
        List<int> value = Antigrav.Main.LoadFromString<List<int>>(antigrav)!;
        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, value);
    }

    [TestMethod]
    public void Decode_List_String() {
        string antigrav = "[\"a\", \"b\", \"c\"]";
        List<string> value = Antigrav.Main.LoadFromString<List<string>>(antigrav)!;
        CollectionAssert.AreEqual(new List<string> { "a", "b", "c"}, value);
    }
    
    [TestMethod]
    public void Decode_Dictionary_String_String() {
        string antigrav = "{\"a\": \"b\", \"c\": \"d\"}";
        Dictionary<string, string> value = Antigrav.Main.LoadFromString<Dictionary<string, string>>(antigrav)!;
        CollectionAssert.AreEqual(new Dictionary<string, string> { { "a", "b" }, { "c", "d" } }, value);
    }
    
    [TestMethod]
    public void Encode_Dictionary_Int_String() {
        string antigrav = "{\"1\": \"a\", \"2\": \"b\"}";
        Dictionary<int, string> value = Antigrav.Main.LoadFromString<Dictionary<int, string>>(antigrav)!;
        CollectionAssert.AreEqual(new Dictionary<int, string> { { 1, "a" }, { 2, "b" } }, value);
    }

    [TestMethod]
    public void Decode_Dictionary_Everything() {
        string antigrav = "{\n    \"string\": \"\\u0436\\u0438\\u0437\\u043d\\u044c \\u0438 \\u0441\\u043c\\u0435\\u0440\\u0442\\u044c \\u0432 scheel \\U0001f988\\U0001f988\\U0001f988\",\n    \"null\": null,\n    \"true\": true,\n    \"false\": false,\n    \"sbyte\": -73b,\n    \"byte\": 234B,\n    \"short\": -4892s,\n    \"ushort\": 4839S,\n    \"int\": 32,\n    \"uint\": 23I,\n    \"long\": 2348429482858735l,\n    \"ulong\": 3287534753486978L,\n    \"Int128\": 21437492358347ll,\n    \"UInt128\": 248073232487LL,\n    \"float\": 3.14F,\n    \"double\": 3.14,\n    \"decimal\": 3.14M,\n    \"\\u0441omplex\": 3.142+84.0i,\n    \"list\": [\n        3s,\n        4s,\n        5s\n    ],\n    \"empty list\": [],\n    \"dict\": {\n        \"1\": 3.0M,\n        \"2\": 31.45M\n    },\n    \"empty dict\": {}\n}";

        // apparently because of goddamn pointers CollectionAssert was a bushes so just take the print thing

        Dictionary<string, object?> value = Antigrav.Main.LoadFromString<Dictionary<string, object?>>(antigrav)!;
        Console.WriteLine("{");
        foreach (KeyValuePair<string, object?> kwp in value) {
            Console.WriteLine($"    {kwp.Key}: {(kwp.Value is List<object> l ? '[' + string.Join(", ", l) + ']' : kwp.Value is Dictionary<object, object> d ? '{' + string.Join(": ", d.Select(kwp => $"{kwp.Key}: {kwp.Value}")) + '}' : kwp.Value)}");
        }
        Console.WriteLine("}");
    }
}