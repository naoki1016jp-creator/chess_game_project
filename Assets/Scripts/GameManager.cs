using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardManager boardManager;
    public PieceColor currentTurn = PieceColor.White;

    public bool isGameOver = false;
    public PieceColor winner;

    public bool stopTimeOnGameOver = true;
    public bool isResolvingTurn = false;

    // =========================
    // 移動だけ行う
    // =========================
    public bool TryMove(Vector2Int from, Vector2Int to)
    {
        if (isGameOver) return false;
        if (isResolvingTurn) return false;
        if (boardManager.IsPromotionPending) return false;

        Piece piece = boardManager.board[from.x, from.y];
        if (piece == null) return false;
        if (piece.color != currentTurn) return false;

        bool moved = boardManager.MovePiece(new Move(from, to));

        if (moved)
        {
            // 昇格待ちでなければ即ターン解決
            if (!boardManager.IsPromotionPending)
            {
                ResolveTurn();
            }
        }

        return moved;
    }

    // =========================
    // 昇格確定後にターン解決
    // =========================
    public void ConfirmPromotion(PieceType promotionType)
    {
        if (isGameOver) return;
        if (!boardManager.IsPromotionPending) return;

        bool success = boardManager.CompletePromotion(promotionType);
        if (success)
        {
            ResolveTurn();
        }
    }

    // =========================
    // ターン解決
    // =========================
    private void ResolveTurn()
    {
        if (isGameOver) return;

        isResolvingTurn = true;

        // 1. 支援フェーズ
        boardManager.ResolveSupportPhase();

        if (isGameOver)
        {
            isResolvingTurn = false;
            return;
        }

        // 2. 攻撃フェーズ（全員攻撃）
        boardManager.ResolveAttackPhase(currentTurn, this);

        if (!isGameOver)
        {
            EndTurn();
        }

        isResolvingTurn = false;
    }

    // =========================
    // 手番交代
    // =========================
    public void EndTurn()
    {
        currentTurn = (currentTurn == PieceColor.White)
            ? PieceColor.Black
            : PieceColor.White;

        Debug.Log($"手番交代: {currentTurn}");
    }

    // =========================
    // ゲーム終了
    // =========================
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
        isResolvingTurn = false;
    }
}