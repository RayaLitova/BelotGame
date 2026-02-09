using System;
using System.Collections.Generic;
using System.Linq;

public class Player
{
    public string Name { get; private set; }
    public List<Card> Hand { get; private set; }
    public Player Teammate { get; set; }
    
    public Player(string name, Player teammate = null)
    {
        Name = name;
        Hand = new List<Card>();
        Teammate = teammate;
    }

    public void AddCard(Card card)
    {
        if (card != null)
        {
            Hand.Add(card);
        }
    }

    public void AddCards(IEnumerable<Card> cards)
    {
        Hand.AddRange(cards);
    }

    public bool RemoveCard(Card card)
    {
        return Hand.Remove(card);
    }

    public Card PlayCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < Hand.Count)
        {
            Card card = Hand[cardIndex];
            Hand.RemoveAt(cardIndex);
            return card;
        }
        return null;
    }

    public Card PlayCard(Card card)
    {
        if (RemoveCard(card))
        {
            return card;
        }
        return null;
    }

    public int GetHandCount()
    {
        return Hand.Count;
    }

    public void ClearHand()
    {
        Hand.Clear();
    }

    public override string ToString()
    {
        return $"{Name} - Teammate: {Teammate.Name}, Cards in hand: {Hand.Count}";
    }
}
