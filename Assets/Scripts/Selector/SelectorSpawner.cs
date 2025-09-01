using UnityEngine;

public class SelectorSpawner : MonoBehaviour
{
    public static SelectorSpawner Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject[] playerPfs;

    [Header("UI")]
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI shieldBar;
    [SerializeField] private WeaponUI weaponUI;

    public GameObject Player { get; private set; }
    public BarUI HealthBar => healthBar;
    public BarUI ShieldBar => shieldBar;
    public WeaponUI WeaponUI => weaponUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Spawn player
        int selectedIndex = PlayerPrefs.GetInt("CharacterHSelector", 0);
        if (selectedIndex < 0 || selectedIndex >= playerPfs.Length) selectedIndex = 0;

        Player = Instantiate(playerPfs[selectedIndex], transform.position, transform.rotation);
    }
}
