using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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
    public List<Card> PlayedCards { get; private set; }
    public (int Team1, int Team2) TeamScores { get; set; }
    public int TrickNumber { get; set; }
    private int DealerIndex = 2;
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
        PlayedCards = new List<Card>();
        TeamScores = (0, 0);
        TrickNumber = 0;
    }

    public void StartNewRound()
    {
        GameDealer.ResetDeck();
        GameDealer.ShuffleDeck();
        GameDealer.DealInitialCards(Players);
        CurrentPhase = GamePhase.Bidding;
        TrumpSuit = null;
        CurrentGameMode = null;
        CurrentTrick.Clear();
        PlayedCards.Clear();
        DealerIndex = (DealerIndex + 1) % Players.Count;
        CurrentPlayerIndex = (DealerIndex + 1) % Players.Count;
        TrickNumber = 0;
    }

    public void SetGameMode(GameMode mode, Card.Suit? suit)
    {
        CurrentGameMode = mode;
        TrumpSuit = suit;
        CurrentPhase = GamePhase.Playing;
        GameDealer.DealSecondRoundCards(Players);
    }

    public void AddCardToTrick(Card card)
    {
        CurrentTrick.Add(card);
        PlayedCards.Add(card);

        if(CurrentTrick.Count == Players.Count)
        {
            EvaluateHand();
            if(TrickNumber >= 8)
                StartNewRound();
        }
        else
        {
            MoveToNextPlayer();
        }
    }

    private void EvaluateHand()
    {
        if (CurrentTrick == null || CurrentTrick.Count == 0) return;
        int winningCardIndex = BelotRules.GetWinningCardIndex(CurrentTrick, CurrentGameMode, TrumpSuit);
        if (winningCardIndex < 0) return;
        int trickStarter = (CurrentPlayerIndex + 1) % Players.Count;
        UnityEngine.Debug.Log($"Trick started by player {trickStarter} CurrentplayerIndex: {CurrentPlayerIndex} CurrentTrick count: {CurrentTrick.Count}");

        int winnerIndex = (trickStarter + winningCardIndex) % Players.Count;
        UnityEngine.Debug.Log($"Trick won by player {winnerIndex} with card {CurrentTrick[winningCardIndex]}");
        int trickPoints = BelotRules.CalculateTrickPoints(CurrentTrick, CurrentGameMode, TrumpSuit);
        AddPoints(trickPoints, winnerIndex);
        
        CurrentPlayerIndex = winnerIndex;
        TrickNumber++;
        CurrentTrick.Clear();
    }

    public void AddPoints(int points, int player_index)
    {
        if (player_index % 2 == 0)
        {
            TeamScores = (TeamScores.Team1 + points, TeamScores.Team2);
        }
        else
        {
            TeamScores = (TeamScores.Team1, TeamScores.Team2 + points);
        }
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
        PlayedCards.Clear();
    }

    public override string ToString()
    {
        string playerInfo = string.Join(", ", Players.Select(p => $"{p.Name}"));
        return $"Phase: {CurrentPhase} - Trump: {TrumpSuit} - Players: {playerInfo}";
    }
}
