using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AudioTerrain : MonoBehaviour
{
    public AudioClip audioClip;
    public Brick brickPrefab;

    public int size = 256;
    public float step = 1.0f;

    public float baseHeight = 5.0f;

    private float[] _spectrum = new float[256];
    private float[] _nextHeights;
    private float[,] _heights;

    private Mesh _mesh;

    void Start()
    {
        _nextHeights = new float[size];
        _heights = new float[size, size];

        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        CreateShape();
    }

    void Update()
    {
        AudioListener.GetSpectrumData(_spectrum, 0, FFTWindow.Rectangular);

        float maxX = Mathf.Log(_spectrum.Length);
        for (int i = 0; i < _spectrum.Length; ++i)
        {
            int index = Mathf.FloorToInt(Mathf.Log(i + 1) / maxX * (_nextHeights.Length - 1) * 0.5f);
            _nextHeights[index] = _spectrum[i] * 10.0f;
            _nextHeights[size - index - 1] = _nextHeights[index];
        }

        for (int j = 0; j < size; ++j)
        {
            if (j + 1 == size)
            {
                for (int i = 0; i < size; ++i)
                {
                    _heights[i, j] = (_heights[i, j] + _nextHeights[i] * Mathf.PerlinNoise(0.1f + i, 0.1f + j) * 2.0f) * 0.5f;
                }
            }
            else
            {
                for (int i = 0; i < size; ++i)
                {
                    _heights[i, j] = _heights[i, j + 1];
                    //_heights[i, j].offset = _nextHeights[i] * Mathf.PerlinNoise(0.1f + i, 0.1f + j) * 2.0f;
                }
            }
        }

        Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];

        Vector3 origin = new Vector3(-1.0f, 0.0f, -1.0f) * (size + 1) * step * 0.5f;
        for (int z = 0; z <= size; ++z)
        {
            for (int x = 0; x <= size; ++x)
            {
                vertices[z * (size + 1) + x] = origin + Vector3.right * step * x;
                if (x < size && z < size)
                {
                    vertices[z * (size + 1) + x] += Vector3.up * (_heights[x, z] + _nextHeights[x] * Mathf.PerlinNoise(0.1f + x, 0.1f + z) * 2.0f) * 0.5f;
                }
            }

            origin += Vector3.forward * step;
        }

        _mesh.vertices = vertices;
        _mesh.RecalculateNormals();
        _mesh.UploadMeshData(false);
    }

    void CreateShape()
    {
        Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
        int[] indices = new int[size * size * 6];

        Vector3 origin = new Vector3(-1.0f, 0.0f, -1.0f) * (size + 1) * step * 0.5f;
        for (int z = 0; z <= size; ++z)
        {
            for (int x = 0; x <= size;  ++x)
            {
                vertices[z * (size + 1) + x] = origin + Vector3.right * step * x;
            }

            origin += Vector3.forward * step;
        }

        int vert = 0;
        int triangle = 0;
        for (int z = 0; z < size; ++z)
        {
            for (int x = 0; x < size; ++x)
            {
                indices[triangle + 0] = vert + 0;
                indices[triangle + 1] = vert + size + 1;
                indices[triangle + 2] = vert + 1;
                indices[triangle + 3] = vert + 1;
                indices[triangle + 4] = vert + size + 1;
                indices[triangle + 5] = vert + size + 2;

                ++vert;
                triangle += 6;
            }
            vert++;
        }

        _mesh.vertices = vertices;
        _mesh.triangles = indices;

        _mesh.UploadMeshData(false);
    }
}
