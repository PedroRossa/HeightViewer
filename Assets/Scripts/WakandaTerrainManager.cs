using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WakandaTerrainManager : MonoBehaviour
{
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    private void GenerateMesh()
    {
        int w = 256;
        int h = 256;
        Vector3[] points = new Vector3[w * h];

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                points[j + i * h] = new Vector3(j, 0, i);
            }
        }
    }
}
