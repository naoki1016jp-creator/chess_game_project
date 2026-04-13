using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    private void Awake()
    {
        pieceType = PieceType.Pawn;
    }

    public override List<Move> GetMoves(Piece[,] board)
    {
        List<Move> moves = new List<Move>();

        int dir = (color == PieceColor.White) ? 1 : -1;
        int startRow = (color == PieceColor.White) ? 1 : 6;
        int promotionRow = (color == PieceColor.White) ? 7 : 0;

        int x = boardPosition.x;
        int y = boardPosition.y;

        // 前に1マス
        if (Inside(x, y + dir) && Empty(board, x, y + dir))
        {
            Move move = new Move(boardPosition, new Vector2Int(x, y + dir));

            if (y + dir == promotionRow)
            {
                move.isPromotion = true;
            }

            moves.Add(move);

            // 初手2マス
            if (!hasMoved && y == startRow && Inside(x, y + dir * 2) && Empty(board, x, y + dir * 2))
            {
                moves.Add(new Move(boardPosition, new Vector2Int(x, y + dir * 2)));
            }
        }

        // 斜め取り
        foreach (int dx in new int[] { -1, 1 })
        {
            int nx = x + dx;
            int ny = y + dir;

            if (Inside(nx, ny) && Enemy(board, nx, ny))
            {
                Move move = new Move(boardPosition, new Vector2Int(nx, ny));

                if (ny == promotionRow)
                {
                    move.isPromotion = true;
                }

                moves.Add(move);
            }
        }

        return moves;
    }
}