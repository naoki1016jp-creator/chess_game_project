using System.Collections.Generic;
using UnityEngine;

public class ChessInput : MonoBehaviour
{
    public Camera targetCamera;
    public GameManager gameManager;
    public BoardHighlighter boardHighlighter;

    private Piece selectedPiece;

    private void Start()
    {
        if (targetCamera == null)
        targetCamera = Camera.main;

        if (boardHighlighter == null)
        boardHighlighter = FindObjectsByType<BoardHighlighter>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];

        Debug.Log($"[ChessInput] Start. Camera={targetCamera}, GameManager={gameManager}, BoardHighlighter={boardHighlighter}");
    }

    private void Update()
    {
        if (gameManager == null) return;

        if (gameManager.boardManager != null && gameManager.boardManager.IsPromotionPending)
        {
            HandlePromotionInput();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[ChessInput] クリック検知");
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
        if (gameManager != null && gameManager.isGameOver)
            return;

        if (targetCamera == null)
        {
            Debug.LogWarning("[ChessInput] Camera が設定されていません");
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        Debug.Log($"[ChessInput] Raycast発射 from {Input.mousePosition}");

        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Debug.Log($"[ChessInput] ヒット: {hit.collider.gameObject.name}");

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

            Debug.Log("[ChessInput] 駒もマスもヒットしませんでした");
            DeselectPiece();
        }
        else
        {
            Debug.Log("[ChessInput] Raycastが何にもヒットしませんでした");
            DeselectPiece();
        }
    }

    private void HandlePieceClick(Piece clickedPiece)
    {
        if (selectedPiece == null)
        {
            if (clickedPiece.color == gameManager.currentTurn)
                SelectPiece(clickedPiece);
            return;
        }

        if (clickedPiece == selectedPiece)
        {
            DeselectPiece();
            return;
        }

        if (clickedPiece.color == gameManager.currentTurn)
        {
            SelectPiece(clickedPiece);
            return;
        }

        bool moved = gameManager.TryMove(selectedPiece.boardPosition, clickedPiece.boardPosition);

        if (moved)
        {
            ClearHighlights();
            selectedPiece = null;
        }
        else
        {
            Debug.Log("その駒は取れません");
        }
    }

    private void HandleSquareClick(ChessSquare clickedSquare)
    {
        if (selectedPiece == null)
            return;

        bool moved = gameManager.TryMove(selectedPiece.boardPosition, clickedSquare.boardPosition);

        if (moved)
        {
            ClearHighlights();
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
    Debug.Log($"選択: {piece.name} ({piece.boardPosition.x}, {piece.boardPosition.y})");

    if (boardHighlighter != null)
        boardHighlighter.ShowMoves(piece, gameManager.boardManager.board);
    else
        Debug.LogWarning("[ChessInput] boardHighlighterがnullです！");
}
    private void DeselectPiece()
    {
        selectedPiece = null;
        ClearHighlights();
    }

    private void ClearHighlights()
    {
        if (boardHighlighter != null)
            boardHighlighter.ClearAll();
    }
}