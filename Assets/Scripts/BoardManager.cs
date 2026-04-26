using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static Move LastMove { get; private set; }

    public Piece[,] board = new Piece[8, 8];

    [Header("Promotion Prefabs")]
    public GameObject whiteQueenPromotionPrefab;
    public GameObject blackQueenPromotionPrefab;
    public GameObject whiteRookPromotionPrefab;
    public GameObject blackRookPromotionPrefab;
    public GameObject whiteBishopPromotionPrefab;
    public GameObject blackBishopPromotionPrefab;
    public GameObject whiteKnightPromotionPrefab;
    public GameObject blackKnightPromotionPrefab;

    public bool IsPromotionPending => isPromotionPending;
    private bool isPromotionPending;

    private Vector2Int promotionPos;
    private PieceColor promotionColor;
    private GameObject pawnObj;
    private Transform pawnParent;
    private Quaternion pawnRot;
    private Vector3 pawnPos;

    private Dictionary<Vector2Int, ChessSquare> squaresByPos = new Dictionary<Vector2Int, ChessSquare>();

    private void Start()
    {
        RegisterSquares();
        RegisterScenePieces();
    }

    private void RegisterSquares()
    {
        squaresByPos.Clear();

#if UNITY_2023_1_OR_NEWER
        ChessSquare[] squares = FindObjectsByType<ChessSquare>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        ChessSquare[] squares = FindObjectsOfType<ChessSquare>(true);
#endif

        foreach (ChessSquare square in squares)
        {
            if (!squaresByPos.ContainsKey(square.boardPosition))
                squaresByPos.Add(square.boardPosition, square);
        }

        Debug.Log($"[BoardManager] {squaresByPos.Count} マス登録完了");
    }

    private void RegisterScenePieces()
    {
        board = new Piece[8, 8];

        Piece[] pieces = FindObjectsByType<Piece>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Piece piece in pieces)
        {
            ChessSquare nearest = GetNearestSquare(piece.transform.position);
            if (nearest == null) continue;

            Vector2Int pos = nearest.boardPosition;

            if (pos.x < 0 || pos.x >= 8 || pos.y < 0 || pos.y >= 8) continue;
            if (board[pos.x, pos.y] != null) continue;

            piece.boardPosition = pos;
            board[pos.x, pos.y] = piece;

            Debug.Log($"[BoardManager] {piece.name} -> {nearest.squareName} ({pos.x},{pos.y})");
        }
    }

    private ChessSquare GetNearestSquare(Vector3 worldPos)
    {
        ChessSquare nearest = null;
        float minDist = float.MaxValue;

        foreach (var pair in squaresByPos)
        {
            ChessSquare square = pair.Value;
            Vector2 pieceXZ  = new Vector2(worldPos.x, worldPos.z);
            Vector2 squareXZ = new Vector2(square.transform.position.x, square.transform.position.z);
            float dist = Vector2.Distance(pieceXZ, squareXZ);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = square;
            }
        }

        return nearest;
    }

    public bool MovePiece(Move move)
    {
        Piece piece = board[move.from.x, move.from.y];
        if (piece == null) return false;

        Move matched = null;
        foreach (var m in piece.GetMoves(board))
            if (m.to == move.to) matched = m;

        if (matched == null) return false;

        if (matched.isEnPassant)
        {
            var p = matched.enPassantCapturedPos;
            Destroy(board[p.x, p.y].gameObject);
            board[p.x, p.y] = null;
        }

        Piece target = board[move.to.x, move.to.y];
        if (target != null)
            Destroy(target.gameObject);

        board[move.from.x, move.from.y] = null;
        board[move.to.x, move.to.y] = piece;

        piece.boardPosition = move.to;
        piece.hasMoved = true;

        MovePieceToSquare(piece, move.to);

        if (matched.isCastling)
            HandleCastling(matched);

        if (matched.isPromotion && piece is Pawn pawn)
            StartPromotion(pawn);

        LastMove = matched;
        return true;
    }

    private void HandleCastling(Move castleMove)
    {
        Piece rook = board[castleMove.rookFrom.x, castleMove.rookFrom.y];
        if (rook == null) return;

        board[castleMove.rookFrom.x, castleMove.rookFrom.y] = null;
        board[castleMove.rookTo.x, castleMove.rookTo.y] = rook;

        rook.boardPosition = castleMove.rookTo;
        rook.hasMoved = true;

        MovePieceToSquare(rook, castleMove.rookTo);
    }

    private void MovePieceToSquare(Piece piece, Vector2Int pos)
    {
        if (!squaresByPos.ContainsKey(pos)) return;

        ChessSquare square = squaresByPos[pos];
        Vector3 current = piece.transform.position;
        Vector3 target  = square.transform.position;
        piece.transform.position = new Vector3(target.x, current.y, target.z);
    }

    private void StartPromotion(Pawn pawn)
    {
        isPromotionPending = true;
        promotionPos   = pawn.boardPosition;
        promotionColor = pawn.color;
        pawnObj        = pawn.gameObject;
        pawnParent     = pawn.transform.parent;
        pawnRot        = pawn.transform.rotation;
        pawnPos        = pawn.transform.position;
    }

    public bool CompletePromotion(PieceType type)
    {
        Destroy(pawnObj);

        GameObject prefab = promotionColor == PieceColor.White
            ? type == PieceType.Queen  ? whiteQueenPromotionPrefab  :
              type == PieceType.Rook   ? whiteRookPromotionPrefab   :
              type == PieceType.Bishop ? whiteBishopPromotionPrefab :
              whiteKnightPromotionPrefab
            : type == PieceType.Queen  ? blackQueenPromotionPrefab  :
              type == PieceType.Rook   ? blackRookPromotionPrefab   :
              type == PieceType.Bishop ? blackBishopPromotionPrefab :
              blackKnightPromotionPrefab;

        GameObject obj      = Instantiate(prefab, pawnPos, pawnRot, pawnParent);
        Piece      newPiece = obj.GetComponent<Piece>();
        newPiece.color         = promotionColor;
        newPiece.pieceType     = type;
        newPiece.boardPosition = promotionPos;

        board[promotionPos.x, promotionPos.y] = newPiece;
        isPromotionPending = false;
        return true;
    }
}