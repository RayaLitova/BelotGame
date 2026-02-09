using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
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
    }

    void Update()
    {
        
    }
}
