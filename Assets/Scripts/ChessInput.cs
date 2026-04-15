using UnityEngine;

public class ChessInput : MonoBehaviour
{
    public Camera targetCamera;
    public GameManager gameManager;

    private Piece selectedPiece;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (gameManager == null) return;

        // ゲーム終了中は入力しない
        if (gameManager.isGameOver) return;

        // ターン解決中は入力しない
        if (gameManager.isResolvingTurn) return;

        // 昇格待ち中はキー入力のみ
        if (gameManager.boardManager != null && gameManager.boardManager.IsPromotionPending)
        {
            HandlePromotionInput();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandlePromotionInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            gameManager.ConfirmPromotion(PieceType.Queen);
        else if (Input.GetKeyDown(KeyCode.R))
            gameManager.ConfirmPromotion(PieceType.Rook);
        else if (Input.GetKeyDown(KeyCode.B))
            gameManager.ConfirmPromotion(PieceType.Bishop);
        else if (Input.GetKeyDown(KeyCode.N))
            gameManager.ConfirmPromotion(PieceType.Knight);
    }

    private void HandleClick()
    {
        if (targetCamera == null) return;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Piece clickedPiece = hit.collider.GetComponentInParent<Piece>();
            if (clickedPiece != null)
            {
                HandlePieceClick(clickedPiece);
                return;
            }

            ChessSquare clickedSquare = hit.collider.GetComponentInParent<ChessSquare>();
            if (clickedSquare != null)
            {
                HandleSquareClick(clickedSquare);
                return;
            }

            DeselectPiece();
        }
        else
        {
            DeselectPiece();
        }
    }

    private void HandlePieceClick(Piece clickedPiece)
    {
        // まだ何も選択していない
        if (selectedPiece == null)
        {
            if (clickedPiece.color == gameManager.currentTurn)
            {
                SelectPiece(clickedPiece);
            }
            return;
        }

        // 同じ駒を再クリックで選択解除
        if (clickedPiece == selectedPiece)
        {
            DeselectPiece();
            return;
        }

        // 味方駒なら選び直し
        if (clickedPiece.color == gameManager.currentTurn)
        {
            SelectPiece(clickedPiece);
            return;
        }

        // 敵駒を直接クリックしても手動攻撃しない
        Debug.Log("攻撃はターン終了時に自動で処理されます");
    }

    private void HandleSquareClick(ChessSquare clickedSquare)
    {
        if (selectedPiece == null)
            return;

        bool moved = gameManager.TryMove(selectedPiece.boardPosition, clickedSquare.boardPosition);

        if (moved)
        {
            selectedPiece = null;
        }
        else
        {
            Debug.Log("そのマスには移動できません");
        }
    }

    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
        Debug.Log($"選択: {piece.name} / HP {piece.currentHP}/{piece.maxHP}");
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
    }
}