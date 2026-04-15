using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    private void Awake()
    {
        pieceType = PieceType.Rook;

        maxHP = 18;
        currentHP = 18;
        attackPower = 5;
        defensePower = 3;
    }

    public override List<Move> GetMoves(Piece[,] board)
    {
        List<Move> moves = new List<Move>();

        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var d in dirs)
        {
            int x = boardPosition.x + d.x;
            int y = boardPosition.y + d.y;

            while (Inside(x, y))
            {
                if (Empty(board, x, y))
                {
                    moves.Add(new Move(boardPosition, new Vector2Int(x, y)));
                }
                else
                {
                    break;
                }

                x += d.x;
                y += d.y;
            }
        }

        return moves;
    }

    public override List<Vector2Int> GetAttackSquares(Piece[,] board)
    {
        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        return GetRayAttackSquares(board, dirs);
    }

    public override List<Vector2Int> GetSupportSquares(Piece[,] board)
    {
        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        return GetRaySupportSquares(board, dirs);
    }
}