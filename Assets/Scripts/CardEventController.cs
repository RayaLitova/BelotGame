using UnityEngine;

public class CardEventController : MonoBehaviour
{
    private GameController gameController;
    public void PlayCard()
    {
        Debug.Log($"Card at position {transform.GetSiblingIndex()} clicked.");
        if(gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }
        gameController.PlayCard(transform.GetSiblingIndex());
    }
}
