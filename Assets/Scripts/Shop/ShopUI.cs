using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject canvasSetting;
    [SerializeField] private EquipItemUI[] itemSlots = new EquipItemUI[5]; // [primary, secondary, melee, health, shield]

    public bool isOpen { get; private set; }
    public bool canOpen { get; set; } = true;

    private Animator animator;
    private PressKeyEvent pressKeyEvent;
    private EquipDescriptionsUI descriptionsUI;
    private Coroutine currentRoutine;
    private WaveManager waveManager;

    // Cached player components
    private IWeapon[] weapons;
    private PlayerHealth playerHealth;
    private PlayerShield playerShield;
    private PlayerMovement playerMovement;
    private MeshMouseLook mouseLook;

    private void Start()
    {
        animator = GetComponent<Animator>();
        pressKeyEvent = canvasSetting?.GetComponent<PressKeyEvent>();
        descriptionsUI = FindAnyObjectByType<EquipDescriptionsUI>();
        waveManager = FindAnyObjectByType<WaveManager>();

        CachePlayerComponents();
    }

    private void OnEnable()
    {
        CoinManager.Instance.OnCoinChanged += OnCoinsChanged;
    }

    private void OnDisable()
    {
        CoinManager.Instance.OnCoinChanged -= OnCoinsChanged;
    }
    private void CachePlayerComponents()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        weapons = player.GetComponentsInChildren<IWeapon>(true);
        playerHealth = player.GetComponent<PlayerHealth>();
        playerShield = player.GetComponent<PlayerShield>();
        playerMovement = player.GetComponent<PlayerMovement>();
        mouseLook = player.GetComponent<MeshMouseLook>();
    }

    public void Show()
    {
        if (isOpen || !canOpen || !waveManager.isBetweenWaves) return;

        isOpen = true;

        EventSystem.current?.SetSelectedGameObject(itemSlots[0]?.gameObject);
        UpdateAllSlots();

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(AnimateShop("In", () =>
        {
            canvasSetting?.SetActive(false);
            if (pressKeyEvent) pressKeyEvent.enabled = false;
            playerMovement.enabled = false;
            mouseLook.Show();
        }));
    }

    public void Hide()
    {
        isOpen = false;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(AnimateShop("Out", () =>
        {
            canvasSetting?.SetActive(true);
            if (pressKeyEvent) pressKeyEvent.enabled = true;
            descriptionsUI?.HideDescription();
            playerMovement.enabled = true;
            mouseLook.Hide();
        }));
    }

    private void UpdateAllSlots()
    {
        // Update weapon slots
        foreach (var weapon in weapons)
        {
            if (weapon is PlayerShoot gun)
            {
                if (gun.currentAmmo == 0 && gun.reserveAmmo == 0) gun.Initialize();

                int slotIndex = gun.gunData.gunSlot == GunSlot.Primary ? 0 : 1;
                itemSlots[slotIndex]?.UpdateSlot(gun, "Gun", 0, gun.currentAmmo, gun.reserveAmmo);
            }
            else if (weapon is MeleeWeapon melee)
            {
                itemSlots[2]?.UpdateSlot(melee, "Melee", melee.level);
            }
        }

        // Update stat slots
        itemSlots[3]?.UpdateSlot(playerHealth, "Health");
        itemSlots[4]?.UpdateSlot(playerShield, "Shield");

        // Force refresh all slots to update button states
        RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in itemSlots)
        {
            slot?.RefreshUI();
        }
    }

    public void OnCoinsChanged(int oldCoin, int newCoin)
    {
        if (isOpen)
        {
            RefreshAllSlots();
        }
    }

    private IEnumerator AnimateShop(string animationName, System.Action onComplete = null)
    {
        animator.Play(animationName);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        onComplete?.Invoke();
        currentRoutine = null;
    }
}
