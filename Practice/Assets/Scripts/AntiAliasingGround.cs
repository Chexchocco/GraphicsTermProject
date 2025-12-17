using UnityEngine;
using UnityEngine.UI;

public class AntiAliasingGround : MonoBehaviour
{
    [Header("Rendering")]
    public RenderTexture gameViewRT;
    public RawImage displayImage;

    [Header("Resolution Settings")]
    public int pixelResWidth = 72;
    public int fullResWidth = 1920;

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

        if (highQuality)
        {
            gameViewRT.width = fullResWidth;
            gameViewRT.height = Mathf.RoundToInt(fullResWidth * 9f / 16f);
            gameViewRT.filterMode = FilterMode.Bilinear;
        }
        else
        {
            gameViewRT.width = pixelResWidth;
            gameViewRT.height = Mathf.RoundToInt(pixelResWidth * 9f / 16f);
            gameViewRT.filterMode = FilterMode.Point;
        }
    }
}