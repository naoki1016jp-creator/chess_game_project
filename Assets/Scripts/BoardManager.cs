using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Piece[,] board = new Piece[8, 8];

    [Header("昇格用 Prefab")]
    public GameObject whiteQueenPromotionPrefab;
    public GameObject blackQueenPromotionPrefab;
    public GameObject whiteRookPromotionPrefab;
    public GameObject blackRookPromotionPrefab;
    public GameObject whiteBishopPromotionPrefab;
    public GameObject blackBishopPromotionPrefab;
    public GameObject whiteKnightPromotionPrefab;
    public GameObject blackKnightPromotionPrefab;

    private Dictionary<Vector2Int, ChessSquare> squaresByPos = new Dictionary<Vector2Int, ChessSquare>();
    private Dictionary<string, ChessSquare> squaresByName = new Dictionary<string, ChessSquare>();

    // 昇格待ち
    public bool IsPromotionPending => isPromotionPending;

    private bool isPromotionPending = false;
    private Vector2Int pendingPromotionPos;
    private PieceColor pendingPromotionColor;
    private GameObject pendingPawnObject;
    private Transform pendingPawnParent;
    private Quaternion pendingPawnRotation;
    private Vector3 pendingPawnPosition;

    private void Start()
    {
        RegisterSquares();
        RegisterScenePieces();
    }

    private void RegisterSquares()
    {
        squaresByPos.Clear();
        squaresByName.Clear();

#if UNITY_2023_1_OR_NEWER
        ChessSquare[] squares = FindObjectsByType<ChessSquare>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        ChessSquare[] squares = FindObjectsOfType<ChessSquare>(true);
#endif

        Debug.Log($"見つかった ChessSquare 数: {squares.Length}");

        foreach (ChessSquare square in squares)
        {
            if (!squaresByPos.ContainsKey(square.boardPosition))
                squaresByPos.Add(square.boardPosition, square);

            string key = square.squareName.ToLower();
            if (!squaresByName.ContainsKey(key))
                squaresByName.Add(key, square);
        }

        Debug.Log($"マス登録完了: {squaresByPos.Count} マス");
    }

    private void RegisterScenePieces()
    {
        board = new Piece[8, 8];

        Piece[] pieces = GetComponentsInChildren<Piece>(true);

        foreach (Piece piece in pieces)
        {
            ChessSquare nearest = GetNearestSquare(piece.transform.position);

            if (nearest == null)
            {
                Debug.LogWarning($"{piece.name} の近くにマスが見つかりません");
                continue;
            }

            Vector2Int pos = nearest.boardPosition;

            if (!Inside(pos))
            {
                Debug.LogWarning($"{piece.name} は盤面外です: {pos}");
                continue;
            }

            if (board[pos.x, pos.y] != null)
            {
                Debug.LogWarning($"同じマスに複数の駒があります: {pos}");
                continue;
            }

            piece.boardPosition = pos;
            board[pos.x, pos.y] = piece;

            Debug.Log($"{piece.name} -> {nearest.squareName} ({pos.x}, {pos.y})");
        }
    }

    private ChessSquare GetNearestSquare(Vector3 worldPos)
    {
        ChessSquare nearest = null;
        float minDist = float.MaxValue;

        foreach (var pair in squaresByPos)
        {
            ChessSquare square = pair.Value;

            Vector2 pieceXZ = new Vector2(worldPos.x, worldPos.z);
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
        if (isPromotionPending)
        {
            Debug.Log("昇格待ち中なので移動できません");
            return false;
        }

        if (!Inside(move.from) || !Inside(move.to))
            return false;

        Piece piece = board[move.from.x, move.from.y];
        if (piece == null)
            return false;

        List<Move> legalMoves = piece.GetMoves(board);
        Move matchedMove = null;

        foreach (Move m in legalMoves)
        {
            if (m.to == move.to)
            {
                matchedMove = m;
                break;
            }
        }

        if (matchedMove == null)
            return false;

        Piece target = board[move.to.x, move.to.y];
        if (target != null)
        {
            Destroy(target.gameObject);
        }

        board[move.from.x, move.from.y] = null;
        board[move.to.x, move.to.y] = piece;

        piece.boardPosition = move.to;
        piece.hasMoved = true;

        MovePieceToSquare(piece, move.to);

        // キャスリング処理
        if (matchedMove.isCastling)
        {
            HandleCastling(matchedMove);
        }

        // 昇格待ちへ
        if (matchedMove.isPromotion && piece is Pawn pawn)
        {
            StartPromotion(pawn);
        }

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

        Debug.Log("キャスリングしました");
    }

    private void MovePieceToSquare(Piece piece, Vector2Int pos)
    {
        if (!squaresByPos.ContainsKey(pos))
        {
            Debug.LogWarning($"移動先マスが見つかりません: {pos}");
            return;
        }

        ChessSquare square = squaresByPos[pos];

        Vector3 current = piece.transform.position;
        Vector3 target = square.transform.position;
        piece.transform.position = new Vector3(target.x, current.y, target.z);
    }

    private void StartPromotion(Pawn pawn)
    {
        isPromotionPending = true;

        pendingPromotionPos = pawn.boardPosition;
        pendingPromotionColor = pawn.color;
        pendingPawnObject = pawn.gameObject;
        pendingPawnParent = pawn.transform.parent;
        pendingPawnRotation = pawn.transform.rotation;
        pendingPawnPosition = pawn.transform.position;

        Debug.Log("昇格待ちです。Q=Queen / R=Rook / B=Bishop / N=Knight");
    }

    public bool CompletePromotion(PieceType promotionType)
    {
        if (!isPromotionPending || pendingPawnObject == null)
            return false;

        if (promotionType != PieceType.Queen &&
            promotionType != PieceType.Rook &&
            promotionType != PieceType.Bishop &&
            promotionType != PieceType.Knight)
        {
            Debug.LogWarning("無効な昇格先です");
            return false;
        }

        board[pendingPromotionPos.x, pendingPromotionPos.y] = null;

        GameObject prefab = GetPromotionPrefab(pendingPromotionColor, promotionType);
        if (prefab == null)
        {
            Debug.LogWarning($"昇格用Prefabが設定されていません: {pendingPromotionColor} {promotionType}");
            return false;
        }

        Destroy(pendingPawnObject);

        GameObject obj = Instantiate(
            prefab,
            pendingPawnPosition,
            pendingPawnRotation,
            pendingPawnParent
        );

        Piece newPiece = obj.GetComponent<Piece>();
        newPiece.color = pendingPromotionColor;
        newPiece.boardPosition = pendingPromotionPos;
        newPiece.hasMoved = true;
        newPiece.pieceType = promotionType;

        board[pendingPromotionPos.x, pendingPromotionPos.y] = newPiece;
        MovePieceToSquare(newPiece, pendingPromotionPos);

        Debug.Log($"{obj.name} が {promotionType} に昇格しました");

        isPromotionPending = false;
        pendingPawnObject = null;

        return true;
    }

    private GameObject GetPromotionPrefab(PieceColor color, PieceType type)
    {
        if (color == PieceColor.White)
        {
            switch (type)
            {
                case PieceType.Queen: return whiteQueenPromotionPrefab;
                case PieceType.Rook: return whiteRookPromotionPrefab;
                case PieceType.Bishop: return whiteBishopPromotionPrefab;
                case PieceType.Knight: return whiteKnightPromotionPrefab;
            }
        }
        else
        {
            switch (type)
            {
                case PieceType.Queen: return blackQueenPromotionPrefab;
                case PieceType.Rook: return blackRookPromotionPrefab;
                case PieceType.Bishop: return blackBishopPromotionPrefab;
                case PieceType.Knight: return blackKnightPromotionPrefab;
            }
        }

        return null;
    }

    public ChessSquare GetSquare(string squareName)
    {
        squareName = squareName.ToLower();

        if (squaresByName.ContainsKey(squareName))
            return squaresByName[squareName];

        return null;
    }

    private bool Inside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }
}