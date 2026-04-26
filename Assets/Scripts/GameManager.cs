using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardManager boardManager;
    public PieceColor currentTurn = PieceColor.White;

    public bool isGameOver      = false;
    public bool isResolvingTurn = false;
    public PieceColor winner;

    public bool stopTimeOnGameOver = true;

    // =========================
    // 移動処理
    // =========================
    public bool TryMove(Vector2Int from, Vector2Int to)
    {
        if (isGameOver)                      return false;
        if (isResolvingTurn)                 return false;
        if (boardManager.IsPromotionPending) return false;

        Piece piece = boardManager.board[from.x, from.y];
        if (piece == null)              return false;
        if (piece.color != currentTurn) return false;

        bool moved = boardManager.MovePiece(new Move(from, to));

        if (moved && !boardManager.IsPromotionPending)
            AfterMove();

        return moved;
    }

    // =========================
    // 昇格確定
    // =========================
    public void ConfirmPromotion(PieceType promotionType)
    {
        if (isGameOver)                       return;
        if (!boardManager.IsPromotionPending) return;

        bool success = boardManager.CompletePromotion(promotionType);
        if (success)
            AfterMove();
    }

    // =========================
    // 移動後の処理（チェック・詰み・ステールメイト判定）
    // =========================
    private void AfterMove()
    {
        if (isGameOver) return;

        isResolvingTurn = true;

        PieceColor next = ChessRules.Opposite(currentTurn);
        bool inCheck    = ChessRules.IsInCheck(boardManager.board, next);
        bool hasLegal   = ChessRules.HasAnyLegalMove(boardManager.board, next);

        if (!hasLegal)
        {
            if (inCheck)
            {
                Debug.Log($"チェックメイト！ 勝者: {currentTurn}");
                EndGame(currentTurn);
            }
            else
            {
                Debug.Log("ステールメイト！ 引き分けです");
                EndGameDraw();
            }
        }
        else
        {
            if (inCheck)
                Debug.Log($"{next} がチェックされています！");

            EndTurn();
        }

        isResolvingTurn = false;
    }

    // =========================
    // 手番交代
    // =========================
    public void EndTurn()
    {
        currentTurn = ChessRules.Opposite(currentTurn);
        Debug.Log($"手番交代: {currentTurn}");
    }

    // =========================
    // ゲーム終了
    // =========================
    public void EndGame(PieceColor winningColor)
    {
        isGameOver = true;
        winner     = winningColor;

        Debug.Log($"ゲーム終了！ 勝者: {winner}");

        if (stopTimeOnGameOver)
            Time.timeScale = 0f;
    }

    public void EndGameDraw()
    {
        isGameOver = true;

        Debug.Log("ゲーム終了！ 引き分け（ステールメイト）");

        if (stopTimeOnGameOver)
            Time.timeScale = 0f;
    }

    // =========================
    // リセット
    // =========================
    public void ResetGameState()
    {
        isGameOver      = false;
        isResolvingTurn = false;
        winner          = PieceColor.White;
        currentTurn     = PieceColor.White;
        Time.timeScale  = 1f;
    }
}