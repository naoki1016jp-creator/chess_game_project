
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    public PieceColor color;
    public PieceType pieceType;
    public Vector2Int boardPosition;
    public bool hasMoved = false;

    public abstract List<Move> GetMoves(Piece[,] board);

    protected bool Inside(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    protected bool Empty(Piece[,] board, int x, int y)
    {
        return board[x, y] == null;
    }

    protected bool Enemy(Piece[,] board, int x, int y)
    {
        return board[x, y] != null && board[x, y].color != color;
    }
}
