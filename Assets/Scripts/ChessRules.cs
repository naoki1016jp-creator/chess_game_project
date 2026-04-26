using System.Collections.Generic;
using UnityEngine;

public static class ChessRules
{
    // =========================
    // チェック判定（実ボード用）
    // =========================
    public static bool IsSquareAttacked(Piece[,] board, Vector2Int square, PieceColor attackerColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = board[x, y];
                if (piece == null || piece.color != attackerColor) continue;

                Vector2Int from = new Vector2Int(x, y);

                switch (piece.pieceType)
                {
                    case PieceType.Pawn:
                    {
                        int dir = (attackerColor == PieceColor.White) ? 1 : -1;
                        if (square == new Vector2Int(x - 1, y + dir) ||
                            square == new Vector2Int(x + 1, y + dir))
                            return true;
                        break;
                    }
                    case PieceType.Knight:
                    {
                        int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
                        int[] dy = { 2, 1, -1, -2, -2, -1, 1, 2 };
                        for (int i = 0; i < 8; i++)
                            if (square == from + new Vector2Int(dx[i], dy[i])) return true;
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
                        for (int ddx = -1; ddx <= 1; ddx++)
                            for (int ddy = -1; ddy <= 1; ddy++)
                            {
                                if (ddx == 0 && ddy == 0) continue;
                                if (square == new Vector2Int(x + ddx, y + ddy)) return true;
                            }
                        break;
                }
            }
        }

        return false;
    }

    public static bool IsInCheck(Piece[,] board, PieceColor color)
    {
        PieceColor enemy = Opposite(color);

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                Piece p = board[x, y];
                if (p != null && p.color == color && p.pieceType == PieceType.King)
                    return IsSquareAttacked(board, new Vector2Int(x, y), enemy);
            }

        return false;
    }

    private static bool RayAttacks(Piece[,] board, Vector2Int from, Vector2Int target, bool diagonal, bool straight)
    {
        Vector2Int[] dirs;

        if (diagonal && straight)
            dirs = new Vector2Int[] {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1),
                new Vector2Int(1,1), new Vector2Int(1,-1),
                new Vector2Int(-1,1), new Vector2Int(-1,-1)
            };
        else if (diagonal)
            dirs = new Vector2Int[] {
                new Vector2Int(1,1), new Vector2Int(1,-1),
                new Vector2Int(-1,1), new Vector2Int(-1,-1)
            };
        else
            dirs = new Vector2Int[] {
                new Vector2Int(1,0), new Vector2Int(-1,0),
                new Vector2Int(0,1), new Vector2Int(0,-1)
            };

        foreach (var d in dirs)
        {
            int x = from.x + d.x;
            int y = from.y + d.y;

            while (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                if (new Vector2Int(x, y) == target) return true;
                if (board[x, y] != null) break;
                x += d.x;
                y += d.y;
            }
        }

        return false;
    }

    // =========================
    // 合法手フィルタ（SimBoardで仮移動してチェック判定）
    // =========================
    public static List<Move> GetLegalMoves(Piece[,] board, Piece piece)
    {
        List<Move> pseudoLegal = piece.GetMoves(board);
        List<Move> legal       = new List<Move>();

        foreach (Move move in pseudoLegal)
        {
            SimBoard sim = SimBoard.From(board, move);
            if (!sim.IsInCheck(piece.color))
                legal.Add(move);
        }

        return legal;
    }

    // =========================
    // チェックメイト / ステールメイト判定
    // =========================
    public static bool HasAnyLegalMove(Piece[,] board, PieceColor color)
    {
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                Piece p = board[x, y];
                if (p == null || p.color != color) continue;
                if (GetLegalMoves(board, p).Count > 0) return true;
            }

        return false;
    }

    // =========================
    // キャスリング可否
    // =========================
    public static bool CanCastleKingSide(Piece[,] board, King king)
    {
        if (king == null || king.hasMoved) return false;

        int x = king.boardPosition.x;
        int y = king.boardPosition.y;

        if (x != 4) return false;
        if (king.color == PieceColor.White && y != 0) return false;
        if (king.color == PieceColor.Black && y != 7) return false;

        PieceColor enemy = Opposite(king.color);

        if (!(board[7, y] is Rook rook) || rook.color != king.color || rook.hasMoved)
            return false;
        if (board[5, y] != null || board[6, y] != null)
            return false;

        if (IsSquareAttacked(board, new Vector2Int(4, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(5, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(6, y), enemy)) return false;

        return true;
    }

    public static bool CanCastleQueenSide(Piece[,] board, King king)
    {
        if (king == null || king.hasMoved) return false;

        int x = king.boardPosition.x;
        int y = king.boardPosition.y;

        if (x != 4) return false;
        if (king.color == PieceColor.White && y != 0) return false;
        if (king.color == PieceColor.Black && y != 7) return false;

        PieceColor enemy = Opposite(king.color);

        if (!(board[0, y] is Rook rook) || rook.color != king.color || rook.hasMoved)
            return false;
        if (board[1, y] != null || board[2, y] != null || board[3, y] != null)
            return false;

        if (IsSquareAttacked(board, new Vector2Int(4, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(3, y), enemy)) return false;
        if (IsSquareAttacked(board, new Vector2Int(2, y), enemy)) return false;

        return true;
    }

    public static PieceColor Opposite(PieceColor color)
    {
        return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }
}

// =========================
// シミュレーション用の純粋データ（MonoBehaviour不使用）
// =========================
public struct CellData
{
    public bool exists;
    public PieceColor color;
    public PieceType  pieceType;
}

public class SimBoard
{
    public CellData[,] cells = new CellData[8, 8];

    public static SimBoard From(Piece[,] board, Move move)
    {
        SimBoard sim = new SimBoard();

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                Piece p = board[x, y];
                if (p != null)
                    sim.cells[x, y] = new CellData { exists = true, color = p.color, pieceType = p.pieceType };
            }

        CellData moving = sim.cells[move.from.x, move.from.y];
        sim.cells[move.from.x, move.from.y] = default;
        sim.cells[move.to.x,   move.to.y]   = moving;

        if (move.isEnPassant)
            sim.cells[move.enPassantCapture.x, move.enPassantCapture.y] = default;

        if (move.isCastling)
        {
            CellData rook = sim.cells[move.rookFrom.x, move.rookFrom.y];
            sim.cells[move.rookFrom.x, move.rookFrom.y] = default;
            sim.cells[move.rookTo.x,   move.rookTo.y]   = rook;
        }

        return sim;
    }

    public bool IsInCheck(PieceColor color)
    {
        PieceColor enemy = color == PieceColor.White ? PieceColor.Black : PieceColor.White;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                CellData c = cells[x, y];
                if (c.exists && c.color == color && c.pieceType == PieceType.King)
                    return IsAttackedBy(new Vector2Int(x, y), enemy);
            }

        return false;
    }

    private bool IsAttackedBy(Vector2Int square, PieceColor attacker)
    {
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                CellData c = cells[x, y];
                if (!c.exists || c.color != attacker) continue;
                if (Attacks(c.pieceType, attacker, x, y, square)) return true;
            }

        return false;
    }

    private bool Attacks(PieceType type, PieceColor color, int fx, int fy, Vector2Int target)
    {
        switch (type)
        {
            case PieceType.Pawn:
            {
                int dir = (color == PieceColor.White) ? 1 : -1;
                return target == new Vector2Int(fx - 1, fy + dir) ||
                       target == new Vector2Int(fx + 1, fy + dir);
            }
            case PieceType.Knight:
            {
                int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
                int[] dy = { 2, 1, -1, -2, -2, -1, 1, 2 };
                for (int i = 0; i < 8; i++)
                    if (target == new Vector2Int(fx + dx[i], fy + dy[i])) return true;
                return false;
            }
            case PieceType.Bishop:  return RaySim(fx, fy, target, true,  false);
            case PieceType.Rook:    return RaySim(fx, fy, target, false, true);
            case PieceType.Queen:   return RaySim(fx, fy, target, true,  true);
            case PieceType.King:
                return Mathf.Abs(target.x - fx) <= 1 && Mathf.Abs(target.y - fy) <= 1 &&
                       !(target.x == fx && target.y == fy);
        }

        return false;
    }

    private bool RaySim(int fx, int fy, Vector2Int target, bool diagonal, bool straight)
    {
        int[,] dirs;

        if (diagonal && straight)
            dirs = new int[,] { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
        else if (diagonal)
            dirs = new int[,] { {1,1},{1,-1},{-1,1},{-1,-1} };
        else
            dirs = new int[,] { {1,0},{-1,0},{0,1},{0,-1} };

        for (int i = 0; i < dirs.GetLength(0); i++)
        {
            int x = fx + dirs[i, 0];
            int y = fy + dirs[i, 1];

            while (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                if (x == target.x && y == target.y) return true;
                if (cells[x, y].exists) break;
                x += dirs[i, 0];
                y += dirs[i, 1];
            }
        }

        return false;
    }
}