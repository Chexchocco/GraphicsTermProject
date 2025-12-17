using UnityEngine;
using UnityEngine.UI;

public class AntiAliasingGround : MonoBehaviour
{
    [Header("Rendering")]
    public RenderTexture gameViewRT;
    public RawImage displayImage;

    [Header("Resolution Settings")]
    public int pixelResWidth = 72;  // 해제 시 해상도
    public int fullResWidth = 1920;  // 작동 시 해상도

    private bool isHighRes = false;

    void Start()
    {
        UpdateGimmickState(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isHighRes = !isHighRes;
            UpdateGimmickState(isHighRes);
        }
    }

    void UpdateGimmickState(bool highQuality)
    {
        gameViewRT.Release();

        if (highQuality)// 안티앨리어싱
        {
            gameViewRT.width = fullResWidth;
            gameViewRT.height = Mathf.RoundToInt(fullResWidth * 9f / 16f);
            gameViewRT.filterMode = FilterMode.Bilinear;
        }
        else// 해제 시 길이 해상도 낮아지고, 보간 해제해서 길 연결되게 할 수 있음.
        {
            gameViewRT.width = pixelResWidth;
            gameViewRT.height = Mathf.RoundToInt(pixelResWidth * 9f / 16f);
            gameViewRT.filterMode = FilterMode.Point;
        }
    }
}