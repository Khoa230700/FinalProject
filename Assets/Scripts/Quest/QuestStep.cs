using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestStep : MonoBehaviour
{
    private bool isCompleted = false;
    private string questId;
    private int stepIndex;

    public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
    {
        this.questId = questId;
        this.stepIndex = stepIndex;

        if (!string.IsNullOrEmpty(questStepState))
        {
            SetQuestStepState(questStepState);
        }
    }

    protected void CompletedQuestStep()
    {
        if (!isCompleted)
        {
            isCompleted = true;

            GameEventsManager.Instance.questEvents.AdvanceQuest(questId);

            Destroy(this.gameObject);
        }
    }

    protected void ChangeState(string newState)
    {
        GameEventsManager.Instance.questEvents.QuestStepStateChanged(questId, stepIndex, new QuestStepState(newState));
    }

    protected abstract void SetQuestStepState(string questStepState);
    public abstract string GetStepStateDescription();
}
