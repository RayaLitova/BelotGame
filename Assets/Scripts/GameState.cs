using System;
using System.Collections.Generic;
using System.Linq;

public class GameState
{
    public enum GameMode
    {
        AllTrumps,
        NoTrumps,
        Suit
    }
    public enum GamePhase
    {
        Bidding,
        Playing,
        RoundEnd,
        GameEnd
    }

    public List<Player> Players { get; private set; }
    public Dealer GameDealer { get; private set; }
    public GamePhase CurrentPhase { get; set; }
    public Card.Suit? TrumpSuit { get; set; }
    public GameMode? CurrentGameMode { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public List<Card> CurrentTrick { get; private set; }
    public (int Team1, int Team2) TeamScores { get; set; }
    private const int MaxScore = 151;

    public GameState(List<Player> players)
    {
        Players = players ?? new List<Player>();;
        GameDealer = new Dealer();
        CurrentPhase = GamePhase.Bidding;
        TrumpSuit = null;
        CurrentGameMode = null;
        CurrentPlayerIndex = 0;
        CurrentTrick = new List<Card>();
        TeamScores = (0, 0);
    }

    public void StartNewRound()
    {
        GameDealer.ResetDeck();
        GameDealer.ShuffleDeck();
        GameDealer.DealInitialCards(Players);
        CurrentPhase = GamePhase.Bidding;
        MoveToNextPlayer();
        TrumpSuit = null;
        CurrentGameMode = null;
        CurrentTrick.Clear();
    }

    public void SetGameMode(GameMode mode, Card.Suit? suit)
    {
        CurrentGameMode = mode;
        TrumpSuit = suit;
        CurrentPhase = GamePhase.Playing;
    }

    public void AddCardToTrick(Card card)
    {
        CurrentTrick.Add(card);
        if(CurrentTrick.Count == Players.Count)
        {
            EvaluateHand();
        }
        else
            MoveToNextPlayer();
    }

    private void EvaluateHand()
    {
        if (CurrentTrick == null || CurrentTrick.Count == 0) return;

        // At this point CurrentPlayerIndex points to the trick leader (see AddCardToTrick flow)
        int leaderIndex = CurrentPlayerIndex;
        int winningCardIndex = BelotRules.GetWinningCardIndex(CurrentTrick, CurrentGameMode, TrumpSuit);
        if (winningCardIndex < 0) return;

        int winnerIndex = (leaderIndex + winningCardIndex) % Players.Count;

        int trickPoints = BelotRules.CalculateTrickPoints(CurrentTrick, CurrentGameMode, TrumpSuit);

        // Teams: players 0 & 2 vs 1 & 3
        if (winnerIndex % 2 == 0)
        {
            TeamScores = (TeamScores.Team1 + trickPoints, TeamScores.Team2);
        }
        else
        {
            TeamScores = (TeamScores.Team1, TeamScores.Team2 + trickPoints);
        }

        // Winner leads next trick
        CurrentPlayerIndex = winnerIndex;

        // clear the trick for next round
        CurrentTrick.Clear();
    }

    public void ClearTrick()
    {
        CurrentTrick.Clear();
    }

    public Player GetCurrentPlayer()
    {
        if (CurrentPlayerIndex >= 0 && CurrentPlayerIndex < Players.Count)
        {
            return Players[CurrentPlayerIndex];
        }
        return null;
    }

    public void MoveToNextPlayer()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
    }

    public bool IsGameOver()
    {
        return TeamScores.Team1 >= MaxScore || TeamScores.Team2 >= MaxScore;
    }

    public Player GetWinner()
    {
        return TeamScores.Team1 >= MaxScore ? Players[0] : TeamScores.Team2 >= MaxScore ? Players[2] : null;
    }

    public void ResetGame()
    {
        foreach (var player in Players)
        {
            player.ClearHand();
        }
        CurrentPhase = GamePhase.Bidding;
        TrumpSuit = null;
        CurrentGameMode = null;
        CurrentTrick.Clear();
    }

    public override string ToString()
    {
        string playerInfo = string.Join(", ", Players.Select(p => $"{p.Name}"));
        return $"Phase: {CurrentPhase} - Trump: {TrumpSuit} - Players: {playerInfo}";
    }
}
