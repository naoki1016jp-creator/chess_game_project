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

        if (Inside(x, y + dir) && Empty(board, x, y + dir))
        {
            Move move = new Move(boardPosition, new Vector2Int(x, y + dir));
            if (y + dir == promotionRow) move.isPromotion = true;
            moves.Add(move);

            if (!hasMoved && y == startRow && Inside(x, y + dir * 2) && Empty(board, x, y + dir * 2))
                moves.Add(new Move(boardPosition, new Vector2Int(x, y + dir * 2)));
        }

        foreach (int dx in new int[] { -1, 1 })
        {
            int nx = x + dx;
            int ny = y + dir;

            if (Inside(nx, ny) && Enemy(board, nx, ny))
            {
                Move move = new Move(boardPosition, new Vector2Int(nx, ny));
                if (ny == promotionRow) move.isPromotion = true;
                moves.Add(move);
            }

            // アンパッサン
            Move lastMove = BoardManager.LastMove;
            if (lastMove != null &&
                board[lastMove.to.x, lastMove.to.y] != null &&
                board[lastMove.to.x, lastMove.to.y].pieceType == PieceType.Pawn &&
                board[lastMove.to.x, lastMove.to.y].color != color &&
                Mathf.Abs(lastMove.from.y - lastMove.to.y) == 2 &&
                lastMove.to.x == nx &&
                lastMove.to.y == y)
            {
                Move epMove = new Move(boardPosition, new Vector2Int(nx, ny));
                epMove.isEnPassant = true;
                epMove.enPassantCapturedPos = lastMove.to;
                moves.Add(epMove);
            }
        }

        return moves;
    }
}