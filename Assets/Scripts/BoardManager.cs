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

    private Dictionary<Vector2Int, ChessSquare> squaresByPos  = new Dictionary<Vector2Int, ChessSquare>();
    private Dictionary<string,     ChessSquare> squaresByName = new Dictionary<string, ChessSquare>();

    public bool IsPromotionPending => isPromotionPending;

    private bool      isPromotionPending  = false;
    private Vector2Int pendingPromotionPos;
    private PieceColor pendingPromotionColor;
    private GameObject pendingPawnObject;
    private Transform  pendingPawnParent;
    private Quaternion pendingPawnRotation;
    private Vector3    pendingPawnPosition;

    // アンパッサン：直前ターンに2マス前進したポーン
    private Pawn lastDoubleStepPawn = null;

    private void Start()
    {
        RegisterSquares();
        RegisterScenePieces();
    }

    // =========================
    // 初期化
    // =========================
    private void RegisterSquares()
    {
        squaresByPos.Clear();
        squaresByName.Clear();

#if UNITY_2023_1_OR_NEWER
        ChessSquare[] squares = FindObjectsByType<ChessSquare>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        ChessSquare[] squares = FindObjectsOfType<ChessSquare>(true);
#endif

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
            if (nearest == null) { Debug.LogWarning($"{piece.name} の近くにマスが見つかりません"); continue; }

            Vector2Int pos = nearest.boardPosition;
            if (!Inside(pos))          { Debug.LogWarning($"{piece.name} は盤面外: {pos}"); continue; }
            if (board[pos.x, pos.y] != null) { Debug.LogWarning($"同じマスに複数の駒: {pos}"); continue; }

            piece.boardPosition    = pos;
            board[pos.x, pos.y]   = piece;

            Debug.Log($"{piece.name} -> {nearest.squareName} ({pos.x}, {pos.y})");
        }
    }

    private ChessSquare GetNearestSquare(Vector3 worldPos)
    {
        ChessSquare nearest = null;
        float minDist = float.MaxValue;

        foreach (var pair in squaresByPos)
        {
            ChessSquare sq = pair.Value;
            float dist = Vector2.Distance(
                new Vector2(worldPos.x, worldPos.z),
                new Vector2(sq.transform.position.x, sq.transform.position.z)
            );
            if (dist < minDist) { minDist = dist; nearest = sq; }
        }

        return nearest;
    }

    // =========================
    // 移動処理
    // =========================
    public bool MovePiece(Move move)
    {
        if (isPromotionPending)            { Debug.Log("昇格待ち中なので移動できません"); return false; }
        if (!Inside(move.from) || !Inside(move.to)) return false;

        Piece piece = board[move.from.x, move.from.y];
        if (piece == null) return false;

        // 合法手チェック（チェック回避フィルタ込み）
        List<Move> legalMoves = ChessRules.GetLegalMoves(board, piece);
        Move matchedMove = null;

        foreach (Move m in legalMoves)
        {
            if (m.to == move.to)
            {
                matchedMove = m;
                break;
            }
        }

        if (matchedMove == null) return false;

        // アンパッサンフラグをリセット（毎ターン）
        if (lastDoubleStepPawn != null)
        {
            lastDoubleStepPawn.enPassantTarget = false;
            lastDoubleStepPawn = null;
        }

        // 通常キャプチャ
        Piece target = board[matchedMove.to.x, matchedMove.to.y];
        if (target != null)
        {
            Destroy(target.gameObject);
            board[matchedMove.to.x, matchedMove.to.y] = null;
        }

        // アンパッサンのキャプチャ
        if (matchedMove.isEnPassant)
        {
            Piece epTarget = board[matchedMove.enPassantCapture.x, matchedMove.enPassantCapture.y];
            if (epTarget != null)
            {
                Destroy(epTarget.gameObject);
                board[matchedMove.enPassantCapture.x, matchedMove.enPassantCapture.y] = null;
            }
        }

        // 駒を移動
        board[matchedMove.from.x, matchedMove.from.y] = null;
        board[matchedMove.to.x,   matchedMove.to.y]   = piece;
        piece.boardPosition = matchedMove.to;
        piece.hasMoved      = true;

        MovePieceToSquare(piece, matchedMove.to);

        // 2マス前進フラグ（アンパッサン権）
        if (matchedMove.isPawnDouble && piece is Pawn pd)
        {
            pd.enPassantTarget = true;
            lastDoubleStepPawn = pd;
        }

        // キャスリング
        if (matchedMove.isCastling)
            HandleCastling(matchedMove);

        // 昇格
        if (matchedMove.isPromotion && piece is Pawn pawn)
            StartPromotion(pawn);

        return true;
    }

    private void HandleCastling(Move castleMove)
    {
        Piece rook = board[castleMove.rookFrom.x, castleMove.rookFrom.y];
        if (rook == null) return;

        board[castleMove.rookFrom.x, castleMove.rookFrom.y] = null;
        board[castleMove.rookTo.x,   castleMove.rookTo.y]   = rook;
        rook.boardPosition = castleMove.rookTo;
        rook.hasMoved      = true;

        MovePieceToSquare(rook, castleMove.rookTo);
        Debug.Log("キャスリングしました");
    }

    private void MovePieceToSquare(Piece piece, Vector2Int pos)
    {
        if (!squaresByPos.ContainsKey(pos)) { Debug.LogWarning($"移動先マスが見つかりません: {pos}"); return; }

        ChessSquare square = squaresByPos[pos];
        Vector3 current    = piece.transform.position;
        Vector3 t          = square.transform.position;
        piece.transform.position = new Vector3(t.x, current.y, t.z);
    }

    // =========================
    // 昇格処理
    // =========================
    private void StartPromotion(Pawn pawn)
    {
        isPromotionPending   = true;
        pendingPromotionPos   = pawn.boardPosition;
        pendingPromotionColor = pawn.color;
        pendingPawnObject     = pawn.gameObject;
        pendingPawnParent     = pawn.transform.parent;
        pendingPawnRotation   = pawn.transform.rotation;
        pendingPawnPosition   = pawn.transform.position;

        Debug.Log("昇格待ちです。Q=Queen / R=Rook / B=Bishop / N=Knight");
    }

    public bool CompletePromotion(PieceType promotionType)
    {
        if (!isPromotionPending || pendingPawnObject == null) return false;

        if (promotionType != PieceType.Queen  &&
            promotionType != PieceType.Rook   &&
            promotionType != PieceType.Bishop &&
            promotionType != PieceType.Knight)
        { Debug.LogWarning("無効な昇格先です"); return false; }

        board[pendingPromotionPos.x, pendingPromotionPos.y] = null;

        GameObject prefab = GetPromotionPrefab(pendingPromotionColor, promotionType);
        if (prefab == null) { Debug.LogWarning($"昇格用Prefabが未設定: {pendingPromotionColor} {promotionType}"); return false; }

        Destroy(pendingPawnObject);

        GameObject obj      = Instantiate(prefab, pendingPawnPosition, pendingPawnRotation, pendingPawnParent);
        Piece      newPiece = obj.GetComponent<Piece>();
        newPiece.color         = pendingPromotionColor;
        newPiece.boardPosition = pendingPromotionPos;
        newPiece.hasMoved      = true;
        newPiece.pieceType     = promotionType;

        board[pendingPromotionPos.x, pendingPromotionPos.y] = newPiece;
        MovePieceToSquare(newPiece, pendingPromotionPos);

        Debug.Log($"{obj.name} が {promotionType} に昇格しました");

        isPromotionPending = false;
        pendingPawnObject  = null;

        return true;
    }

    private GameObject GetPromotionPrefab(PieceColor color, PieceType type)
    {
        if (color == PieceColor.White) switch (type)
        {
            case PieceType.Queen:  return whiteQueenPromotionPrefab;
            case PieceType.Rook:   return whiteRookPromotionPrefab;
            case PieceType.Bishop: return whiteBishopPromotionPrefab;
            case PieceType.Knight: return whiteKnightPromotionPrefab;
        }
        else switch (type)
        {
            case PieceType.Queen:  return blackQueenPromotionPrefab;
            case PieceType.Rook:   return blackRookPromotionPrefab;
            case PieceType.Bishop: return blackBishopPromotionPrefab;
            case PieceType.Knight: return blackKnightPromotionPrefab;
        }

        return null;
    }

    // =========================
    // ユーティリティ
    // =========================
    public ChessSquare GetSquare(string squareName)
    {
        squareName = squareName.ToLower();
        return squaresByName.ContainsKey(squareName) ? squaresByName[squareName] : null;
    }

    private bool Inside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }
}