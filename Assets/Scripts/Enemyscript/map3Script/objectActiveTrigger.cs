using System.Collections.Generic;
using UnityEngine;

public class objectActiveTrigger : MonoBehaviour
{
    public GameObject[] objectsToActivate;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }
}

