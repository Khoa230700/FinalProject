using System;
using System.Collections.Generic;

[Serializable]
public class QuestData
{
    public List<QuestDataSO> activeQuests = new();
    public List<string> completedQuestIDs = new();
}

[Serializable]
public class QuestDataSO
{
    public string questID;
    public QuestStatus status;
    public List<ObjectiveData> objectives = new();
}

[Serializable]
public class ObjectiveData
{
    public string objectiveID;
    public int currentAmount;
    public bool isCompleted;
}