using UnityEngine;

// 列挙型の定義（クラスの外に置くことでエラーを防ぎます）
public enum PieceType { Pawn, Knight, Bishop, Rook, Queen, King }
public enum PieceColor { White, Black }

public class ChessPiece : MonoBehaviour
{
    public PieceType type;
    public PieceColor color;
    public Vector2Int gridPos; // 実行時に自動で設定されます

    void Start()
    {
        UpdateGridPosFromTile();
    }

    // 足元のタイルから正しい座標を読み取る
    public void UpdateGridPosFromTile()
    {
        RaycastHit hit;
        // 自分の位置から真下にレイを飛ばしてタイルを探す（距離2.0以内）
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2.0f))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                gridPos = tile.gridPos;
            }
        }
    }

    public bool CanMoveTo(Vector2Int target, ChessPiece targetPiece)
    {
        if (targetPiece != null && targetPiece.color == this.color) return false;

        int diffX = Mathf.Abs(target.x - gridPos.x);
        int diffY = Mathf.Abs(target.y - gridPos.y);

        switch (type)
        {
            case PieceType.Pawn:
                int dir = (color == PieceColor.White) ? 1 : -1;
                // 白ポーンがy=1、黒ポーンがy=6にいる場合に2マス移動可能
                // あなたの配置が y=0 スタートなら、ここを 0 と 7 に変えてください
                int startRank = (color == PieceColor.White) ? 1 : 6; 

                // 1歩前進
                if (diffX == 0 && target.y == gridPos.y + dir && targetPiece == null) return true;
                // 初手2歩前進
                if (diffX == 0 && gridPos.y == startRank && target.y == gridPos.y + 2 * dir && targetPiece == null) return true;
                // 斜め取り
                if (diffX == 1 && target.y == gridPos.y + dir && targetPiece != null) return true;
                return false;

            case PieceType.Knight:
                return (diffX == 1 && diffY == 2) || (diffX == 2 && diffY == 1);
            case PieceType.Rook:
                return (gridPos.x == target.x || gridPos.y == target.y);
            case PieceType.Bishop:
                return (diffX == diffY);
            case PieceType.Queen:
                return (gridPos.x == target.x || gridPos.y == target.y) || (diffX == diffY);
            case PieceType.King:
                return (diffX <= 1 && diffY <= 1);
            default:
                return false;
        }
    }
}