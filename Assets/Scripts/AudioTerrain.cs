using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AudioTerrain : MonoBehaviour
{
    public AudioSource ship;

    public float maxAttenuation = 40.0f;
    public float maxDirectionAttenuation = 10.0f;
    public float spectorFactor = 2.0f;
    public float amplitude = 4.0f;
    public float noiseScale = 0.1f;

    public int size = 256;
    public float step = 1.0f;

    private float[] _spectrum = new float[256];
    private float[] _nextHeights;

    private Mesh _mesh;

    void Start()
    {
        _nextHeights = new float[size];

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
            _nextHeights[index] = _spectrum[i] * 10.0f / ship.volume;
            _nextHeights[size - index - 1] = _nextHeights[index];
        }

        float sqrMaxAttenuation = maxAttenuation * maxAttenuation;
        float sqrMaxDirectionAttenuation = maxDirectionAttenuation * maxDirectionAttenuation;

        Vector3 shipPosition = ship.transform.position;

        Vector3[] vertices = _mesh.vertices;

        Vector3 origin = new Vector3(-1.0f, 0.0f, -1.0f) * (size + 1) * step * 0.5f;
        for (int j = 0; j <= size; ++j)
        {
            for (int i = 0; i <= size; ++i)
            {
                float lastY = vertices[j * (size + 1) + i].y;

                Vector3 postion = origin + Vector3.right * step * i;

                Vector3 direction = shipPosition - postion - transform.position;

                float sqrDistance = direction.sqrMagnitude;
                float attenuation = Mathf.Max(sqrMaxAttenuation - sqrDistance, 0.0f) / sqrMaxAttenuation;

                float sqrDirectionDistance = Vector3.Project(direction, ship.transform.right).sqrMagnitude;
                float directionAttenuation = Mathf.Min(sqrDirectionDistance, sqrMaxDirectionAttenuation) / sqrMaxDirectionAttenuation + attenuation * attenuation;

                int x = Mathf.FloorToInt(attenuation * size * spectorFactor / 2.0f);

                if (x < size)
                {
                    float noise = Mathf.PerlinNoise(noiseScale * (i + _nextHeights[x] * 10.0f), noiseScale * (j + _nextHeights[x] * 10.0f));

                    postion.y += _nextHeights[x] * noise * amplitude * attenuation * directionAttenuation * 0.5f;
                    postion.y += lastY * 0.5f;
                }

                vertices[j * (size + 1) + i] = postion;
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
