using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections;

#nullable enable

public class AnimationController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform cardsContainer;
    public Transform playedCardsContainer;
    public Transform turnIndicatorContainer;
    public Transform announcesContainer;

    public void DisplayAnnounce(AnnounceManager.Announce announce, int playerIndex)
    {
        if (announcesContainer == null) return;
        Transform announceSlot = announcesContainer.GetChild(playerIndex);
        if (announceSlot.TryGetComponent<Text>(out var textComponent))
        {
            textComponent.text = announce.ToString();
            announceSlot.gameObject.SetActive(true);
            StartCoroutine(HideAnnounceAfterDelay(announceSlot, 2f));
        }
    }

    private IEnumerator HideAnnounceAfterDelay(Transform announceSlot, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (announceSlot != null)
        {
            announceSlot.gameObject.SetActive(false);
        }
    }
    public void DisplayAllCards(List<Player> players)
    {
        if (cardsContainer == null) return;
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            for (int j = 0; j < player.Hand.Count; j++)
            {
                DisplayCard(i==0 ? player.Hand[j] : null, j, i);
            }
            Transform playerContainer = cardsContainer.GetChild(i);
            for (int j = player.Hand.Count; j < 8; j++)
            {
                playerContainer.GetChild(j).gameObject.SetActive(false);
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
        cardContainer.gameObject.SetActive(true);
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

    public void RemoveDisplayedCard(int position, int player_idx, int hand_count)
    {
        RepositionCards(position, player_idx, hand_count);
    }

    public void RepositionCards(int position, int player_idx, int hand_count)
    {
        if (cardsContainer == null) return;
        Transform playerContainer = cardsContainer.GetChild(player_idx);
        for (int i = position; i < hand_count; i++)
        {
            Transform currentCard = playerContainer.GetChild(i);
            Transform nextCard = playerContainer.GetChild(i + 1);
            if (nextCard.TryGetComponent<Image>(out var nextImage) && 
                currentCard.TryGetComponent<Image>(out var currentImage))
            {
                currentImage.sprite = nextImage.sprite;
            }
        }
        for (int i = hand_count; i < 8; i++)
        {
            playerContainer.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void RemoveDisplayedPlayedCard(int player_idx)
    {
        if (playedCardsContainer == null) return;
        Transform cardContainer = playedCardsContainer.GetChild(player_idx);
        if (cardContainer.TryGetComponent<Image>(out var cardImage))
        {
            cardContainer.gameObject.SetActive(false);
            cardImage.sprite = null;
        }
    }

    public void DisplayPlayedCard(Card card, int player_idx)
    {
        if(playedCardsContainer == null) return;
        Transform cardContainer = playedCardsContainer.GetChild(player_idx);
        Sprite sprite = Resources.Load<Sprite>($"CardSet/{card.suit}/{card.rank}-{card.suit}");
        if (sprite == null)
        {
            Debug.LogError($"Failed to load sprite for played card: {card.suit} {card.rank}");
            return;
        }
        if (cardContainer.TryGetComponent<Image>(out var cardImage))
        {
            cardContainer.gameObject.SetActive(true);
            cardImage.sprite = sprite;
        }
    }

    public void MoveTurnIndicator(int player_idx)
    {
        turnIndicatorContainer.GetChild(player_idx).gameObject.SetActive(true);
        turnIndicatorContainer.GetChild(player_idx - 1 >= 0 ? player_idx - 1 : 3).gameObject.SetActive(false);
    }

}
