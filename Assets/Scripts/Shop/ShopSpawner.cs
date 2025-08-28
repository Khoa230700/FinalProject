using UnityEngine;

public class ShopSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shop;        
    [SerializeField] private Transform[] spawnPoints;

    public void ShowShop()
    {
        if (shop == null || spawnPoints.Length == 0) return;

        int randIndex = Random.Range(0, spawnPoints.Length);
        Vector3 pos = spawnPoints[randIndex].position;
        Quaternion rot = spawnPoints[randIndex].rotation;

        if (shop != null)
        {
            shop.transform.SetPositionAndRotation(pos, rot);
            shop.SetActive(true);
        }
    }

    public void HideShop()
    {
        if (shop != null)
            shop.SetActive(false);
    }
}
