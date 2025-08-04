using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public QuestInfoSO questInfo;
    public QuestState questState;
    public int currentQuestStepIndex = 0;

    private QuestStepState[] questStepStates;

    public Quest(QuestInfoSO questInfo)
    {
        this.questInfo = questInfo;
        this.questState = QuestState.RequirementsNotMet;
        this.currentQuestStepIndex = 0;
        this.questStepStates = new QuestStepState[questInfo.questStepsPrefabs.Length];
        for (int i = 0; i < questStepStates.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
    }

    public Quest(QuestInfoSO questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        this.questInfo = questInfo;
        this.questState = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;
    }

    public void MoveToNextStep()
    {
        currentQuestStepIndex++;
    }

    public bool IsCurrentStepExists()
    {
        return currentQuestStepIndex < questInfo.questStepsPrefabs.Length;
    }

    public void InstantiateCurrentQuestStep(Transform parent)
    {
        if (IsCurrentStepExists())
        {
            GameObject stepPrefab = questInfo.questStepsPrefabs[currentQuestStepIndex];
            GameObject stepInstance = Object.Instantiate(stepPrefab, parent);

            stepInstance.GetComponent<QuestStep>()
                .InitializeQuestStep(questInfo.questId, currentQuestStepIndex, questStepStates[currentQuestStepIndex].state);
            stepInstance.name = $"{questInfo.questId}_Step_{currentQuestStepIndex}";
        }
    }

    public void StoreQuestStepState(int stepIndex, QuestStepState questStepState)
    {
        if (stepIndex < questStepStates.Length)
        {
            questStepStates[stepIndex].state = questStepState.state;
            questStepStates[stepIndex].status = questStepState.status;
        }
    }

    public QuestData GetQuestData()
    {
        return new QuestData(questState, currentQuestStepIndex, questStepStates);
    }

    public string GetStatus()
    {
        string status = "";

        for (int i = 0; i < currentQuestStepIndex; i++)
        {
            status += $"Step {i + 1}: {questStepStates[i].status}\n";
        }

        return status;
    }
}
