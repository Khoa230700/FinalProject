using UnityEngine;

public class MultiAnimatorController : MonoBehaviour
{
    [Header("Target Animators")]
    [Tooltip("Kéo tất cả Animator bạn muốn điều khiển vào đây (body, arms, weapon, UI animators...).")]
    public Animator[] animators;

    [Header("Options")]
    [Tooltip("Kiểm tra tham số tồn tại trước khi set (tránh spam lỗi).")]
    public bool checkParameterExistence = true;

    // -------- Public API --------
    public void SetTriggerAll(string param)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            if (!checkParameterExistence || HasParam(a, param, AnimatorControllerParameterType.Trigger))
                a.SetTrigger(param);
        }
    }

    public void ResetTriggerAll(string param)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            if (!checkParameterExistence || HasParam(a, param, AnimatorControllerParameterType.Trigger))
                a.ResetTrigger(param);
        }
    }

    public void SetBoolAll(string param, bool value)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            if (!checkParameterExistence || HasParam(a, param, AnimatorControllerParameterType.Bool))
                a.SetBool(param, value);
        }
    }

    public void SetFloatAll(string param, float value)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            if (!checkParameterExistence || HasParam(a, param, AnimatorControllerParameterType.Float))
                a.SetFloat(param, value);
        }
    }

    public void SetIntAll(string param, int value)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            if (!checkParameterExistence || HasParam(a, param, AnimatorControllerParameterType.Int))
                a.SetInteger(param, value);
        }
    }

    public void PlayAll(string stateName, int layer = 0, float normalizedTime = 0f)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            a.Play(stateName, layer, normalizedTime);
        }
    }

    public void CrossFadeAll(string stateName, float transitionDuration, int layer = 0, float normalizedTimeOffset = 0f)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            a.CrossFadeInFixedTime(stateName, transitionDuration, layer, normalizedTimeOffset);
        }
    }

    public void SetSpeedAll(float speed)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            a.speed = speed;
        }
    }

    public void SetLayerWeightAll(int layerIndex, float weight)
    {
        foreach (var a in animators)
        {
            if (!IsValid(a)) continue;
            if (layerIndex >= 0 && layerIndex < a.layerCount)
                a.SetLayerWeight(layerIndex, weight);
        }
    }

    // -------- Helpers --------
    private static bool IsValid(Animator a) => a != null && a.isActiveAndEnabled;

    private static bool HasParam(Animator a, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in a.parameters)
        {
            if (p.type == type && p.name == name)
                return true;
        }
        return false;
    }
}
