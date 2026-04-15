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

    [Header("Combat Settings")]
    public int minimumDamage = 1;
    public int supportReduction = 1;

    private Dictionary<Vector2Int, ChessSquare> squaresByPos = new Dictionary<Vector2Int, ChessSquare>();
    private Dictionary<string, ChessSquare> squaresByName = new Dictionary<string, ChessSquare>();

    // 支援マップ
    private Dictionary<Vector2Int, int> whiteSupportMap = new Dictionary<Vector2Int, int>();
    private Dictionary<Vector2Int, int> blackSupportMap = new Dictionary<Vector2Int, int>();

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

        foreach (ChessSquare square in squares)
        {
            if (!squaresByPos.ContainsKey(square.boardPosition))
                squaresByPos.Add(square.boardPosition, square);

            string key = square.squareName.ToLower();
            if (!squaresByName.ContainsKey(key))
                squaresByName.Add(key, square);
        }
    }

    private void RegisterScenePieces()
    {
        board = new Piece[8, 8];

        Piece[] pieces = GetComponentsInChildren<Piece>(true);

        foreach (Piece piece in pieces)
        {
            ChessSquare nearest = GetNearestSquare(piece.transform.position);
            if (nearest == null) continue;

            Vector2Int pos = nearest.boardPosition;
            if (!Inside(pos)) continue;
            if (board[pos.x, pos.y] != null) continue;

            piece.boardPosition = pos;
            board[pos.x, pos.y] = piece;
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

    // =========================
    // 移動処理
    // =========================
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

        board[move.from.x, move.from.y] = null;
        board[move.to.x, move.to.y] = piece;

        piece.boardPosition = move.to;
        piece.hasMoved = true;

        MovePieceToSquare(piece, move.to);

        // キャスリング
        if (matchedMove.isCastling)
        {
            HandleCastling(matchedMove);
        }

        // 昇格待ち開始
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

    // =========================
    // 昇格処理
    // =========================
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

    // =========================
    // 支援フェーズ
    // =========================
    public void ResolveSupportPhase()
    {
        whiteSupportMap.Clear();
        blackSupportMap.Clear();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = board[x, y];
                if (piece == null) continue;

                foreach (Vector2Int pos in piece.GetSupportSquares(board))
                {
                    if (!Inside(pos)) continue;

                    Piece target = board[pos.x, pos.y];
                    if (target == null) continue;
                    if (target.color != piece.color) continue;
                    if (target.boardPosition == piece.boardPosition) continue;

                    var map = piece.color == PieceColor.White ? whiteSupportMap : blackSupportMap;

                    if (!map.ContainsKey(pos))
                        map[pos] = 0;

                    map[pos]++;
                }
            }
        }
    }

    public int GetSupportCount(Vector2Int pos, PieceColor color)
    {
        var map = color == PieceColor.White ? whiteSupportMap : blackSupportMap;
        return map.ContainsKey(pos) ? map[pos] : 0;
    }

    // =========================
    // 攻撃フェーズ
    // 攻撃範囲内の敵「全員」を攻撃
    // =========================
    public void ResolveAttackPhase(PieceColor attackingColor, GameManager gameManager)
    {
        List<Piece> attackers = new List<Piece>();

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Piece p = board[x, y];
                if (p != null && p.color == attackingColor)
                    attackers.Add(p);
            }
        }

        foreach (Piece attacker in attackers)
        {
            if (attacker == null) continue;
            if (!Inside(attacker.boardPosition)) continue;
            if (board[attacker.boardPosition.x, attacker.boardPosition.y] != attacker) continue;

            foreach (Piece target in GetAllAttackTargets(attacker))
            {
                if (target == null) continue;
                if (!Inside(target.boardPosition)) continue;
                if (board[target.boardPosition.x, target.boardPosition.y] != target) continue;

                bool wasKing = target.pieceType == PieceType.King;

                int support = GetSupportCount(target.boardPosition, target.color);
                int damage = attacker.attackPower - target.defensePower - (support * supportReduction);
                if (damage < minimumDamage) damage = minimumDamage;

                target.TakeDamage(damage);

                if (target.IsDead())
                {
                    Vector2Int pos = target.boardPosition;
                    Destroy(target.gameObject);
                    board[pos.x, pos.y] = null;

                    if (wasKing)
                    {
                        gameManager.EndGame(attackingColor);
                        return;
                    }
                }
            }
        }
    }

    private List<Piece> GetAllAttackTargets(Piece attacker)
    {
        HashSet<Piece> result = new HashSet<Piece>();

        foreach (Vector2Int pos in attacker.GetAttackSquares(board))
        {
            if (!Inside(pos)) continue;

            Piece target = board[pos.x, pos.y];
            if (target == null) continue;
            if (target.color == attacker.color) continue;

            result.Add(target);
        }

        return new List<Piece>(result);
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