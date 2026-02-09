using System;

public class Card
{
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    public enum Rank
    {
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }
    
    public Suit CardSuit { get; private set; }
    public Rank CardRank { get; private set; }

    public Card(Suit suit, Rank rank)
    {
        CardSuit = suit;
        CardRank = rank;
    }

    public override string ToString()
    {
        return $"{CardRank} of {CardSuit}";
    }

    public override bool Equals(object obj)
    {
        if (obj is Card other)
        {
            return CardSuit == other.CardSuit && CardRank == other.CardRank;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CardSuit, CardRank);
    }
}
