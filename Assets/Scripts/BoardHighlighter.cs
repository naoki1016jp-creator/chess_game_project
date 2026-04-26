using System.Collections.Generic;
using UnityEngine;

public class BoardHighlighter : MonoBehaviour
{
    private Dictionary<Vector2Int, SquareHighlighter> _highlighters
        = new Dictionary<Vector2Int, SquareHighlighter>();

    private List<Vector2Int> _currentlyLit = new List<Vector2Int>();

    private void Awake()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        _highlighters.Clear();

        ChessSquare[] squares = FindObjectsByType<ChessSquare>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Debug.Log($"[BoardHighlighter] ChessSquare数: {squares.Length}");

        foreach (ChessSquare sq in squares)
        {
            SquareHighlighter hl = sq.GetComponent<SquareHighlighter>();
            if (hl == null)
                hl = sq.gameObject.AddComponent<SquareHighlighter>();

            if (!_highlighters.ContainsKey(sq.boardPosition))
                _highlighters.Add(sq.boardPosition, hl);
        }

        Debug.Log($"[BoardHighlighter] {_highlighters.Count} マスをキャッシュしました");
    }

    public void ShowMoves(Piece piece, Piece[,] board)
    {
        ClearAll();

        Highlight(piece.boardPosition, HighlightType.Selected);

        List<Move> moves = piece.GetMoves(board);

        foreach (Move move in moves)
        {
            Piece target = board[move.to.x, move.to.y];
            if (target != null && target.color != piece.color)
                Highlight(move.to, HighlightType.Capture);
            else
                Highlight(move.to, HighlightType.Move);
        }
    }

    public void ClearAll()
    {
        foreach (var pos in _currentlyLit)
        {
            if (_highlighters.TryGetValue(pos, out SquareHighlighter hl))
                hl.ClearHighlight();
        }
        _currentlyLit.Clear();
    }

    private enum HighlightType { Selected, Move, Capture }

    private void Highlight(Vector2Int pos, HighlightType type)
    {
        if (!_highlighters.TryGetValue(pos, out SquareHighlighter hl))
        {
            Debug.LogWarning($"[BoardHighlighter] マスが見つかりません: {pos}");
            return;
        }

        switch (type)
        {
            case HighlightType.Selected: hl.HighlightSelected(); break;
            case HighlightType.Move:     hl.HighlightMove();     break;
            case HighlightType.Capture:  hl.HighlightCapture();  break;
        }

        if (!_currentlyLit.Contains(pos))
            _currentlyLit.Add(pos);
    }
}