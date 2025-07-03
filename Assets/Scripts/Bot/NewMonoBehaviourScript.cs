using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       float xinput = Input.GetAxis("Horizontal");
        float yinput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(xinput, 0, yinput);
    }
}
