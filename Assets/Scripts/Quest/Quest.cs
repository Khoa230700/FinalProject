using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public QuestInfoSO info;
    public QuestState state;
    private int currentQuestStepIndex = 0;

    public Quest(QuestInfoSO questInfo)
    {
        this.info = questInfo;
        this.state = QuestState.NotStarted;
        this.currentQuestStepIndex = 0;
    }

    public void MoveToNextStep()
    {
        currentQuestStepIndex++;
    }

    public bool IsCurrentStepExists()
    {
        return currentQuestStepIndex < info.questStepsPrefabs.Length;
    }

    public void InstantiateCurrentStep(Transform parent)
    {
        if (IsCurrentStepExists())
        {
            GameObject stepPrefab = info.questStepsPrefabs[currentQuestStepIndex];
            GameObject stepInstance = Object.Instantiate(stepPrefab, parent);
            stepInstance.name = $"{info.questId}_Step_{currentQuestStepIndex}";
        }
    }
}
