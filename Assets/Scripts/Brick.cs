using UnityEngine;

public class Brick : MonoBehaviour
{
    public float height
    {
        set
        {
            _heights.z = _heights.y;
            _heights.y = _heights.x;
            _heights.x = (_heights.z + _heights.y + value) / 3.0f;

            UpdatePosition();
        }
        get
        {
            return _heights.x;
        }
    }

    public float offset
    {
        set
        {
            _offset.z = _heights.y;
            _offset.y = _heights.x;
            _offset.x = (_offset.z + _offset.y + value) / 3.0f;
            UpdatePosition();
        }
        get
        {
            return _offset.x;
        }
    }


    public Vector3 origin = Vector3.zero;

    private Vector3 _heights;
    private Vector3 _offset;

    public void ResetHeight(float height)
    {
        _heights = Vector3.one * height;
        UpdatePosition();
    }

    void UpdatePosition()
    {
        Vector3 position = transform.localPosition;
        position.y = origin.y + (_heights.x + _offset.x) * 0.5f;

        transform.localPosition = position;
        transform.localScale = new Vector3(1.0f, _heights.x, 1.0f);
    }
}
