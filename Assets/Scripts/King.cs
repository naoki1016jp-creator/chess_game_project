using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    private void Awake()
    {
        pieceType = PieceType.King;
    }

    public override List<Move> GetMoves(Piece[,] board)
    {
        List<Move> moves = new List<Move>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int x = boardPosition.x + dx;
                int y = boardPosition.y + dy;

                if (Inside(x, y) && (Empty(board, x, y) || Enemy(board, x, y)))
                    moves.Add(new Move(boardPosition, new Vector2Int(x, y)));
            }
        }

        int row = boardPosition.y;

        if (ChessRules.CanCastleKingSide(board, this))
        {
            Move castleMove = new Move(boardPosition, new Vector2Int(6, row));
            castleMove.isCastling = true;
            castleMove.rookFrom = new Vector2Int(7, row);
            castleMove.rookTo = new Vector2Int(5, row);
            moves.Add(castleMove);
        }

        if (ChessRules.CanCastleQueenSide(board, this))
        {
            Move castleMove = new Move(boardPosition, new Vector2Int(2, row));
            castleMove.isCastling = true;
            castleMove.rookFrom = new Vector2Int(0, row);
            castleMove.rookTo = new Vector2Int(3, row);
            moves.Add(castleMove);
        }

        return moves;
    }
}