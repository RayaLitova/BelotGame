using UnityEngine;

public class CardEventController : MonoBehaviour
{
    private GameController gameController;
    public void PlayCard()
    {
        if(gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }
        gameController.PlayCard(transform.GetSiblingIndex());
    }
}
