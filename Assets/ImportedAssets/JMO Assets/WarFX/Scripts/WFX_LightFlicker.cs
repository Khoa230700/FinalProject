using UnityEngine;
using System.Collections;

/**
 *	Rapidly sets a light on/off.
 *	
 *	(c) 2015, Jean Moreno
**/

[RequireComponent(typeof(Light))]
public class WFX_LightFlicker : MonoBehaviour
{
    public float flickerDuration = 0.05f;

    private Light myLight;

    void Awake()
    {
        myLight = GetComponent<Light>();
        myLight.enabled = false; // tắt đèn từ đầu
    }

    public void FlickerOnce()
    {
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        myLight.enabled = true;
        yield return new WaitForSeconds(flickerDuration);
        myLight.enabled = false;
      
    }

}
