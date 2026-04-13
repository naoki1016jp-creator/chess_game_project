using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public Camera targetCamera;

    [Header("視点位置")]
    public Transform whiteViewPoint;
    public Transform blackViewPoint;

    [Header("切り替え方法")]
    public bool instantSwitch = true;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    private Transform currentTarget;

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!instantSwitch && targetCamera != null && currentTarget != null)
        {
            targetCamera.transform.position = Vector3.Lerp(
                targetCamera.transform.position,
                currentTarget.position,
                Time.deltaTime * moveSpeed
            );

            targetCamera.transform.rotation = Quaternion.Slerp(
                targetCamera.transform.rotation,
                currentTarget.rotation,
                Time.deltaTime * rotateSpeed
            );
        }
    }

    public void SetView(PieceColor side)
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("ViewManager: targetCamera が設定されていません");
            return;
        }

        if (side == PieceColor.White)
        {
            currentTarget = whiteViewPoint;
        }
        else
        {
            currentTarget = blackViewPoint;
        }

        if (currentTarget == null)
        {
            Debug.LogWarning("ViewManager: 視点用 Transform が設定されていません");
            return;
        }

        if (instantSwitch)
        {
            targetCamera.transform.position = currentTarget.position;
            targetCamera.transform.rotation = currentTarget.rotation;
        }
    }

    public void ToggleView()
    {
        if (currentTarget == whiteViewPoint)
        {
            SetView(PieceColor.Black);
        }
        else
        {
            SetView(PieceColor.White);
        }
    }
}