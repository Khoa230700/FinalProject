using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Options")]
    public EncryptionType encryption = EncryptionType.AES;
    public float autoSaveInterval = 60f;
    public bool verbose = true;

    private List<ISaveLoad> saveableObjects = new();
    private bool isDirty;
    private float sessionPlayTime = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (autoSaveInterval > 0) StartCoroutine(AutoSaveLoop());
    }

    void Update() => sessionPlayTime += Time.unscaledDeltaTime;
    void OnApplicationQuit() => SaveNow();
    void OnApplicationPause(bool pause)
    {
        if (pause) SaveNow();
    }

    // API
    public void Register(ISaveLoad saveableObject)
    {
        if (!saveableObjects.Contains(saveableObject))
        {
            saveableObjects.Add(saveableObject);
            saveableObject.LoadFromData(SaveLoadData.Data);

            if (verbose) Debug.Log($"[SaveLoadManager] Registered: {saveableObject.GetType().Name}");
        }
    }

    public void Unregister(ISaveLoad saveableObject)
    {
        if (saveableObjects.Remove(saveableObject))
        {
            if (verbose) Debug.Log($"[SaveLoadManager] Unregistered: {saveableObject.GetType().Name}");
        }
    }

    public void MarkDirty() => isDirty = true;

    [ContextMenu("Save Now")]
    public void SaveNow()
    {
        SaveDataFromManagers();
        SaveLoadData.Save(encryption);
        isDirty = false;
        if (verbose) Debug.Log("[SaveLoadManager] Game Saved Successfully!");
    }

    [ContextMenu("Load Now")]
    public void LoadNow()
    {
        var data = SaveLoadData.Load(encryption);
        LoadDataToManagers(data);
        isDirty = false;
        if (verbose) Debug.Log("[SaveLoadManager] Game Loaded Successfully!");
    }

    // CORE
    private void SaveDataFromManagers()
    {
        var gameData = SaveLoadData.Data ?? new GameData();

        foreach (var saveableObject in saveableObjects)
        {
            try
            {
                saveableObject.SaveToData(gameData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] Error saving {saveableObject.GetType().Name}: {e.Message}");
            }
        }

        SaveLoadData.Data = gameData;
        SaveLoadData.Data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SaveLoadData.Data.totalPlayTime = FormatPlayTime(sessionPlayTime);

        sessionPlayTime = 0f;
    }

    private void LoadDataToManagers(GameData data)
    {
        foreach (var saveableObject in saveableObjects)
        {
            try
            {
                saveableObject.LoadFromData(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoadManager] Error loading {saveableObject.GetType().Name}: {e.Message}");
            }
        }
    }

    IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            if (isDirty) SaveNow();
        }
    }

    // HELPER
    private string FormatPlayTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{hours:D2}:{minutes:D2}:{secs:D2}";
    }
}
