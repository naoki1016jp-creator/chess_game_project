using UnityEngine;

public class ChessGameManager : MonoBehaviour
{
    [Header("Settings")]
    public PieceColor currentTurn = PieceColor.White;
    private ChessPiece selectedPiece;
    private Color originalColor;

    [Header("Cameras")]
    public Camera camera1p;
    public Camera camera2p;

    void Start()
    {
        if (Display.displays.Length > 1) Display.displays[1].Activate();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleMouseClick();
    }

    void HandleMouseClick()
    {
        
        RaycastHit hit;
        Ray ray = camera1p.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
{
    // これを追加！当たったものの名前をコンソールに出す
    Debug.Log($"<color=yellow>当たったオブジェクト:</color> {hit.collider.gameObject.name}");
    
    HandleSelection(hit.collider.gameObject);
}
        if (Physics.Raycast(ray, out hit))
        {
            HandleSelection(hit.collider.gameObject);
        }
        else if (camera2p != null)
        {
            Ray ray2 = camera2p.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray2, out hit))
            {
                HandleSelection(hit.collider.gameObject);
            }
        }
    }

    void HandleSelection(GameObject clickedObj)
    {
        ChessPiece clickedPiece = clickedObj.GetComponent<ChessPiece>();
        Tile clickedTile = clickedObj.GetComponent<Tile>();

        // 1. 自分の駒を選択
        if (clickedPiece != null && clickedPiece.color == currentTurn)
        {
            if (selectedPiece != null) ResetPieceColor();
            selectedPiece = clickedPiece;
            HighlightPiece(selectedPiece.gameObject);
            Debug.Log($"選択: {selectedPiece.type} at {selectedPiece.gridPos}");
            return;
        }

        // 2. 移動先の座標を決定
        Vector2Int targetPos;
        if (clickedPiece != null) targetPos = clickedPiece.gridPos;
        else if (clickedTile != null) targetPos = clickedTile.gridPos;
        else return;

        // 3. 移動実行
        if (selectedPiece != null)
        {
            if (selectedPiece.CanMoveTo(targetPos, clickedPiece) && IsPathClear(selectedPiece.gridPos, targetPos))
            {
                ExecuteMove(clickedObj, clickedPiece, targetPos);
            }
            else
            {
                Debug.LogWarning("ルール違反または道が塞がっています。");
                ResetPieceColor();
                selectedPiece = null;
            }
        }
    }

    void ExecuteMove(GameObject targetObj, ChessPiece enemyPiece, Vector2Int targetPos)
    {
        if (enemyPiece != null) Destroy(enemyPiece.gameObject);

        // 物理的な位置を吸着（クリックしたマスの中心へ、高さ0.5固定）
        Vector3 newPos = targetObj.transform.position;
        selectedPiece.transform.position = new Vector3(newPos.x, 0.5f, newPos.z);
        
        // 数値座標の更新
        selectedPiece.gridPos = targetPos;

        ResetPieceColor();
        currentTurn = (currentTurn == PieceColor.White) ? PieceColor.Black : PieceColor.White;
        selectedPiece = null;
    }

    bool IsPathClear(Vector2Int start, Vector2Int target)
{
    // ナイトは飛び越えるのでチェック不要
    if (selectedPiece.type == PieceType.Knight) return true;

    Vector2Int diff = target - start;
    // 進む方向を 1 または -1 または 0 で取得
    Vector2Int dir = new Vector2Int(
        diff.x == 0 ? 0 : (diff.x > 0 ? 1 : -1),
        diff.y == 0 ? 0 : (diff.y > 0 ? 1 : -1)
    );
    
    Vector2Int cur = start + dir;

    // 目的地に到達する手前まで、データ上の gridPos をチェック
    while (cur != target)
    {
        // ここがポイント：物理的な接触ではなく、全駒の gridPos 数値だけを検索
        if (GetPieceAt(cur) != null) 
        {
            Debug.Log($"<color=orange>道が塞がっています:</color> 座標 {cur} に他の駒のデータがあります。");
            return false;
        }
        cur += dir;
    }
    return true;
}


    ChessPiece GetPieceAt(Vector2Int coords)
    {
        // 実行速度が速い新しいメソッドに書き換え
ChessPiece[] allPieces = Object.FindObjectsByType<ChessPiece>(FindObjectsSortMode.None);
        foreach (var p in allPieces)
        {
            if (p.gridPos == coords && p != selectedPiece) return p;
        }
        return null;
    }

    void HighlightPiece(GameObject piece)
    {
        var renderer = piece.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            originalColor = renderer.material.color;
            renderer.material.color = Color.red;
        }
    }

    void ResetPieceColor()
    {
        if (selectedPiece != null)
        {
            var renderer = selectedPiece.GetComponentInChildren<MeshRenderer>();
            if (renderer != null) renderer.material.color = originalColor;
        }
    }
}