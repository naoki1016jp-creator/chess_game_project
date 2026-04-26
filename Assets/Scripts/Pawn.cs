using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    // 直前ターンに2マス前進したときtrueになる（アンパッサン権）
    public bool enPassantTarget = false;

    private void Awake()
    {
        pieceType = PieceType.Pawn;
    }

    public override List<Move> GetMoves(Piece[,] board)
    {
        List<Move> moves = new List<Move>();

        int dir          = (color == PieceColor.White) ? 1 : -1;
        int startRow     = (color == PieceColor.White) ? 1 : 6;
        int promotionRow = (color == PieceColor.White) ? 7 : 0;

        int x = boardPosition.x;
        int y = boardPosition.y;

        // 1マス前進（空きのみ）
        if (Inside(x, y + dir) && Empty(board, x, y + dir))
        {
            Move move = new Move(boardPosition, new Vector2Int(x, y + dir));
            if (y + dir == promotionRow) move.isPromotion = true;
            moves.Add(move);

            // 2マス前進（初期位置かつ両マス空き）
            if (!hasMoved && y == startRow && Inside(x, y + dir * 2) && Empty(board, x, y + dir * 2))
            {
                Move doubleMove = new Move(boardPosition, new Vector2Int(x, y + dir * 2));
                doubleMove.isPawnDouble = true;
                moves.Add(doubleMove);
            }
        }

        // 斜め前キャプチャ＋アンパッサン
        foreach (int dx in new int[] { -1, 1 })
        {
            int nx = x + dx;
            int ny = y + dir;

            if (!Inside(nx, ny)) continue;

            // 通常キャプチャ
            if (Enemy(board, nx, ny))
            {
                Move move = new Move(boardPosition, new Vector2Int(nx, ny));
                if (ny == promotionRow) move.isPromotion = true;
                moves.Add(move);
            }
            // アンパッサン：斜め前が空き、かつ横隣に敵ポーンのアンパッサン権あり
            else if (Empty(board, nx, ny) && Inside(nx, y))
            {
                Piece adjacent = board[nx, y];
                if (adjacent != null &&
                    adjacent.color != color &&
                    adjacent is Pawn adjacentPawn &&
                    adjacentPawn.enPassantTarget)
                {
                    Move ep = new Move(boardPosition, new Vector2Int(nx, ny));
                    ep.isEnPassant     = true;
                    ep.enPassantCapture = new Vector2Int(nx, y);
                    moves.Add(ep);
                }
            }
        }

        return moves;
    }
}