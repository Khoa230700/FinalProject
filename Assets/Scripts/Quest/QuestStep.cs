using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{
    private bool isCompleted = false;

    protected void CompletedQuestStep()
    {
        if (!isCompleted)
        {
            isCompleted = true;

            Destroy(this.gameObject);
        }
    }
}
