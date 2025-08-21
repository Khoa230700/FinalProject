using UnityEngine;

public class ShopPoint : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private GameObject shopNotification;
    private bool playerInRange = false;

    private void Start()
    {
        if (shopUI == null)
        {
            shopUI = FindAnyObjectByType<ShopUI>();
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            shopUI.Show();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            shopNotification.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            shopNotification.SetActive(false);

            if (shopUI != null && shopUI.IsOpen)
            {
                shopUI.Hide();
            }
        }
    }
}
