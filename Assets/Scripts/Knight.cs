using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    private void Awake()
    {
        pieceType = PieceType.Knight;
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

            if (Inside(x, y) && (Empty(board, x, y) || Enemy(board, x, y)))
            {
                moves.Add(new Move(boardPosition, new Vector2Int(x, y)));
            }
        }

        return moves;
    }
}