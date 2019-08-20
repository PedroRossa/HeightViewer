using UnityEngine;

public class Helper
{
    public static void Resize(ref Texture2D source, int newWidth, int newHeight, FilterMode filterMode = FilterMode.Point)
    {
        float ratio = Mathf.Min((float)newWidth / source.width, (float)newHeight / source.height);

        int nW = (int)(source.width * ratio);
        int nH = (int)(source.height * ratio);
        
        source.filterMode = filterMode;
        RenderTexture rt = RenderTexture.GetTemporary(nW, nH);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        source = new Texture2D(nW, nH);
        source.ReadPixels(new Rect(0, 0, nW, nH), 0, 0);
        source.Apply();
        RenderTexture.active = null;
    }

    public static void Resize(ref Texture2D source, int maxWidth, FilterMode filterMode = FilterMode.Point)
    {
        float factor = source.width > source.height ? ((float)maxWidth / source.width) : ((float)maxWidth / source.height);
        int newWidth = (int)(source.width * factor);
        int newHeight = (int)(source.height * factor);

        source.filterMode = filterMode;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        source = new Texture2D(newWidth, newHeight);
        source.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
        source.Apply();
        RenderTexture.active = null;
    }
}


