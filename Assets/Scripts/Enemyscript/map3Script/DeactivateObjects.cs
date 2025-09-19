using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateObjects : MonoBehaviour
{
    public List<GameObject> objectsToDeactivate;
    public List<GameObject> objectsToActivate;
    public float delayInSeconds = 6f; 

    void Start()
    {
        StartCoroutine(ToggleAfterDelay());
    }

    IEnumerator ToggleAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds);

        // Deactivate objects
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Activate objects
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}
