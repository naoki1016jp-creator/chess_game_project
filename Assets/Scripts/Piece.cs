using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    public PieceColor color;
    public PieceType pieceType;
    public Vector2Int boardPosition;
    public bool hasMoved = false;

    [Header("Battle Status")]
    public int maxHP = 10;
    public int currentHP = 10;
    public int attackPower = 3;
    public int defensePower = 1;

    public abstract List<Move> GetMoves(Piece[,] board);

    // 攻撃可能マス
    public abstract List<Vector2Int> GetAttackSquares(Piece[,] board);

    // 支援可能マス
    public abstract List<Vector2Int> GetSupportSquares(Piece[,] board);

    protected bool Inside(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    protected bool Empty(Piece[,] board, int x, int y)
    {
        return board[x, y] == null;
    }

    protected bool Enemy(Piece[,] board, int x, int y)
    {
        return board[x, y] != null && board[x, y].color != color;
    }

    protected bool Ally(Piece[,] board, int x, int y)
    {
        return board[x, y] != null && board[x, y].color == color;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{name} が {damage} ダメージ受けた。残りHP: {currentHP}");
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }

    // 攻撃用：最初の駒まで
    protected List<Vector2Int> GetRayAttackSquares(Piece[,] board, Vector2Int[] dirs)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var d in dirs)
        {
            int x = boardPosition.x + d.x;
            int y = boardPosition.y + d.y;

            while (Inside(x, y))
            {
                result.Add(new Vector2Int(x, y));

                if (board[x, y] != null)
                    break;

                x += d.x;
                y += d.y;
            }
        }

        return result;
    }

    // 支援用：味方は貫通、敵で止まる
    protected List<Vector2Int> GetRaySupportSquares(Piece[,] board, Vector2Int[] dirs)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var d in dirs)
        {
            int x = boardPosition.x + d.x;
            int y = boardPosition.y + d.y;

            while (Inside(x, y))
            {
                result.Add(new Vector2Int(x, y));

                if (board[x, y] != null)
                {
                    // 敵にぶつかったら止まる
                    if (board[x, y].color != color)
                        break;

                    // 味方ならそのまま継続（貫通）
                }

                x += d.x;
                y += d.y;
            }
        }

        return result;
    }

    // 周囲1マス
    protected List<Vector2Int> GetAdjacentSquares()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int x = boardPosition.x + dx;
                int y = boardPosition.y + dy;

                if (Inside(x, y))
                {
                    result.Add(new Vector2Int(x, y));
                }
            }
        }

        return result;
    }

    // 八角形範囲（半径 radius）
    protected List<Vector2Int> GetOctagonSquares(int radius)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                // 四隅を除外 → 八角形
                if (Mathf.Abs(dx) == radius && Mathf.Abs(dy) == radius)
                    continue;

                int x = boardPosition.x + dx;
                int y = boardPosition.y + dy;

                if (Inside(x, y))
                {
                    result.Add(new Vector2Int(x, y));
                }
            }
        }

        return result;
    }
}