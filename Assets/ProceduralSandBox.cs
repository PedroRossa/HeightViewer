﻿using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralSandBox : MonoBehaviour
{
    public Texture2D heightMap;
    public Texture2D colorMap;
    public float maxHeight;
    public float cubeSize;
    [Range(0.25f, 5f)]
    public float animationSpeed;

    public Gradient gradient;
    public AnimationCurve animCurve;

    public TextMeshProUGUI txt;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    Vector3[] points;
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;
    Color[] colors;
    int[] triangles;

    private void CreatePoints(float size = 1f)
    {
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(cubeSize, 0, 0);
        Vector3 p2 = new Vector3(0, size + cubeSize, 0);
        Vector3 p3 = new Vector3(cubeSize, size + cubeSize, 0);
        Vector3 p4 = new Vector3(0, 0, cubeSize);
        Vector3 p5 = new Vector3(cubeSize, 0, cubeSize);
        Vector3 p6 = new Vector3(0, size + cubeSize, cubeSize);
        Vector3 p7 = new Vector3(cubeSize, size + cubeSize, cubeSize);

        points = new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7 };
    }

    private Vector3[] CreateVertices(Vector3 pos)
    {
        return new Vector3[]
        {	     
	        // Back
	        pos + points[5], pos + points[7], pos + points[4],
            pos + points[4], pos + points[7], pos + points[6],   
            // Front
	        pos + points[0], pos + points[2], pos + points[1],
            pos + points[1], pos + points[2], pos + points[3],
	        // Left
	        pos + points[4], pos + points[6], pos + points[0],
            pos + points[0], pos + points[6], pos + points[2],
	        // Right
	        pos + points[1], pos + points[3], pos + points[5],
            pos + points[5], pos + points[3], pos + points[7],
	        // Top
	        pos + points[2], pos + points[6], pos + points[3],
            pos + points[3], pos + points[6], pos + points[7]
        };
    }

    private Vector3[] CreateNormals()
    {
        return new Vector3[]
        { 
            // Back
	        Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward,
           	// Front
	        Vector3.back,Vector3.back,Vector3.back,Vector3.back,Vector3.back,Vector3.back,
	        // Left
	        Vector3.left,Vector3.left,Vector3.left,Vector3.left,Vector3.left,Vector3.left,
	        // Right
	        Vector3.right,Vector3.right,Vector3.right,Vector3.right,Vector3.right,Vector3.right,
	        // Top
            Vector3.up,Vector3.up,Vector3.up,Vector3.up,Vector3.up,Vector3.up
        };
    }

    private Vector2[] CreateUVs()
    {
        Vector2 _00 = new Vector2(0f, 0f);
        Vector2 _10 = new Vector2(1f, 0f);
        Vector2 _01 = new Vector2(0f, 1f);
        Vector2 _11 = new Vector2(1f, 1f);

        return new Vector2[]
        { 
	        // Back
	        _00, _01, _10, _10, _01, _11,
	        // Front
	        _00, _01, _10, _10, _01, _11,
	        // Left
	        _00, _01, _10, _10, _01, _11,
	        // Right
	        _00, _01, _10, _10, _01, _11,
	        // Top
	        _00, _01, _10, _10, _01, _11
        };
    }

    private Color[] CreateColors(Color color)
    {
        Color[] colors = new Color[30];

        for (int i = 0; i < 30; i++)
        {
            colors[i] = color;
        }
        return colors;
    }

    private int[] CreateTriangles(int lastIndex)
    {
        int[] triangles = new int[30];

        for (int j = 0; j < 30; j++)
        {
            triangles[j] = j + lastIndex;
        }
        return triangles;
    }

    private void CreateSandBox()
    {
        int w = heightMap.width;
        int h = heightMap.height;
        int cubeElements = w * h * 30;

        vertices = new Vector3[cubeElements];
        normals = new Vector3[cubeElements];
        uvs = new Vector2[cubeElements];
        colors = new Color[cubeElements];
        triangles = new int[cubeElements];

        Vector3 currentPos = Vector3.zero;
        int cubeElementIndex = 0;

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                currentPos = new Vector3(j * cubeSize, 0, i * cubeSize);

                //Set Size (width) of the cube
                CreatePoints(heightMap.GetPixel(j, i).r * maxHeight);

                CreateVertices(currentPos).CopyTo(vertices, cubeElementIndex);
                CreateNormals().CopyTo(normals, cubeElementIndex);

                if (colorMap == null)
                {
                    CreateColors(gradient.Evaluate(heightMap.GetPixel(j, i).r)).CopyTo(colors, cubeElementIndex);
                }
                else
                {
                    CreateColors(colorMap.GetPixel(j, i)).CopyTo(colors, cubeElementIndex);
                }

                CreateUVs().CopyTo(uvs, cubeElementIndex);
                CreateTriangles(cubeElementIndex).CopyTo(triangles, cubeElementIndex);

                cubeElementIndex += 30;
            }
        }

        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }

    IEnumerator AnimateMeshCoroutine(float duration)
    {
        float timer = 0;
        float percent = 0;

        meshRenderer.transform.localScale = new Vector3(1, 0.001f, 1);
        yield return new WaitForSeconds(1);
        while (timer < duration)
        {
            timer += Time.deltaTime;
            percent = animCurve.Evaluate(timer / duration);
            meshRenderer.transform.localScale = new Vector3
                (
                    1,
                    Mathf.Lerp(0, 1 * percent, percent),
                    1
                );
            yield return null;
        }

        meshRenderer.transform.localScale = Vector3.one;
    }

    private void AnimateVertices()
    {
        StartCoroutine(AnimateMeshCoroutine(animationSpeed));
    }

    [Button]
    public void CreateByTexture()
    {
        CreateSandBox();
        AnimateVertices();
    }

    [Button]
    public void UpdateColorByGradient()
    {
        int w = heightMap.width;
        int h = heightMap.height;
        Color[] colors = new Color[w * h * 30];

        int index = 0;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                for (int k = 0; k < 30; k++)
                {
                    if (colorMap == null)
                    {
                        colors[index] = gradient.Evaluate(heightMap.GetPixel(j, i).r);
                    }
                    else
                    {
                        colors[index] = colorMap.GetPixel(j, i);
                    }
                    index++;
                }
            }
        }

        mesh.colors = colors;
    }

    void Start()
    {
        if (heightMap == null)
        {
            Debug.LogError("You NEED a texture Folk!");
            return;
        }

        Init();
        CreateByTexture();
    }

    private void Init()
    {
        maxHeight = maxHeight <= 0 ? 1 : maxHeight;
        cubeSize = cubeSize <= 0 ? 1 : cubeSize;

        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Particles/Standard Surface"));
    }
    
    void Update()
    {
        //Mesh mesh = GetComponent<MeshFilter>().mesh;
        //Vector3[] vertices = mesh.vertices;
        //Vector3[] normals = mesh.normals;

        //for (var i = 0; i < vertices.Length; i++)
        //{
        //    vertices[i] += normals[i] * Mathf.Sin(Time.time);
        //}

        //mesh.vertices = vertices;
    }
}
