using TMPro;
using UnityEngine;
using DG.Tweening;

public class SelectorCharacter : MonoBehaviour
{
    [Header("References")]
    public TMP_Text nameTMP;
    public Transform[] cameraPoints;
    public GameObject[] previewCharacters;

    [Header("Animation")]
    public float animDuration = 0.5f;

    private int selectedIndex = -1;
    private int lastIndex = -1;
    private Transform cam;

    private void Awake()
    {
        cam = Camera.main.transform;

        foreach (var c in previewCharacters)
            c.SetActive(false);
    }

    public void SetCharacter(int index)
    {
        if (index < 0 || index >= cameraPoints.Length) return;

        if (lastIndex != -1 && lastIndex < previewCharacters.Length)
            previewCharacters[lastIndex].SetActive(false);

        if (index < previewCharacters.Length)
            previewCharacters[index].SetActive(true);

        if (lastIndex == -1)
        {
            selectedIndex = index;

            var targetPoint = cameraPoints[selectedIndex];
            cam.position = targetPoint.position;
            cam.rotation = targetPoint.rotation;

            if (nameTMP != null && selectedIndex < previewCharacters.Length)
                nameTMP.text = previewCharacters[selectedIndex].name;

            lastIndex = selectedIndex;
            return;
        }

        lastIndex = selectedIndex;
        selectedIndex = index;
        AudioManager.Instance.PlaySFX("Ready");
        Debug.Log("Selected character: " + previewCharacters[selectedIndex].name);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (selectedIndex == -1) return;

        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;

        var targetPoint = cameraPoints[selectedIndex];

        DOTween.Kill(cam);

        cam.DOMove(targetPoint.position, animDuration)
            .SetEase(Ease.InOutSine)
            .SetTarget(cam);

        cam.DORotateQuaternion(targetPoint.rotation, animDuration)
            .SetEase(Ease.InOutSine)
            .SetTarget(cam);

        if (nameTMP != null && selectedIndex < previewCharacters.Length)
            nameTMP.text = previewCharacters[selectedIndex].name;
    }
}
