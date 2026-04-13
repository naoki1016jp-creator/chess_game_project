using UnityEngine;

public class PromotionUI : MonoBehaviour
{
    public GameObject panelRoot;
    public GameManager gameManager;

    private void Start()
    {
        Hide();
    }

    public void Show(PieceColor color)
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        Debug.Log($"昇格UI表示: {color}");
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void PromoteToQueen()
    {
        if (gameManager != null)
            gameManager.ConfirmPromotion(PieceType.Queen);
    }

    public void PromoteToRook()
    {
        if (gameManager != null)
            gameManager.ConfirmPromotion(PieceType.Rook);
    }

    public void PromoteToBishop()
    {
        if (gameManager != null)
            gameManager.ConfirmPromotion(PieceType.Bishop);
    }

    public void PromoteToKnight()
    {
        if (gameManager != null)
            gameManager.ConfirmPromotion(PieceType.Knight);
    }
}