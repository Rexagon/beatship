using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTerrain : MonoBehaviour
{
    public AudioClip audioClip;
    public Brick brickPrefab;

    public int size = 256;
    public float step = 1.0f;

    public float baseHeight = 5.0f;

    private Brick[,] _bricks;

    private float[] _spectrum = new float[256];
    private float[] _nextHeights;

    void Start()
    {
        _bricks = new Brick[size, size];
        _nextHeights = new float[size];

        Vector3 origin = new Vector3(-1.0f, -baseHeight, -1.0f) * (size - 1) * step * 0.5f;

        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                Vector3 position = origin + Vector3.forward * j * step;

                Brick brick = Instantiate(brickPrefab, position, Quaternion.identity);
                brick.transform.position = position;

                brick.ResetHeight(baseHeight);

                _bricks[i, j] = brick;
            }

            origin += Vector3.right * step;
        }
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
            if (j + 1 == size) { 
                for (int i = 0; i < size; ++i)
                {
                    _bricks[i, j].height = baseHeight + _nextHeights[i] * Mathf.PerlinNoise(0.1f + i, 0.1f + j) * 2.0f;
                }
            }
            else
            {
                for (int i = 0; i < size; ++i)
                {
                    _bricks[i, j].ResetHeight(_bricks[i, j + 1].height);
                    _bricks[i, j].offset = _nextHeights[i] * Mathf.PerlinNoise(0.1f + i, 0.1f + j) * 2.0f;
                }
            }
        }
    }
}
