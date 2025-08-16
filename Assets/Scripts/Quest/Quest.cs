using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestStatus
{
    NotStarted,
    Active,
    Completed
}

[System.Serializable]
public class Quest
{
    public QuestSO questSO;
    public QuestStatus status;
    public List<QuestObjective> objectives;

    public Quest(QuestSO questSO)
    {
        this.questSO = questSO;
        status = QuestStatus.NotStarted;

        //Copy objectives từ ScriptableObject
        objectives = new();
        for (int i = 0; i < questSO.objectives.Count; i++)
        {
            var obj = questSO.objectives[i];
            objectives.Add(new QuestObjective
            {
                // objectiveID = questData.objectives[i].objectiveID,
                objectiveID = questSO.questID + "_" + i,
                description = obj.description,
                type = obj.type,
                targetID = obj.targetID,
                currentAmount = 0,
                requiredAmount = obj.requiredAmount,
                isCompleted = false
            });
        }
    }

    public bool IsCompleted() => objectives.TrueForAll(o => o.isCompleted);
    public void UpdateObjective(string objectiveID, int amount = 1)
    {
        var objective = objectives.Find(obj => obj.objectiveID == objectiveID);
        if (objective != null && !objective.isCompleted)
        {
            objective.currentAmount += amount;
            if (objective.currentAmount >= objective.requiredAmount)
            {
                objective.isCompleted = true;
                objective.currentAmount = objective.requiredAmount;
            }
        }
    }
}