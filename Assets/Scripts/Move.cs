using UnityEngine;

public class Move
{
    public Vector2Int from;
    public Vector2Int to;

    public bool isPromotion = false;
    public bool isCastling = false;
    public bool isEnPassant = false;

    public Vector2Int rookFrom;
    public Vector2Int rookTo;
    public Vector2Int enPassantCapturedPos;

    public PieceType promotionType = PieceType.Queen;

    public Move(Vector2Int from, Vector2Int to)
    {
        this.from = from;
        this.to = to;
    }
}