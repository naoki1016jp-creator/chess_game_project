using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardManager boardManager;
    public PieceColor currentTurn = PieceColor.White;
    public bool isGameOver = false;

    public bool TryMove(Vector2Int from, Vector2Int to)
    {
        if (isGameOver || boardManager.IsPromotionPending) return false;

        Piece piece = boardManager.board[from.x, from.y];
        if (piece == null || piece.color != currentTurn) return false;

        if (!boardManager.MovePiece(new Move(from, to))) return false;

        if (!boardManager.IsPromotionPending)
            EndTurn();

        return true;
    }

    public void ConfirmPromotion(PieceType type)
    {
        if (boardManager.CompletePromotion(type))
            EndTurn();
    }

    private void EndTurn()
    {
        currentTurn = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }
}