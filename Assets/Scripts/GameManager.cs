using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardManager boardManager;
    public PieceColor currentTurn = PieceColor.White;

    public bool isGameOver = false;
    public PieceColor winner;

    public bool stopTimeOnGameOver = true;

    public bool TryMove(Vector2Int from, Vector2Int to)
    {
        if (isGameOver)
        {
            Debug.Log("ゲームはすでに終了しています");
            return false;
        }

        if (boardManager.IsPromotionPending)
        {
            Debug.Log("昇格待ち中です。Q/R/B/N を押してください。");
            return false;
        }

        Piece piece = boardManager.board[from.x, from.y];
        if (piece == null)
            return false;

        if (piece.color != currentTurn)
            return false;

        Piece target = boardManager.board[to.x, to.y];

        bool kingWillBeCaptured = false;
        if (target != null &&
            target.color != piece.color &&
            target.pieceType == PieceType.King)
        {
            kingWillBeCaptured = true;
        }

        bool moved = boardManager.MovePiece(new Move(from, to));

        if (moved)
        {
            if (kingWillBeCaptured)
            {
                EndGame(currentTurn);
            }
            else
            {
                // 昇格待ちでなければ手番交代
                if (!boardManager.IsPromotionPending)
                {
                    EndTurn();
                }
            }
        }

        return moved;
    }

    public void ConfirmPromotion(PieceType promotionType)
    {
        if (isGameOver) return;
        if (!boardManager.IsPromotionPending) return;

        bool success = boardManager.CompletePromotion(promotionType);
        if (success)
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        currentTurn = (currentTurn == PieceColor.White) ? PieceColor.Black : PieceColor.White;
        Debug.Log($"手番交代: {currentTurn}");
    }

    public void EndGame(PieceColor winningColor)
    {
        isGameOver = true;
        winner = winningColor;

        Debug.Log($"ゲーム終了！ 勝者: {winner}");

        if (stopTimeOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    public void ResetGameState()
    {
        isGameOver = false;
        winner = PieceColor.White;
        currentTurn = PieceColor.White;
        Time.timeScale = 1f;
    }
}