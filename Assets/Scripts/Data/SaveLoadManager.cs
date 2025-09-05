using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Options")]
    public EncryptionType encryption = EncryptionType.AES;
    [Tooltip("Giây giữa các lần autosave. <= 0 để tắt.")]
    public float autoSaveInterval = 60f;
    public bool autoLoadOnStart = true;
    public bool saveOnQuit = true;
    public bool saveOnPause = true;
    public bool findRefsAutomatically = true;
    public bool verbose = true;

    [Header("Scene refs (optional nếu Auto)")]
    [SerializeField] private List<PlayerShoot> guns = new();
    [SerializeField] private MeleeWeapon melee;

    bool isDirty;
    Coroutine autosaveCo;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (findRefsAutomatically) DiscoverRefs();
        Subscribe();
    }

    void Start()
    {
        if (autoLoadOnStart) LoadAndApply();
        if (autoSaveInterval > 0) autosaveCo = StartCoroutine(AutoSaveLoop());
    }

    void OnDestroy() => Unsubscribe();

    void OnApplicationPause(bool pause)
    {
        if (saveOnPause && pause) SaveNow();
    }

    void OnApplicationQuit()
    {
        if (saveOnQuit) SaveNow();
    }

    // Cho phép đăng ký động (nếu bạn spawn vũ khí runtime)
    public void RegisterGun(PlayerShoot gun) { if (gun == null) return; if (!guns.Contains(gun)) guns.Add(gun); SubscribeOne(gun); }
    public void UnregisterGun(PlayerShoot gun) { if (gun == null) return; guns.Remove(gun); UnsubscribeOne(gun); }
    public void SetMelee(MeleeWeapon m) { melee = m; }

    public void MarkDirty() => isDirty = true;

    public void QueueAutosave(float delay = 2f)
    {
        if (!isActiveAndEnabled) { SaveNow(); return; }
        StartCoroutine(DelayedSave(delay));
    }

    IEnumerator DelayedSave(float delay)
    {
        yield return new WaitForSeconds(delay);
        SaveNow();
    }

    IEnumerator AutoSaveLoop()
    {
        var wait = new WaitForSeconds(autoSaveInterval);
        while (true)
        {
            yield return wait;
            if (isDirty) SaveNow();
        }
    }

    // PUBLIC API
    [ContextMenu("Save Now")]
    public void SaveNow()
    {
        BuildGameDataFromScene();
        SaveLoadData.Save(encryption);
        isDirty = false;
        if (verbose) Debug.Log("[SaveLoadManager] Saved.");
    }

    [ContextMenu("Load Now")]
    public void LoadAndApply()
    {
        var data = SaveLoadData.Load(encryption);
        ApplyToScene(data);
        isDirty = false;
        if (verbose) Debug.Log("[SaveLoadManager] Loaded & Applied.");
    }

    // CORE
    void BuildGameDataFromScene()
    {
        // SaveLoadData.Data luôn non-null nhờ static init
        var weapons = SaveLoadData.Data.weaponData ?? new WeaponData();
        weapons.guns.Clear();

        //gun
        foreach (var g in GetGunsSafe())
        {
            var up = g.GetComponent<GunUpgradeState>();

            weapons.guns.Add(new GunSave
            {
                gunId = GetGunId(g.gunData),
                level = up ? up.level : 0,
            });
        }

        //melee
        if (melee != null && melee.data != null)
        {
            weapons.melee = new MeleeSave
            {
                meleeId = GetMeleeId(melee.data),
                level = melee.level
            };
        }

        SaveLoadData.Data.weaponData = weapons;
        SaveLoadData.Data.coins = CoinManager.Instance?.GetCoins() ?? 0;

        // Save quests
        if (QuestManager.Instance != null)
        {
            var questData = new QuestData();

            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                QuestDataSO questSave = new QuestDataSO
                {
                    questID = quest.questSO.questID,
                    status = quest.status
                };

                foreach (var objective in quest.objectives)
                {
                    questSave.objectives.Add(new ObjectiveData
                    {
                        objectiveID = objective.objectiveID,
                        currentAmount = objective.currentAmount,
                        isCompleted = objective.isCompleted
                    });
                }
                questData.activeQuests.Add(questSave);
            }

            questData.completedQuestIDs = QuestManager.Instance.completedQuestIDs;
            SaveLoadData.Data.questData = questData;
        }

        // Player
        if (SelectorSpawner.Instance != null && SelectorSpawner.Instance.Player != null)
        {
            Transform t = SelectorSpawner.Instance.Player.transform;
            SaveLoadData.Data.playerData.posX = t.position.x;
            SaveLoadData.Data.playerData.posY = t.position.y;
            SaveLoadData.Data.playerData.posZ = t.position.z;
            SaveLoadData.Data.playerData.rotY = t.eulerAngles.y;
        }
    }

    void ApplyToScene(GameData data)
    {
        if (data == null || data.weaponData == null) return;

        var saved = data.weaponData;

        // Guns
        var map = new Dictionary<string, PlayerShoot>();
        foreach (var g in GetGunsSafe())
        {
            string id = GetGunId(g.gunData);
            if (!map.ContainsKey(id)) map.Add(id, g);
        }

        foreach (var gs in saved.guns)
        {
            if (map.TryGetValue(gs.gunId, out var gun))
            {
                var up = gun.GetComponent<GunUpgradeState>();
                if (up != null) up.SetLevel(gs.level);
            }
            else
            {
                if (verbose) Debug.LogWarning($"[SaveLoadManager] Không tìm thấy gun id '{gs.gunId}' trong scene.");
            }
        }

        // Melee
        if (melee != null && melee.data != null && saved.melee != null)
        {
            if (GetMeleeId(melee.data) == saved.melee.meleeId)
            {
                melee.level = saved.melee.level;
            }
        }

        //Coins
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.LoadCoins();
        }

        // Quests
        if (QuestManager.Instance != null && data.questData != null)
        {
            QuestManager.Instance.activeQuests.Clear();
            QuestManager.Instance.completedQuestIDs.Clear();

            QuestManager.Instance.completedQuestIDs.AddRange(data.questData.completedQuestIDs);

            foreach (var questSave in data.questData.activeQuests)
            {
                QuestSO questSO = QuestManager.Instance.allQuests.Find(q => q.questID == questSave.questID);
                if (questSO != null)
                {
                    Quest quest = new Quest(questSO);
                    quest.status = questSave.status;

                    foreach (var objSave in questSave.objectives)
                    {
                        var obj = quest.objectives.Find(o => o.objectiveID == objSave.objectiveID);
                        if (obj != null)
                        {
                            obj.currentAmount = objSave.currentAmount;
                            obj.isCompleted = objSave.isCompleted;
                        }
                    }

                    QuestManager.Instance.activeQuests.Add(quest);
                }
            }
        }

        // Player
        if (SelectorSpawner.Instance != null && SelectorSpawner.Instance.Player != null && data.playerData != null)
        {
            var t = SelectorSpawner.Instance.Player.transform;
            t.position = new Vector3(data.playerData.posX, data.playerData.posY, data.playerData.posZ);
            t.rotation = Quaternion.Euler(0, data.playerData.rotY, 0);
        }
    }

    // Helpers
    List<PlayerShoot> GetGunsSafe()
    {
        if (findRefsAutomatically && (guns == null || guns.Count == 0))
            DiscoverRefs();

        guns.RemoveAll(g => g == null);
        return guns;
    }

    void DiscoverRefs()
    {
        guns = FindObjectsByType<PlayerShoot>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.InstanceID
                ).ToList();
        if (melee == null) melee = FindAnyObjectByType<MeleeWeapon>(
                    FindObjectsInactive.Include
                );
    }

    void Subscribe()
    {
        foreach (var g in GetGunsSafe())
            SubscribeOne(g);
    }

    void SubscribeOne(PlayerShoot g)
    {
        var up = g.GetComponent<GunUpgradeState>();
        if (up != null)
        {
            up.OnLevelChanged.RemoveListener(OnGunLevelChanged);
            up.OnLevelChanged.AddListener(OnGunLevelChanged);
        }

        // Gợi ý: nếu PlayerShoot có event OnAmmoChanged/OnReloaded, hãy subscribe:
        // g.OnAmmoChanged += OnAmmoChanged;
    }

    void Unsubscribe()
    {
        foreach (var g in guns)
        {
            if (g != null)
                UnsubscribeOne(g);
        }
    }

    void UnsubscribeOne(PlayerShoot g)
    {
        var up = g.GetComponent<GunUpgradeState>();
        if (up != null) up.OnLevelChanged.RemoveListener(OnGunLevelChanged);

        // g.OnAmmoChanged -= OnAmmoChanged;
    }

    void OnGunLevelChanged(int _)
    {
        MarkDirty();
        QueueAutosave(1.5f);
    }

    // Nếu muốn gọi thủ công từ PlayerShoot khi bắn/nạp đạn:
    public void NotifyAmmoChanged()
    {
        MarkDirty();
        QueueAutosave(3f);
    }

    static string GetGunId(GunData data)
    {
        if (data == null) return "";
        return string.IsNullOrWhiteSpace(data.gunName) ? data.name : data.gunName;
    }

    static string GetMeleeId(MeleeData data)
    {
        if (data == null) return "";
        return string.IsNullOrWhiteSpace(data.weaponName) ? data.name : data.weaponName;
    }
}