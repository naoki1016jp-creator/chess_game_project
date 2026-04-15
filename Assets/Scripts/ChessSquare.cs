using UnityEngine;

public class ChessSquare : MonoBehaviour
{
    public string squareName;
    public Vector2Int boardPosition;

    private void Awake()
    {
        if (string.IsNullOrEmpty(squareName))
            squareName = gameObject.name.ToLower();

        ParseSquareName();
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(squareName))
            squareName = gameObject.name.ToLower();

        ParseSquareName();
    }

    private void ParseSquareName()
    {
        if (string.IsNullOrEmpty(squareName) || squareName.Length < 2)
            return;

        char fileChar = char.ToLower(squareName[0]);
        char rankChar = squareName[1];

        int x = fileChar - 'a';
        int y = rankChar - '1';

        boardPosition = new Vector2Int(x, y);
    }
}