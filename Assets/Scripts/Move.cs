using UnityEngine;

public class Move
{
    public Vector2Int from;
    public Vector2Int to;

    public bool isPromotion  = false;
    public bool isCastling   = false;
    public bool isPawnDouble = false; // 2マス前進（アンパッサン権発生）

    // アンパッサン
    public bool isEnPassant = false;
    public Vector2Int enPassantCapture; // 取り除くポーンの座標

    // キャスリング
    public Vector2Int rookFrom;
    public Vector2Int rookTo;

    public PieceType promotionType = PieceType.Queen;

    public Move(Vector2Int from, Vector2Int to)
    {
        this.from = from;
        this.to   = to;
    }
}