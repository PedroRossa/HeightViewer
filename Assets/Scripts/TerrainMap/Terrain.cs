
using UnityEngine;

public class Terrain
{
    #region Attributes

    private int width;
    private int height;
    private int maxHeight;
    private int diffFromTex_w;
    private int diffFromTex_h;
    private Texture2D texture;

    #endregion

    #region Constructors

    public Terrain(int width, int height, int maxHeight)
    {
        this.width = width;
        this.height = height;
        this.maxHeight = maxHeight;
    }

    #endregion

    #region Private Methods

    private void CalculateDiffFromTerrainToTexture()
    {
        diffFromTex_w = (width - texture.width) / 2;
        diffFromTex_h = (height - texture.height) / 2;
    }

    #endregion

    #region Public Methods

    public Texture2D GetTexture()
    {
        return texture;
    }

    public void SetTexture(Texture2D tex)
    {
        texture = tex;
        Helper.Resize(ref texture, width, height);
        CalculateDiffFromTerrainToTexture();
    }

    public Vector3[] CalculateVertices()
    {
        Vector3[] vertices = new Vector3[width * height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;
                if ((j - diffFromTex_w) < texture.width && j > diffFromTex_w &&
                    (i - diffFromTex_h) < texture.height && i > diffFromTex_h)
                {
                    float currHeight = texture.GetPixel(j - diffFromTex_w, i - diffFromTex_h).r;
                    vertices[currIndex] = new Vector3(j, currHeight * maxHeight, i);
                }
                else
                {
                    vertices[currIndex] = new Vector3(j, 0, i);
                }
            }
        }
        return vertices;
    }

    public Color[] ColorizeVertices(Color color)
    {
        Color[] colors = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;

                if ((j - diffFromTex_w) < texture.width && j > diffFromTex_w &&
                    (i - diffFromTex_h) < texture.height && i > diffFromTex_h)
                {
                    float currHeight = texture.GetPixel(j - diffFromTex_w, i - diffFromTex_h).r;
                    colors[currIndex] = color * currHeight;
                }
                else
                {
                    colors[currIndex] = new Color(0, 0, 0, 0);
                }
            }
        }
        return colors;
    }

    public Color[] ColorizeVertices(Gradient gradient)
    {
        Color[] colors = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;

                if ((j - diffFromTex_w) < texture.width && j > diffFromTex_w &&
                    (i - diffFromTex_h) < texture.height && i > diffFromTex_h)
                {
                    float currHeight = texture.GetPixel(j - diffFromTex_w, i - diffFromTex_h).r;
                    colors[currIndex] = gradient.Evaluate(currHeight);
                }
                else
                {
                    colors[currIndex] = new Color(0, 0, 0, 0);
                }
            }
        }
        return colors;
    }

    #endregion
}
