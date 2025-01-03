# Antigrav
[NuGet](https://www.nuget.org/packages/Antigrav) | [Source](https://github.com/antigrav-technologies/antigrav-cs)\
Serializing library based on JSON format, so it's also easy to read format.
## Types
- null
- string: specified in quotation marks (`""`)
- bool: true or false
- integer: sbyte or byte or short or ushort or int or uint or long or ulong or Int128 or UInt128
- Enum: encoded as number values
- floating point number: float or double or decimal
- Complex
- Dictionary<,>: specified in curly brackets (`{}`). Objects are encoded as dictionaries
- Arrays, ITuple, List<>: specified in square brackets (`[]`)\
Note that if you encode any ICollection, but because of electric caterpillars you can decode only types that are listed here
## Usage example

```c#
using Antigrav;

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
    [AntigravSerializable("value")] // warning, if you use fields then implement a set method.
    public Values? Value { get; private set; }
    [AntigravSerializable("suit")]
    public Suits? Suit { get; private set; }
    public override string ToString() => $"{Value} of {Suit}";
}

Card value = new(Values.Ace, Suits.Spades);

// encoding
string antigrav = AntigravConvert.DumpToString(value);
Console.WriteLine(antigrav); // "{\"value\": 1, \"suit\": 3}"

// decoding
Card decodedValue = AntigravConvert.LoadFromString<Card>(antigrav);
Console.WriteLine(decodedValue); // "Ace of Spades"
```
This was made as an experiment at first place, any tweaks in code are welcome. Don't ask why format is called so, the origin was extremely stupid
