using System;
using System.Collections.Generic;
using System.Linq;

public class Dealer
{
    private List<Card> Deck;

    public Dealer()
    {
        Deck = new List<Card>();
        InitializeDeck();
    }

    private void InitializeDeck()
    {
        Deck.Clear();

        Card.Suit[] suits = { Card.Suit.Clubs, Card.Suit.Diamonds, Card.Suit.Hearts, Card.Suit.Spades };
        Card.Rank[] ranks = { Card.Rank.Seven, Card.Rank.Eight, Card.Rank.Nine, Card.Rank.Ten, 
                              Card.Rank.Jack, Card.Rank.Queen, Card.Rank.King, Card.Rank.Ace };

        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                Deck.Add(new Card(suit, rank));
            }
        }
    }

    public void ShuffleDeck()
    {
        Random random = new Random();
        for (int i = Deck.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            (Deck[randomIndex], Deck[i]) = (Deck[i], Deck[randomIndex]);
        }
    }

    public void ResetDeck()
    {
        InitializeDeck();
    }

    public Card DealCard()
    {
        if (Deck.Count > 0)
        {
            Card card = Deck[0];
            Deck.RemoveAt(0);
            return card;
        }
        return null;
    }

    public void DealCards(Player player, int cardCount)
    {
        for (int i = 0; i < cardCount && Deck.Count > 0; i++)
        {
            player.AddCard(DealCard());
        }
    }

    public void DealInitialCards(List<Player> players)
    {
        foreach (var player in players)
        {
            player.ClearHand();
        }
        DealToMultiplePlayers(players, 3);
        DealToMultiplePlayers(players, 2);
    }

    public void DealSecondRoundCards(List<Player> players)
    {
        DealToMultiplePlayers(players, 3);
    }

    public void DealToMultiplePlayers(List<Player> players, int cardsPerPlayer)
    {
        foreach (var player in players)
        {
            DealCards(player, cardsPerPlayer);
        }
    }

    public int GetRemainingCards()
    {
        return Deck.Count;
    }

    public bool HasCards()
    {
        return Deck.Count > 0;
    }

    public override string ToString()
    {
        return $"Dealer - Cards remaining in Deck: {Deck.Count}";
    }
}
