using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    TurnedIn
}

[System.Serializable]
public class Quest
{
    public QuestData questData;
    public QuestStatus status;
    public List<QuestObjective> objectives;

    public Quest(QuestData data)
    {
        questData = data;
        status = QuestStatus.NotStarted;

        //Copy objectives từ ScriptableObject
        objectives = new();
        foreach (var obj in data.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                targetID = obj.targetID,
                currentAmount = 0,
                requiredAmount = obj.requiredAmount,
                // isCompleted = false
            });
        }
    }

    public bool IsCompleted() => objectives.TrueForAll(o => o.isCompleted);
    // public void UpdateObjective(string objectiveID, int amount = 1)
    // {
    //     var objective = objectives.Find(obj => obj.objectiveID == objectiveID);
    //     if (objective != null && !objective.isCompleted)
    //     {
    //         objective.currentAmount += amount;
    //         if (objective.currentAmount >= objective.requiredAmount)
    //         {
    //             objective.isCompleted = true;
    //             objective.currentAmount = objective.requiredAmount;
    //         }
    //     }
    // }
}