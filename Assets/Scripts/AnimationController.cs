using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

#nullable enable

public class AnimationController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform? cardsContainer;
    public Transform? playerCardsContainer;

    public void DisplayAllCards(List<Player> players)
    {
        if (cardsContainer == null) return;
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            for (int j = 0; j < player.Hand.Count; j++)
            {
                DisplayCard(player.Hand[j], j, i);
            }
        }
    }

    public void DisplayCard(Card? card, int position, int player_idx)
    {
        if (cardsContainer == null) return;
        string cardImagePath = card == null 
            ? "CardSet/Blue-Cover"
            : $"CardSet/{card.suit}/{card.rank}-{card.suit}";

        Transform playerContainer = cardsContainer.GetChild(player_idx);
        Transform cardContainer = playerContainer.GetChild(position);
        Sprite sprite = Resources.Load<Sprite>(cardImagePath);
        if (sprite == null)
        {
            Debug.LogError($"Failed to load sprite at path: {cardImagePath}");
            return;
        }
        if (cardContainer.TryGetComponent<Image>(out var cardImage))
        {
            cardImage.sprite = sprite;
        }
    }

    public void RemoveDisplayedCard(int position, int player_idx)
    {
        if (cardsContainer == null) return;
        Transform playerContainer = cardsContainer.GetChild(player_idx);
        Transform cardContainer = playerContainer.GetChild(position);
        if (cardContainer.TryGetComponent<Image>(out var cardImage))
        {
            cardImage.sprite = null; // Clear the sprite to indicate the card has been played
        }
        RepositionCards(position, player_idx);
    }

    public void RepositionCards(int position, int player_idx)
    {
        if (cardsContainer == null) return;
        Transform playerContainer = cardsContainer.GetChild(player_idx);
        for (int i = position; i < playerContainer.childCount - 1; i++)
        {
            Transform currentCard = playerContainer.GetChild(i);
            Transform nextCard = playerContainer.GetChild(i + 1);
            if (nextCard.TryGetComponent<Image>(out var nextImage) && 
                currentCard.TryGetComponent<Image>(out var currentImage))
            {
                currentImage.sprite = nextImage.sprite;
            }
        }
        Transform lastCard = playerContainer.GetChild(playerContainer.childCount - 1);
        if (lastCard.TryGetComponent<Image>(out var lastImage))
        {
            lastImage.sprite = null;
        }
    }

    public void RemoveDisplayedPlayedCard(int player_idx)
    {
        if (playerCardsContainer == null) return;
        Transform cardContainer = playerCardsContainer.GetChild(player_idx);
        if (cardContainer.TryGetComponent<Image>(out var cardImage))
        {
            cardImage.sprite = null;
        }
    }

    public void DisplayPlayedCard(Card card, int player_idx)
    {
        if(playerCardsContainer == null) return;
        Transform cardContainer = playerCardsContainer.GetChild(player_idx);
        Sprite sprite = Resources.Load<Sprite>($"CardSet/{card.suit}/{card.rank}-{card.suit}");
        if (sprite == null)
        {
            Debug.LogError($"Failed to load sprite for played card: {card.suit} {card.rank}");
            return;
        }
        if (cardContainer.TryGetComponent<Image>(out var cardImage))
        {
            cardImage.sprite = sprite;
        }
    }

}
