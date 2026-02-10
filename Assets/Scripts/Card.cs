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

    public Suit suit { get; private set; }
    public Rank rank { get; private set; }

    public Card(Suit s, Rank r)
    {
        suit = s;
        rank = r;
    }

    public override string ToString()
    {
        return $"{rank} of {suit}";
    }

    public override bool Equals(object obj)
    {
        if (obj is Card other)
        {
            return suit == other.suit && rank == other.rank;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(suit, rank);
    }
}
