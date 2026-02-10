using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public AnimationController animationController;
    private GameState gameState;
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
        foreach (var player in gameState.Players)
        {
            Debug.Log($"{player.Name}: {string.Join(", ", player.Hand)}");
        }
        animationController.DisplayAllCards(gameState.Players);
    }

    public void PlayCard(int position)
    {
        Debug.Log($"Player {gameState.CurrentPlayerIndex} is playing card at position {position}");
        Player player = gameState.GetCurrentPlayer();
        List<Card> legalPlays = BelotRules.GetLegalPlays(gameState, player);
        Card card = player.GetCard(position);
        if (card == null || !legalPlays.Contains(card))
        {
            Debug.LogWarning("Illegal card play!");
            return;
        }
        // remove from player's hand (model) so indices stay in sync with UI repositioning
        Card removed = player.PlayCard(position);
        if (removed == null)
        {
            Debug.LogError($"Failed to remove card at position {position} from player {gameState.CurrentPlayerIndex}");
            return;
        }

        animationController.DisplayPlayedCard(removed, gameState.CurrentPlayerIndex);
        animationController.RemoveDisplayedCard(position, gameState.CurrentPlayerIndex);
        gameState.AddCardToTrick(removed);
        if (gameState.CurrentTrick.Count == gameState.Players.Count)
        {
            for (int i = 0; i < gameState.Players.Count; i++)
            {
                animationController.RemoveDisplayedPlayedCard(i);
            }
        }
    }
}
