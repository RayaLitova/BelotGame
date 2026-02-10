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

    // Returns index in the trick list of the winning card
    public static int GetWinningCardIndex(List<Card> trick, GameState.GameMode? mode, Card.Suit? trump)
    {
        if (trick == null || trick.Count == 0) return -1;

        if (mode == GameState.GameMode.AllTrumps)
        {
            // Treat every card as trump
            int bestIndex = 0;
            int bestStrength = TrumpStrength[trick[0].rank];
            for (int i = 1; i < trick.Count; i++)
            {
                int s = TrumpStrength[trick[i].rank];
                if (s > bestStrength)
                {
                    bestStrength = s;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        Card.Suit leadSuit = trick[0].suit;

        // If a trump is present (when mode is Suit), highest trump wins
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

    // Returns legal plays for a player given current state and current trick
    // Simplified rules: must follow suit if possible; if cannot follow suit and there is at least one trump in trick,
    // and player has trumps, must play a higher trump if possible, otherwise any trump; else free play.
    public static List<Card> GetLegalPlays(GameState state, Player player)
    {
        var hand = player.Hand;
        if (hand == null || hand.Count == 0) return new List<Card>();

        if (state.CurrentTrick == null || state.CurrentTrick.Count == 0)
        {
            // any card may be led
            return new List<Card>(hand);
        }

        Card.Suit leadSuit = state.CurrentTrick[0].suit;
        var followSuitCards = hand.Where(c => c.suit == leadSuit).ToList();
        if (followSuitCards.Count > 0)
            return followSuitCards;

        // cannot follow suit
        // if there is a trump in the trick and player has trumps, must overtrump if possible
        var trumpsInTrick = state.CurrentTrick.Where(c => state.CurrentGameMode == GameState.GameMode.AllTrumps || (state.CurrentGameMode == GameState.GameMode.Suit && state.TrumpSuit.HasValue && c.suit == state.TrumpSuit.Value)).ToList();
        var playerTrumps = (state.CurrentGameMode == GameState.GameMode.AllTrumps)
            ? new List<Card>(hand)
            : (state.TrumpSuit.HasValue ? hand.Where(c => c.suit == state.TrumpSuit.Value).ToList() : new List<Card>());

        if (trumpsInTrick.Count > 0 && playerTrumps.Count > 0)
        {
            // find highest trump in trick
            int highestTrumpStrength = trumpsInTrick.Max(c => TrumpStrength[c.rank]);
            var overtrumps = playerTrumps.Where(c => TrumpStrength[c.rank] > highestTrumpStrength).ToList();
            if (overtrumps.Count > 0)
                return overtrumps;
            // otherwise must play a trump
            return playerTrumps;
        }

        // otherwise free to play any card
        return new List<Card>(hand);
    }
}
