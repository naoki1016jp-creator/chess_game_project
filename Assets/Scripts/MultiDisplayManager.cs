using UnityEngine;

public class MultiDisplayManager : MonoBehaviour
{
    private void Start()
    {
        if (Display.displays.Length > 1)
        {
            // Display 2 を有効化
            Display.displays[1].Activate();
            Debug.Log("Display 2 を有効化しました");
        }

        if (Display.displays.Length > 2)
        {
            // 必要なら Display 3 以降も
            Display.displays[2].Activate();
        }
    }
}

