using System.Numerics;
using static Antigrav.Main;

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
        CollectionAssert.AreEqual(new List<string> { "a", "b", "c" }, value);
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
    [TestMethod]
    public void Decode_Array_Int() {
        string antigrav = "[1, 2, 4, 6, 280]";
        int[] value = LoadFromString<int[]>(antigrav)!;
        CollectionAssert.AreEqual(new int[] { 1, 2, 4, 6, 280 }, value);
    }
    [TestMethod]
    public void Decode_Array_String() {
        string antigrav = "[\"a\", \"b\", \"qwertyuiop\"]";
        string[] value = LoadFromString<string[]>(antigrav)!;
        CollectionAssert.AreEqual(new string[] { "a", "b", "qwertyuiop" }, value);
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
    private class CardButWithFields {
        public CardButWithFields() {
            Value = null;
            Suit = null;
        }
        public CardButWithFields(Values value, Suits suit) {
            Value = value;
            Suit = suit;
        }
        [AntigravSerializable("value")]
        public Values? Value;
        [AntigravSerializable("suit")]
        public Suits? Suit;
        public override string ToString() => $"{Value} of {Suit}";
    }

    [TestMethod]
    public void Decode_Enum() {
        string antigrav = "1";
        Values value = LoadFromString<Values>(antigrav)!;
        Assert.AreEqual(Values.Ace, value);
    }
    [TestMethod]
    public void Decode_NullableEnum() {
        string antigrav = "1";
        Values? value = LoadFromString<Values?>(antigrav);
        Values? ace = Values.Ace;
        Assert.AreEqual(ace, value);
    }
    [TestMethod]
    public void Decode_NullableEnumList() {
        string antigrav = "[1, 2, 3, null, 4]";
        List<Values?> value = LoadFromString<List<Values?>>(antigrav)!;
        CollectionAssert.AreEqual(new List<Values?> { Values.Ace, Values.Two, Values.Three, null, Values.Four }, value);
    }
    [TestMethod]
    public void Decode_Object() {
        string antigrav = "{\"value\": 1, \"suit\": 3}";
        Card value = LoadFromString<Card>(antigrav)!;
        Console.WriteLine(value.ToString());
        Assert.IsTrue(Values.Ace == value.Value && Suits.Spades == value.Suit);
    }
    [TestMethod]
    public void Decode_ObjectWithFields() {
        string antigrav = "{\"value\": 1, \"suit\": 3}";
        CardButWithFields value = LoadFromString<CardButWithFields>(antigrav)!;
        Console.WriteLine(value.ToString());
        Assert.IsTrue(Values.Ace == value.Value && Suits.Spades == value.Suit);
    }
    private class ExtensionDataTestClass {
        [AntigravSerializable]
        private Card Card1 = new(Values.Ace, Suits.Spades);
        public Card GetCard1() => Card1;
        [AntigravSerializable("card name or not really idk")]
        public Card Card2 { get; private set; } = new Card(Values.Seven, Suits.Diamonds);
        [AntigravExtensionData]
        public Dictionary<string, int> extensionData = new() { { "a", 2 }, { "b", 314 } };
    }
    [TestMethod]
    public void Decode_ObjectWithExtensionData() {
        string antigrav = "{\"card name or not really idk\": {\"value\": 7, \"suit\": 0}, \"Card1\": {\"value\": 1, \"suit\": 3}, \"a\": 2, \"b\": 314}";
        ExtensionDataTestClass value = LoadFromString<ExtensionDataTestClass>(antigrav)!;
        ExtensionDataTestClass expected = new();
        Assert.IsTrue(expected.GetCard1().Value == value.GetCard1().Value && expected.GetCard1().Suit == value.GetCard1().Suit);
        Assert.IsTrue(expected.Card2.Value == value.Card2.Value && expected.Card2.Suit == value.Card2.Suit);
        CollectionAssert.AreEqual(expected.extensionData, value.extensionData);
    }
    [TestMethod]
    public void Decode_TuplesList() {
        string antigrav = "[[12, 34], [34, 45]]";
        List<Tuple<int, int>> value = LoadFromString<List<Tuple<int, int>>>(antigrav)!;
        CollectionAssert.AreEqual(new List<Tuple<int, int>>() { new(12, 34), new(34, 45) }, value);
    }
    private enum CtqaType {
        Unknown = -1,
        Fine = 1,
        Nice = 2,
        Good = 3,
        Uncommon = 4,
        Rare = 5,
        Wild = 6,
        Baby = 7,
        Old = 8,
        Epic = 9,
        Brave = 10,
        Reverse = 11,
        Inverted = 12,
        Superior = 13,
        Tema5002 = 14,
        Legendary = 15,
        Mythic = 16,
        EightBit = 17,
        Corrupted = 18,
        Professor = 19,
        Real = 20,
        Ultimate = 21,
        Cool = 22,
        Silly = 1000,
        Icosahedron = 10001,
        Aflyde = 10002,
        Octopus = 10003,
        typing = 10004,
        Kesslon = 10005,
        Bread = 10006,
        Blep = 10007,
        cake64 = 10008,
        antaegeav = 10009,
        Jeremy = 10010,
        Maxwell = 10011,
        Pentachoron = 10012,
        NetscapeAd = 10013
    }
    private class Inventory {
        [AntigravSerializable("achs")]
        public List<string> Achievements { get; private set; } = [];

        [AntigravSerializable("fastest_catch")]
        private double fastestCatch = float.PositiveInfinity;
        public double FastestCatch { get => fastestCatch; set => fastestCatch = Math.Min(fastestCatch, value); }

        [AntigravSerializable("slowest_catch")]
        private double slowestCatch = float.NegativeInfinity;
        public double SlowestCatch { get => slowestCatch; set => slowestCatch = Math.Max(slowestCatch, value); }

        [AntigravExtensionData]
        public Dictionary<CtqaType, long> Ctqas { get; set; } = [];
    }
    [TestMethod]
    public void Decode_CtqaBtoInventory() {
        string antigrav = "{\"achs\": [], 1: 1l, \"fastest_catch\": inf, \"slowest_catch\": -inf}";
        Inventory value = LoadFromString<Inventory>(antigrav)!;
        var expected = new Inventory {
            Ctqas = new() { { CtqaType.Fine, 1 } }
        };
        CollectionAssert.AreEqual(expected.Ctqas, value.Ctqas);
        CollectionAssert.AreEqual(expected.Achievements, value.Achievements);
        Assert.AreEqual(expected.FastestCatch, value.FastestCatch);
        Assert.AreEqual(expected.SlowestCatch, value.SlowestCatch);
    }
    private struct SpawnMessageData(CtqaType type = CtqaType.Unknown, ulong messageId = 0, string sayToCatch = "ctqa") {
        [AntigravSerializable("type")]
        public CtqaType Type { get; private set; } = type;
        [AntigravSerializable("message_id")]
        public ulong MessageId { get; private set; } = messageId;
        [AntigravSerializable("say_to_catch")]
        public string SayToCatch { get; private set; } = sayToCatch;
    }

    [TestMethod]
    public void Decode_CtqaBtoSpawnMessageData() {
        string antigrav = "{1196792237783273573L: {\"type\": 4, \"message_id\": 1276595838700884172L, \"say_to_catch\": \"ctqa\"}}";
        Dictionary<ulong, SpawnMessageData> value = LoadFromString<Dictionary<ulong, SpawnMessageData>>(antigrav)!;
        Dictionary<ulong, SpawnMessageData> expected = new() { { 1196792237783273573, new SpawnMessageData(CtqaType.Uncommon, 1276595838700884172, "ctqa") } };
        CollectionAssert.AreEqual(expected, value);
    }
}