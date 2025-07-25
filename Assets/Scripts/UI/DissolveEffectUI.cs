using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// [ExecuteInEditMode]
[DisallowMultipleComponent]
public class DissolveEffectUI : BaseMeshEffect
{
    [SerializeField][Range(0, 1)] float _location = 0.5f;
    [ColorUsage(false)] Color color = Color.black;

    public float location { get { return _location; } set { _location = Mathf.Clamp(value, 0, 1); _SetDirty(); } }

    public override void ModifyMesh(VertexHelper vh)
    {

        if (!IsActive())
            return;

        Rect rect = graphic.rectTransform.rect;

        UIVertex vertex = default(UIVertex);
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            var x = Mathf.Clamp01(vertex.position.x / rect.width + 0.5f);
            var y = Mathf.Clamp01(vertex.position.y / rect.height + 0.5f);
            vertex.uv1 = new Vector2(_PackToFloat(x, y, location, 0), _PackToFloat(color.r, color.g, color.b, 1));

            vh.SetUIVertex(vertex, i);
        }
    }

    void _SetDirty()
    {
        if (graphic)
            graphic.SetVerticesDirty();
    }

    static float _PackToFloat(float x, float y, float z, float w)
    {
        const int PRECISION = (1 << 6) - 1;
        return (Mathf.FloorToInt(w * PRECISION) << 18)
        + (Mathf.FloorToInt(z * PRECISION) << 12)
        + (Mathf.FloorToInt(y * PRECISION) << 6)
        + Mathf.FloorToInt(x * PRECISION);
    }
}

