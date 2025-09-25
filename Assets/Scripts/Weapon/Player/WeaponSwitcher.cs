using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class WeaponSwitcher : MonoBehaviour
{
    public WeaponUI weaponUI;

    [SerializeField] private List<GameObject> weaponList = new List<GameObject>();

    [Header("Audio")]
    [Tooltip("AudioSource trên Player/HUD để phát tiếng khi đổi súng.")]
    public AudioSource audioSource;
    [Tooltip("Âm thanh khi đổi sang vũ khí khác.")]
    public AudioClip switchSound;

    // LUÔN giữ kích thước == weaponList.Count để mapping theo index không bị lệch
    private readonly List<IWeapon> weapons = new List<IWeapon>();

    private int currentWeaponIndex = 0;
    private bool isSwitching = false;
    private bool didSwitch = false;

    public IWeapon Current
        => (weapons.Count > 0 &&
            currentWeaponIndex >= 0 &&
            currentWeaponIndex < weapons.Count)
           ? weapons[currentWeaponIndex]
           : null;

    // ---------------- LIFECYCLE ----------------
    void Awake()
    {
        BuildWeaponsFromList(); // chỉ build danh sách vũ khí, KHÔNG đụng đến UI ở đây
    }

    void Start()
    {
        // Tìm WeaponUI an toàn: ưu tiên SelectorSpawner.Instance, fallback FindObjectOfType
        if (weaponUI == null)
        {
            var selector = (SelectorSpawner.Instance != null) ? SelectorSpawner.Instance : null;
            weaponUI = selector != null ? selector.WeaponUI : FindObjectOfType<WeaponUI>(true);
            if (weaponUI == null)
                Debug.LogWarning("WeaponSwitcher: Không tìm thấy WeaponUI. OnSelected sẽ nhận null (nên code IWeapon xử lý null an toàn).");
        }

        if (weapons.Count == 0)
        {
            Debug.LogError("WeaponSwitcher: Không có vũ khí hợp lệ trong weaponList.");
            return;
        }

        // Giới hạn chỉ số và kích hoạt vũ khí đầu tiên
        currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, Mathf.Max(0, weaponList.Count - 1));
        ActivateWeapon(currentWeaponIndex);
    }

    void Update()
    {
        if (isSwitching || weapons.Count == 0) return;

        // Đổi vũ khí
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchKey(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchKey(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchKey(2);

        // Bắn / Đánh
        if (Input.GetMouseButtonDown(0)) Current?.FireOnce();
        if (Input.GetMouseButton(0)) Current?.StartFiring();
        if (Input.GetMouseButtonUp(0)) Current?.StopFiring();
    }

    private void SwitchKey(int index)
    {
        StartCoroutine(SwitchWeaponRoutine(index));

        if (!didSwitch && QuestManager.Instance.UpdateQuestProgress(QuestObjectiveType.Interact, "TutorialSwitch"))
        {
            didSwitch = true;
        }
    }

    // ---------------- BUILD ----------------
    private void BuildWeaponsFromList()
    {
        weapons.Clear();

        if (weaponList == null || weaponList.Count == 0)
        {
            Debug.LogError("WeaponSwitcher: weaponList rỗng hoặc null.");
            return;
        }

        for (int i = 0; i < weaponList.Count; i++)
        {
            var go = weaponList[i];

            if (go == null)
            {
                Debug.LogError($"WeaponSwitcher: weaponList[{i}] là null.");
                weapons.Add(null); // placeholder để không lệch index
                continue;
            }

            if (go == this.gameObject)
            {
                Debug.LogError("WeaponSwitcher: KHÔNG được thêm chính GameObject có WeaponSwitcher vào weaponList.");
                weapons.Add(null);
                continue;
            }

            // Lấy cả khi object đang inactive
            var w = go.GetComponentInChildren<IWeapon>(true);
            if (w == null)
            {
                Debug.LogError($"{go.name}: thiếu component IWeapon (trên chính nó hoặc con).");
                weapons.Add(null);
                continue;
            }

            weapons.Add(w);
        }
    }

    // ---------------- SWITCH ----------------
    private IEnumerator SwitchWeaponRoutine(int newIndex)
    {
        if (newIndex < 0 || newIndex >= weaponList.Count || newIndex == currentWeaponIndex)
            yield break;

        isSwitching = true;

        var prev = (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
                 ? weapons[currentWeaponIndex] : null;

        // Hủy reload nếu là vũ khí bắn đạn
        if (prev is IReloadable r) r.CancelReload();

        prev?.OnDeselected();
        if (prev != null) yield return prev.SwitchOut(this);

        ActivateWeapon(newIndex);
        currentWeaponIndex = newIndex;

        // Phát âm thanh đổi súng ngay khi vũ khí mới được active
        PlaySwitchSound();

        var cur = (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
                ? weapons[currentWeaponIndex] : null;

        if (cur != null) yield return cur.SwitchIn(this);
        else Debug.LogError($"WeaponSwitcher: weapons[{currentWeaponIndex}] null.");

        isSwitching = false;
    }

    private void PlaySwitchSound()
    {
        if (switchSound == null) return;

        if (audioSource != null)
            audioSource.PlayOneShot(switchSound, AudioManager.Instance.GetSFXVolume());
        else
            AudioSource.PlayClipAtPoint(switchSound, transform.position, AudioManager.Instance.GetSFXVolume());
    }

    // ---------------- ACTIVATE ----------------
    void ActivateWeapon(int index)
    {
        if (weaponList == null || weaponList.Count == 0)
        {
            Debug.LogError("WeaponSwitcher: weaponList rỗng.");
            return;
        }
        if (index < 0 || index >= weaponList.Count)
        {
            Debug.LogError($"WeaponSwitcher: index {index} ngoài phạm vi weaponList.");
            return;
        }

        for (int i = 0; i < weaponList.Count; i++)
        {
            var go = weaponList[i];
            if (go == null) continue;

            // Không bao giờ tắt chính object gắn WeaponSwitcher
            if (go == this.gameObject) continue;

            go.SetActive(i == index);
        }

        // Gọi OnSelected an toàn
        if (index < weapons.Count && weapons[index] != null)
        {
            weapons[index].OnSelected(weaponUI);
        }
        else
        {
            Debug.LogError($"WeaponSwitcher: weapons[{index}] null hoặc không khớp với weaponList.");
        }
    }
}
