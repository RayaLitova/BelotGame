using System;
using System.Collections.Generic;
using System.Linq;

public static class BelotRules
{
    private static readonly Dictionary<Card.Rank, int> NonTrumpPoints = new Dictionary<Card.Rank, int>
    {
        { Card.Rank.Ace, 11 },
        { Card.Rank.Ten, 10 },
        { Card.Rank.King, 4 },
        { Card.Rank.Queen, 3 },
        { Card.Rank.Jack, 2 },
        { Card.Rank.Nine, 0 },
        { Card.Rank.Eight, 0 },
        { Card.Rank.Seven, 0 }
    };

    private static readonly Dictionary<Card.Rank, int> TrumpPoints = new Dictionary<Card.Rank, int>
    {
        { Card.Rank.Jack, 20 },
        { Card.Rank.Nine, 14 },
        { Card.Rank.Ace, 11 },
        { Card.Rank.Ten, 10 },
        { Card.Rank.King, 4 },
        { Card.Rank.Queen, 3 },
        { Card.Rank.Eight, 0 },
        { Card.Rank.Seven, 0 }
    };

    // Higher value means stronger card for winning a trick
    private static readonly Dictionary<Card.Rank, int> NonTrumpStrength = new Dictionary<Card.Rank, int>
    {
        { Card.Rank.Ace, 8 },
        { Card.Rank.Ten, 7 },
        { Card.Rank.King, 6 },
        { Card.Rank.Queen, 5 },
        { Card.Rank.Jack, 4 },
        { Card.Rank.Nine, 3 },
        { Card.Rank.Eight, 2 },
        { Card.Rank.Seven, 1 }
    };

    private static readonly Dictionary<Card.Rank, int> TrumpStrength = new Dictionary<Card.Rank, int>
    {
        { Card.Rank.Jack, 8 },
        { Card.Rank.Nine, 7 },
        { Card.Rank.Ace, 6 },
        { Card.Rank.Ten, 5 },
        { Card.Rank.King, 4 },
        { Card.Rank.Queen, 3 },
        { Card.Rank.Eight, 2 },
        { Card.Rank.Seven, 1 }
    };

    public static int GetCardPoints(Card card, GameState.GameMode? mode, Card.Suit? trump)
    {
        if (card == null) return 0;

        if (mode == GameState.GameMode.AllTrumps)
        {
            return TrumpPoints[card.rank];
        }

        if (mode == GameState.GameMode.NoTrumps)
        {
            return NonTrumpPoints[card.rank];
        }

        // Suit mode
        if (trump.HasValue && card.suit == trump.Value)
            return TrumpPoints[card.rank];

        return NonTrumpPoints[card.rank];
    }

    public static int CalculateTrickPoints(List<Card> trick, GameState.GameMode? mode, Card.Suit? trump)
    {
        if (trick == null) return 0;
        return trick.Sum(c => GetCardPoints(c, mode, trump));
    }

    public static int GetWinningCardIndex(List<Card> trick, GameState.GameMode? mode, Card.Suit? trump)
    {
        if (trick == null || trick.Count == 0) return -1;

        Card.Suit leadSuit = trick[0].suit;
        if (mode == GameState.GameMode.AllTrumps)
        {
            int bestIndex = 0;
            int bestStrength = TrumpStrength[trick[0].rank];
            for (int i = 1; i < trick.Count; i++)
            {
                if (trick[i].suit != leadSuit) continue;
                int s = TrumpStrength[trick[i].rank];
                if (s > bestStrength)
                {
                    bestStrength = s;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        if (mode == GameState.GameMode.Suit && trump.HasValue)
        {
            int bestTrumpIndex = -1;
            int bestTrumpStrength = -1;
            for (int i = 0; i < trick.Count; i++)
            {
                if (trick[i].suit == trump.Value)
                {
                    int s = TrumpStrength[trick[i].rank];
                    if (s > bestTrumpStrength)
                    {
                        bestTrumpStrength = s;
                        bestTrumpIndex = i;
                    }
                }
            }
            if (bestTrumpIndex != -1) return bestTrumpIndex;
        }

        // Otherwise highest card of lead suit wins
        int bestIndex2 = -1;
        int bestStrength2 = -1;
        for (int i = 0; i < trick.Count; i++)
        {
            if (trick[i].suit == leadSuit)
            {
                int s = NonTrumpStrength[trick[i].rank];
                if (s > bestStrength2)
                {
                    bestStrength2 = s;
                    bestIndex2 = i;
                }
            }
        }

        return bestIndex2 >= 0 ? bestIndex2 : 0;
    }

    public static List<Card> GetLegalPlays(GameState state, Player player)
    {
        var hand = player.Hand;
        if (hand == null || hand.Count == 0) return new List<Card>();

        if (state.CurrentTrick == null || state.CurrentTrick.Count == 0)
        {
            return new List<Card>(hand);
        }
        GameState.GameMode? mode = state.CurrentGameMode;
        Card.Suit? trumpSuit = state.TrumpSuit;
        if (mode == null) return new List<Card>();

        Card.Suit leadSuit = state.CurrentTrick[0].suit;
        
        var followSuitCards = hand.Where(c => c.suit == leadSuit).ToList();
        if (followSuitCards != null && followSuitCards.Count > 0)
        {
            if(mode == GameState.GameMode.AllTrumps || (trumpSuit == leadSuit))
            {
                var trumpsInTrick = state.CurrentTrick.Where(c => leadSuit == c.suit).ToList();
                var highestTrumpInTrick = trumpsInTrick.Max(c => TrumpStrength[c.rank]);
                var higherfollowSuitCards = followSuitCards.Where(c => TrumpStrength[c.rank] > highestTrumpInTrick).ToList();
                if (higherfollowSuitCards != null && higherfollowSuitCards.Count > 0)
                    return higherfollowSuitCards;
            }
            return followSuitCards;
        }

        if(trumpSuit.HasValue)
        {
            var trumpsInTrick = state.CurrentTrick.Where(c => trumpSuit == c.suit).ToList();
            var trickStarterIndex = (state.Players.Count - state.CurrentTrick.Count + state.CurrentPlayerIndex) % state.Players.Count;
            var winnerIndex = trickStarterIndex + GetWinningCardIndex(state.CurrentTrick, mode, trumpSuit);
            if((trumpsInTrick.Count > 0 || winnerIndex % 2 != state.CurrentPlayerIndex % 2) && !(trumpsInTrick.Count > 0 && winnerIndex % 2 == state.CurrentPlayerIndex % 2))
            {
                var highestTrumpInTrick = trumpsInTrick.Count > 0 ? trumpsInTrick.Max(c => TrumpStrength[c.rank]) : -1;
                var playerTrumps = hand.Where(c => c.suit == trumpSuit && TrumpPoints[c.rank] > highestTrumpInTrick).ToList();
                if (playerTrumps.Count > 0)
                    return playerTrumps;
            }
        }

        return new List<Card>(hand);
    }
}
