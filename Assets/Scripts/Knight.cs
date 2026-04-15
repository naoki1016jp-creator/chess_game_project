using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    private void Awake()
    {
        pieceType = PieceType.Knight;

        maxHP = 12;
        currentHP = 12;
        attackPower = 4;
        defensePower = 2;
    }

    public override List<Move> GetMoves(Piece[,] board)
    {
        List<Move> moves = new List<Move>();

        Vector2Int[] offsets =
        {
            new Vector2Int(1, 2), new Vector2Int(2, 1),
            new Vector2Int(2, -1), new Vector2Int(1, -2),
            new Vector2Int(-1, -2), new Vector2Int(-2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-1, 2)
        };

        foreach (var o in offsets)
        {
            int x = boardPosition.x + o.x;
            int y = boardPosition.y + o.y;

            if (Inside(x, y) && Empty(board, x, y))
            {
                moves.Add(new Move(boardPosition, new Vector2Int(x, y)));
            }
        }

        return moves;
    }

    public override List<Vector2Int> GetAttackSquares(Piece[,] board)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int[] offsets =
        {
            new Vector2Int(1, 2), new Vector2Int(2, 1),
            new Vector2Int(2, -1), new Vector2Int(1, -2),
            new Vector2Int(-1, -2), new Vector2Int(-2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-1, 2)
        };

        foreach (var o in offsets)
        {
            int x = boardPosition.x + o.x;
            int y = boardPosition.y + o.y;

            if (Inside(x, y))
            {
                result.Add(new Vector2Int(x, y));
            }
        }

        return result;
    }

    public override List<Vector2Int> GetSupportSquares(Piece[,] board)
    {
        // 八角形支援範囲
        return GetOctagonSquares(2);
    }
}