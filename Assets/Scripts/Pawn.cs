using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    private void Awake()
    {
        pieceType = PieceType.Pawn;

        maxHP = 8;
        currentHP = 8;
        attackPower = 2;
        defensePower = 1;
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

            if (y + dir == promotionRow)
                move.isPromotion = true;

            moves.Add(move);

            if (!hasMoved && y == startRow && Inside(x, y + dir * 2) && Empty(board, x, y + dir * 2))
            {
                moves.Add(new Move(boardPosition, new Vector2Int(x, y + dir * 2)));
            }
        }

        return moves;
    }

    public override List<Vector2Int> GetAttackSquares(Piece[,] board)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        int dir = (color == PieceColor.White) ? 1 : -1;
        int x = boardPosition.x;
        int y = boardPosition.y;

        if (Inside(x - 1, y + dir))
            result.Add(new Vector2Int(x - 1, y + dir));

        if (Inside(x + 1, y + dir))
            result.Add(new Vector2Int(x + 1, y + dir));

        return result;
    }

    public override List<Vector2Int> GetSupportSquares(Piece[,] board)
    {
        return GetAttackSquares(board);
    }
}