using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrosshairType { Static, Dynamic }

[Serializable]
public class CrosshairData
{
    public Color normalColor = Color.white;
    public Color onEntityColor = Color.red;
    public CrosshairType type = CrosshairType.Dynamic;
    public StaticCrosshairSettings staticCrosshair;
    public DynamicCrosshairSettings dynamicCrosshair;
}

[Serializable]
public class StaticCrosshairSettings
{
    public Sprite sprite;
    public Vector2 size = new Vector2(64f, 64f);
}

[Serializable]
public class DynamicCrosshairSettings
{
    [Range(0f, 3f)] public float idleScale = 1f;
    [Range(0f, 3f)] public float crouchScale = 0.65f;
    [Range(0f, 3f)] public float moveScale = 1.2f;
    [Range(0f, 3f)] public float runScale = 1.5f;
    [Range(0f, 3f)] public float jumpScale = 1.8f;
    [Range(0f, 10f)] public float punchSize = 2f;
    [Range(0f, 20f)] public float moveSpeed = 3f;
}
