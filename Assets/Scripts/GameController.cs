using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Transactions;
using System;

public class GameController : MonoBehaviour
{
    public AnimationController animationController;
    private GameState gameState;
    public GameObject biddingContainerController;
    
    void Start()
    {
        gameState = new GameState(new List<Player>
        {
            new Player("1"),
            new Player("2"),
            new Player("3"),
            new Player("4")
        });
        gameState.StartNewRound();
        animationController.DisplayAllCards(gameState.Players);
    }

    public void PlayCard(int position)
    {
        Player player = gameState.GetCurrentPlayer();
        List<Card> legalPlays = BelotRules.GetLegalPlays(gameState, player);
        Card card = player.GetCard(position);
        if (card == null || !legalPlays.Contains(card))
        {
            Debug.LogWarning("Illegal card play!");
            return;
        }
        Card removed = player.PlayCard(position);
        if (removed == null)
        {
            Debug.LogError($"Failed to remove card at position {position} from player {gameState.CurrentPlayerIndex}");
            return;
        }

        animationController.DisplayPlayedCard(removed, gameState.CurrentPlayerIndex);
        animationController.RemoveDisplayedCard(position, gameState.CurrentPlayerIndex);
        gameState.AddCardToTrick(removed);
        animationController.MoveTurnIndicator(gameState.CurrentPlayerIndex);
        if (gameState.CurrentTrick.Count == 0)
        {
            for (int i = 0; i < gameState.Players.Count; i++)
            {
                StartCoroutine(RemoveDisplayedPlayedCardDelayed(i));
            }
        }
        if(gameState.CurrentPlayerIndex != 0)
        {
            StartCoroutine(RequestActionDelayed());
        }
    }

    private IEnumerator RemoveDisplayedPlayedCardDelayed(int i)
    {
        yield return new WaitForSeconds(1);
        animationController.RemoveDisplayedPlayedCard(i);
    }

    private IEnumerator RequestActionDelayed()
    {
        yield return new WaitForSeconds(1);
        
        yield return StartCoroutine(PythonConnector.RequestAction(gameState, gameState.CurrentPlayerIndex, card => 
        {
            PlayCard(card);
        }));
    }

    public void PlayCard(Card card)
    {
        Player player = gameState.GetCurrentPlayer();
        int position = player.Hand.IndexOf(card);
        if (position < 0)
        {
            Debug.LogError($"Card {card} not found in player {gameState.CurrentPlayerIndex}'s hand");
            return;
        }
        PlayCard(position);
    }

    public void SetModeToAT()
    {
        gameState.SetGameMode(GameState.GameMode.AllTrumps, null);
        Destroy(biddingContainerController);
        animationController.DisplayAllCards(gameState.Players);
    }

    public void SetModeToNT()
    {
        gameState.SetGameMode(GameState.GameMode.NoTrumps, null);
        Destroy(biddingContainerController);
        animationController.DisplayAllCards(gameState.Players);
    }

    public void SetModeToSpades()
    {
        gameState.SetGameMode(GameState.GameMode.Suit, Card.Suit.Spades);
        Destroy(biddingContainerController);
        animationController.DisplayAllCards(gameState.Players);
    }

    public void SetModeToHearts()
    {
        gameState.SetGameMode(GameState.GameMode.Suit, Card.Suit.Hearts);
        Destroy(biddingContainerController);
        animationController.DisplayAllCards(gameState.Players);
    }
    public void SetModeToDiamonds()
    {
        gameState.SetGameMode(GameState.GameMode.Suit, Card.Suit.Diamonds);
        Destroy(biddingContainerController);
        animationController.DisplayAllCards(gameState.Players);
    }

    public void SetModeToClubs()
    {
        gameState.SetGameMode(GameState.GameMode.Suit, Card.Suit.Clubs);
        Destroy(biddingContainerController);
        animationController.DisplayAllCards(gameState.Players);
    }
}
