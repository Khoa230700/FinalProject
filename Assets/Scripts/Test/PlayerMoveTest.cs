using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMoveTest : MonoBehaviour
{
    public bool isMoving = false;

    void Update()
    {
        isMoving = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).sqrMagnitude > 0.1f;
    }
}
