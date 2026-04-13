using UnityEngine;

public static class ChessRules
{
    public static bool IsSquareAttacked(Piece[,] board, Vector2Int square, PieceColor attackerColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = board[x, y];
                if (piece == null || piece.color != attackerColor)
                    continue;

                Vector2Int from = new Vector2Int(x, y);

                switch (piece.pieceType)
                {
                    case PieceType.Pawn:
                    {
                        int dir = (attackerColor == PieceColor.White) ? 1 : -1;

                        if (square == new Vector2Int(x - 1, y + dir) ||
                            square == new Vector2Int(x + 1, y + dir))
                        {
                            return true;
                        }
                        break;
                    }

                    case PieceType.Knight:
                    {
                        Vector2Int[] offsets =
                        {
                            new Vector2Int(1, 2), new Vector2Int(2, 1),
                            new Vector2Int(2, -1), new Vector2Int(1, -2),
                            new Vector2Int(-1, -2), new Vector2Int(-2, -1),
                            new Vector2Int(-2, 1), new Vector2Int(-1, 2)
                        };

                        foreach (var o in offsets)
                        {
                            if (square == from + o)
                                return true;
                        }
                        break;
                    }

                    case PieceType.Bishop:
                        if (RayAttacks(board, from, square, true, false)) return true;
                        break;

                    case PieceType.Rook:
                        if (RayAttacks(board, from, square, false, true)) return true;
                        break;

                    case PieceType.Queen:
                        if (RayAttacks(board, from, square, true, true)) return true;
                        break;

                    case PieceType.King:
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0) continue;

                                if (square == new Vector2Int(x + dx, y + dy))
                                    return true;
                            }
                        }
                        break;
                    }
                }
            }
        }

        return false;
    }

    private static bool RayAttacks(Piece[,] board, Vector2Int from, Vector2Int target, bool diagonal, bool straight)
    {
        Vector2Int[] dirs;

        if (diagonal && straight)
        {
            dirs = new Vector2Int[]
            {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1),
                new Vector2Int(1,1), new Vector2Int(1,-1),
                new Vector2Int(-1,1), new Vector2Int(-1,-1)
            };
        }
        else if (diagonal)
        {
            dirs = new Vector2Int[]
            {
                new Vector2Int(1,1), new Vector2Int(1,-1),
                new Vector2Int(-1,1), new Vector2Int(-1,-1)
            };
        }
        else
        {
            dirs = new Vector2Int[]
            {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1)
            };
        }

        foreach (var d in dirs)
        {
            int x = from.x + d.x;
            int y = from.y + d.y;

            while (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                Vector2Int current = new Vector2Int(x, y);

                if (current == target)
                    return true;

                if (board[x, y] != null)
                    break;

                x += d.x;
                y += d.y;
            }
        }

        return false;
    }

    public static bool CanCastleKingSide(Piece[,] board, King king)
    {
        if (king == null) return false;
        if (king.hasMoved) return false;

        int x = king.boardPosition.x;
        int y = king.boardPosition.y;

        // 白は e1(4,0)、黒は e8(4,7) からのみ
        if (x != 4) return false;
        if (king.color == PieceColor.White && y != 0) return false;
        if (king.color == PieceColor.Black && y != 7) return false;

        PieceColor enemy = (king.color == PieceColor.White) ? PieceColor.Black : PieceColor.White;

        // h列のルーク確認
        if (!(board[7, y] is Rook rook) || rook.color != king.color || rook.hasMoved)
            return false;

        // 間に駒がない
        if (board[5, y] != null || board[6, y] != null)
            return false;

        // 現在地、通過マス、到着マスが攻撃されていない
        if (IsSquareAttacked(board, new Vector2Int(4, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(5, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(6, y), enemy)) return false;

        return true;
    }

    public static bool CanCastleQueenSide(Piece[,] board, King king)
    {
        if (king == null) return false;
        if (king.hasMoved) return false;

        int x = king.boardPosition.x;
        int y = king.boardPosition.y;

        // 白は e1(4,0)、黒は e8(4,7) からのみ
        if (x != 4) return false;
        if (king.color == PieceColor.White && y != 0) return false;
        if (king.color == PieceColor.Black && y != 7) return false;

        PieceColor enemy = (king.color == PieceColor.White) ? PieceColor.Black : PieceColor.White;

        // a列のルーク確認
        if (!(board[0, y] is Rook rook) || rook.color != king.color || rook.hasMoved)
            return false;

        // 間に駒がない
        if (board[1, y] != null || board[2, y] != null || board[3, y] != null)
            return false;

        // 現在地、通過マス、到着マスが攻撃されていない
        if (IsSquareAttacked(board, new Vector2Int(4, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(3, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(2, y), enemy)) return false;

        return true;
    }
}
