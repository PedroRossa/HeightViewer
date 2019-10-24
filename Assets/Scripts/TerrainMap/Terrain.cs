
using UnityEngine;

public class Terrain
{
    #region Attributes

    private int width;
    private int height;
    private int maxHeight;
    private Texture2D texture;
    private Vector2 differenceFromTexture;

    #endregion

    #region Constructors

    public Terrain(int width, int height, int maxHeight)
    {
        this.width = width;
        this.height = height;
        this.maxHeight = maxHeight;
        differenceFromTexture = new Vector2();
    }

    #endregion

    #region Private Methods

    private void CalculateDiffFromTerrainToTexture()
    {
        differenceFromTexture.x = (width - texture.width) / 2;
        differenceFromTexture.y = (height - texture.height) / 2;
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

    public Vector2 GetDifferenceFromTexture()
    {
        return differenceFromTexture;
    }

    public Vector3[] CalculateVertices(bool centralized)
    {
        Vector3[] vertices = new Vector3[width * height];
        int diffX = (int)differenceFromTexture.x;
        int diffY = (int)differenceFromTexture.y;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;
                //centralize on the middle of the terrain
                if (centralized)
                {
                    if ((j - diffX) < texture.width && j > diffX &&
                     (i - diffY) < texture.height && i > diffY)
                    {
                        float currHeight = texture.GetPixel(j - diffX, i - diffY).r;
                        vertices[currIndex] = new Vector3(j, currHeight * maxHeight, i);
                    }
                    else
                    {
                        vertices[currIndex] = new Vector3(j, 0, i);
                    }
                }
                else //Load model on bottom of terrain
                {
                    if (j < texture.width && i < texture.height)
                    {
                        float currHeight = texture.GetPixel(j, i).r;
                        vertices[currIndex] = new Vector3(j, currHeight * maxHeight, i);
                    }
                    else
                    {
                        vertices[currIndex] = new Vector3(j, 0, i);
                    }
                }
            }
        }
        return vertices;
    }
    
    public Color[] ColorizeVertices(Color color, bool centralized)
    {
        Color[] colors = new Color[width * height];
        int diffX = (int)differenceFromTexture.x;
        int diffY = (int)differenceFromTexture.y;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;
                //centralize on the middle of the terrain
                if (centralized)
                {
                    if ((j - diffX) < texture.width && j > diffX &&
                    (i - diffY) < texture.height && i > diffY)
                    {
                        float currHeight = texture.GetPixel(j - diffX, i - diffY).r;
                        colors[currIndex] = color * currHeight;
                    }
                    else
                    {
                        colors[currIndex] = new Color(0, 0, 0, 0);
                    }
                }
                else //Load model on bottom of terrain
                {
                    if (j < texture.width && i < texture.height)
                    {
                        float currHeight = texture.GetPixel(j, i).r;
                        colors[currIndex] = color * currHeight;
                    }
                    else
                    {
                        colors[currIndex] = new Color(0, 0, 0, 0);
                    }
                }
            }
        }
        return colors;
    }

    public Color[] ColorizeVertices(Gradient gradient, bool centralized)
    {
        Color[] colors = new Color[width * height];
        int diffX = (int)differenceFromTexture.x;
        int diffY = (int)differenceFromTexture.y;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int currIndex = j + i * width;
                //centralize on the middle of the terrain
                if (centralized)
                {
                    if ((j - diffX) < texture.width && j > diffX &&
                    (i - diffY) < texture.height && i > diffY)
                    {
                        float currHeight = texture.GetPixel(j - diffX, i - diffY).r;
                        colors[currIndex] = gradient.Evaluate(currHeight);
                    }
                    else
                    {
                        colors[currIndex] = new Color(0, 0, 0, 0);
                    }
                }
                else //Load model on bottom of terrain
                {
                    if (j < texture.width && i < texture.height)
                    {
                        float currHeight = texture.GetPixel(j, i).r;
                        colors[currIndex] = gradient.Evaluate(currHeight);
                    }
                    else
                    {
                        colors[currIndex] = new Color(0, 0, 0, 0);
                    }
                }
            }
        }
        return colors;
    }
        
    #endregion
}
