using TMPro;
using UnityEngine;
using DG.Tweening;

public class SelectorCharacter : MonoBehaviour
{
    [Header("References")]
    public TMP_Text nameTMP;
    public Transform[] cameraPoints; // vị trí camera cho từng nhân vật
    public string[] characterNames;  // tên hiển thị cho từng nhân vật

    [Header("Animation")]
    public float animDuration = 0.5f;

    private int selectedIndex = -1;  // chưa chọn nhân vật nào
    private Transform cam;
    private Vector3 defaultPos;
    private Quaternion defaultRot;

    private void Start()
    {
        cam = Camera.main.transform;

        // lưu vị trí gốc của camera trong scene
        defaultPos = cam.position;
        defaultRot = cam.rotation;

        // Không auto chọn nhân vật ở Start
        if (nameTMP != null)
            nameTMP.text = "Chọn nhân vật...";
    }

    public void SetCharacter(int index)
    {
        if (index < 0 || index >= cameraPoints.Length) return;

        int lastIndex = selectedIndex;
        selectedIndex = index;

        if (lastIndex == selectedIndex) return;

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        Transform targetPoint;

        if (selectedIndex == -1)
        {
            // nếu chưa chọn thì giữ ở camera gốc
            targetPoint = null;
        }
        else
        {
            // chọn đúng nhân vật -> cameraPoint tương ứng
            targetPoint = cameraPoints[selectedIndex];

            // tween camera đến vị trí/rotation mới
            cam.DOMove(targetPoint.position, animDuration);
            cam.DORotateQuaternion(targetPoint.rotation, animDuration);

            if (nameTMP != null && selectedIndex < characterNames.Length)
                nameTMP.text = characterNames[selectedIndex];
        }
    }

    public void ResetToDefault()
    {
        // nếu muốn cho phép quay lại góc gốc
        cam.DOMove(defaultPos, animDuration);
        cam.DORotateQuaternion(defaultRot, animDuration);
        selectedIndex = -1;

        if (nameTMP != null)
            nameTMP.text = "Chọn nhân vật...";
    }
}
