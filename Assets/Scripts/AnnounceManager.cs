using System.Collections.Generic;
using System.Linq;

#nullable enable

public static class AnnounceManager
{
    public enum AnnounceType
    {
        Belote,
        Sequence,
        FourOfAKind
    }

    public class Announce
    {
        public AnnounceType Type { get; }
        public int PlayerIndex { get; }
        public int Points { get; }
        public int Length { get; }

        public Announce(AnnounceType type, int playerIndex, int points, int length = 0)
        {
            Type = type;
            PlayerIndex = playerIndex;
            Points = points;
            Length = length;
        }

        public override string ToString()
        {
            return Type switch
            {
                AnnounceType.Belote => "Belote",
                AnnounceType.Sequence => $"Sequence of {Length}",
                AnnounceType.FourOfAKind => "Four of a Kind",
                _ => "Unknown"
            };
        }
    }

    private static readonly Dictionary<Card.Rank, int> FourOfAKindPoints = new Dictionary<Card.Rank, int>
    {
        { Card.Rank.Jack, 200 },
        { Card.Rank.Nine, 150 }
    };

    private const int Sequence3Points = 20;
    private const int Sequence4Points = 50;
    private const int Sequence5PlusPoints = 100;
    private const int BelotePoints = 20;
    private const int DefaultFourOfAKindPoints = 100;

    public static Announce? HandleBelote(List<Card> hand, Card played_card, int playerIndex, GameState.GameMode? mode, Card.Suit? trumpSuit)
    {
        if (hand == null) return null;
        if (mode == GameState.GameMode.NoTrumps || (played_card.rank != Card.Rank.Queen && played_card.rank != Card.Rank.King)) return null;
        GameState.GameMode allTrumpsMode = GameState.GameMode.AllTrumps;
        if (mode != allTrumpsMode && played_card.suit != trumpSuit) return null;

        Card.Rank searchedRank = played_card.rank == Card.Rank.Queen ? Card.Rank.King : Card.Rank.Queen;
        bool hasOther = hand.Any(c => c.suit == played_card.suit && c.rank == searchedRank);
        return hasOther ? new Announce(AnnounceType.Belote, playerIndex, BelotePoints) : null;
    } 
    public static List<Announce> GetAnnouncesFromHand(List<Card> hand, int playerIndex)
    {
        var announces = new List<Announce>();
        if (hand == null || hand.Count == 0) return announces;

        var rankGroups = hand.GroupBy(c => c.rank).ToList();
        foreach (var g in rankGroups)
        {
            if (g.Count() == 4)
            {
                int pts = FourOfAKindPoints.ContainsKey(g.Key) ? FourOfAKindPoints[g.Key] : DefaultFourOfAKindPoints;
                announces.Add(new Announce(AnnounceType.FourOfAKind, playerIndex, pts));
            }
        }

        var suits = hand.GroupBy(c => c.suit);
        foreach (var suitGroup in suits)
        {
            var ranks = suitGroup.Select(c => (int)c.rank).Distinct().OrderBy(x => x).ToList();
            if (ranks.Count == 0) continue;

            int runStart = ranks[0];
            int runLen = 1;
            for (int i = 1; i < ranks.Count; i++)
            {
                if (ranks[i] == ranks[i - 1] + 1)
                {
                    runLen++;
                }
                else
                {
                    if (runLen >= 3)
                    {
                        AddSequenceAnnounce(playerIndex, runLen, announces);
                    }
                    runStart = ranks[i];
                    runLen = 1;
                }
            }
            if (runLen >= 3)
            {
                AddSequenceAnnounce(playerIndex, runLen, announces);
            }
        }

        return announces;
    }

    private static void AddSequenceAnnounce(int playerIndex, int length, List<Announce> announces)
    {
        int pts = length switch
        {
            3 => Sequence3Points,
            4 => Sequence4Points,
            _ => Sequence5PlusPoints
        };
        announces.Add(new Announce(AnnounceType.Sequence, playerIndex, pts, length));
    }
}
