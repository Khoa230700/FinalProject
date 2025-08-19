using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    public string tag;
    public GameObject prefab;
    public int size;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    public bool Initialized { get; private set; }

    private void Awake()
    {
        // Singleton đơn giản (optional)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePools();   // <-- chuyển từ Start() sang đây
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            var q = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                q.Enqueue(obj);
            }
            poolDictionary[pool.tag] = q;
        }

        Initialized = true;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!Initialized || poolDictionary == null)
        {
            Debug.LogError("ObjectPooler: pools chưa được khởi tạo (poolDictionary null). Hãy kiểm tra thứ tự khởi tạo hoặc Awake().");
            return null;
        }

        if (!poolDictionary.TryGetValue(tag, out var queue))
        {
            Debug.LogError($"ObjectPooler: không có pool với tag '{tag}'. Các tag hiện có: {string.Join(", ", poolDictionary.Keys)}");
            return null;
        }

        var objectToSpawn = queue.Dequeue();
        if (objectToSpawn == null)
        {
            Debug.LogError($"ObjectPooler: phần tử trong pool '{tag}' bị null. Kiểm tra prefab và quá trình Instantiate.");
            return null;
        }

        objectToSpawn.transform.SetPositionAndRotation(position, rotation);
        objectToSpawn.SetActive(true);

        queue.Enqueue(objectToSpawn);
        return objectToSpawn;
    }
}
