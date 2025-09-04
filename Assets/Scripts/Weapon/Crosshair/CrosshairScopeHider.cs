using UnityEngine;

public class CrosshairScopeHider : MonoBehaviour
{
    public CSGOScope scope;
    public CanvasGroup crosshairGroup;  // đặt CanvasGroup trên CrosshairRoot
    public bool hideWhenScoped = true;

    void Update()
    {
        if (!scope || !crosshairGroup) return;
        bool scoped = scope.IsScoped;
        float targetAlpha = (hideWhenScoped && scoped) ? 0f : 1f;
        crosshairGroup.alpha = targetAlpha;
        crosshairGroup.blocksRaycasts = targetAlpha > 0f;
        crosshairGroup.interactable = targetAlpha > 0f;
    }
}
