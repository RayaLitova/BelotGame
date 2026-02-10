using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

public static class PythonConnector
{
    [System.Serializable]
    public class CardArray
    {
        public int[] suit;
        public int[] rank;
        
        public CardArray(Card[] cardArray)
        {
            suit = new int[cardArray.Length];
            rank = new int[cardArray.Length];
            for (int i = 0; i < cardArray.Length; i++)
            {
                suit[i] = (int)cardArray[i].suit;
                rank[i] = (int)cardArray[i].rank;
            }
        }
    }
    
    [System.Serializable]
    public class TeamScores
    {
        public int team1;
        public int team2;
        
        public TeamScores(int t1, int t2)
        {
            team1 = t1;
            team2 = t2;
        }
    }
    
    [System.Serializable]
    public class RequestData
    {
        public int player_idx;
        public CardArray hand0;
        public CardArray hand1;
        public CardArray hand2;
        public CardArray hand3;
        public CardArray current_trick;
        public int contract;
        public int trump_suit;
        public TeamScores team_scores;
        public CardArray played_cards;
        public int trick_starter_idx;
    }
    
    [System.Serializable]
    public class ResponseData
    {
        public int rank;
        public int suit;
    }
    
    private static RequestData SerializeGameState(GameState gameState, int playerIndex)
    {
        return new RequestData
        {
            player_idx = playerIndex,
            hand0 = new CardArray(gameState.Players[0].Hand.ToArray()),
            hand1 = new CardArray(gameState.Players[1].Hand.ToArray()),
            hand2 = new CardArray(gameState.Players[2].Hand.ToArray()),
            hand3 = new CardArray(gameState.Players[3].Hand.ToArray()),
            current_trick = new CardArray(gameState.CurrentTrick.ToArray()),
            contract = gameState.CurrentGameMode.HasValue ? (int)gameState.CurrentGameMode.Value : 0,
            trump_suit = gameState.TrumpSuit.HasValue ? (int)gameState.TrumpSuit.Value : -1,
            team_scores = new TeamScores(gameState.TeamScores.Team1, gameState.TeamScores.Team2),
            played_cards = new CardArray(gameState.PlayedCards.ToArray()),
            trick_starter_idx = (gameState.Players.Count - gameState.CurrentTrick.Count + gameState.CurrentPlayerIndex) % gameState.Players.Count
        };
    }
    public static IEnumerator RequestAction(GameState gameState, int playerIndex, System.Action<Card> onCardReceived)
    {
        string url = "http://127.0.0.1:5000/process";

        string jsonData = JsonUtility.ToJson(SerializeGameState(gameState, playerIndex));
        using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ResponseData response = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
                Debug.Log($"Received: {response}");
                onCardReceived?.Invoke(new Card((Card.Suit)response.suit, (Card.Rank)response.rank));
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                onCardReceived?.Invoke(null);
            }
        }
    }
}