using System.Collections.Generic;

[System.Serializable]
public class QuestData
{
    public List<QuestDataSO> activeQuests = new();
    public List<string> completedQuestIDs = new();
    public string saveTime;
}

[System.Serializable]
public class QuestDataSO
{
    public string questID;
    public QuestStatus status;
    public List<ObjectiveData> objectives = new();
}

[System.Serializable]
public class ObjectiveData
{
    public string objectiveID;
    public int currentAmount;
    public bool isCompleted;
}