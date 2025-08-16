using System.Collections.Generic;

[System.Serializable]
public class QuestSaveData
{
    public List<QuestSave> activeQuests = new();
    public List<string> completedQuestIDs = new();
    public string currentScene;
    public string saveTime;
}

[System.Serializable]
public class QuestSave
{
    public string questID;
    public QuestStatus status;
    public List<ObjectiveSave> objectives = new();
}

[System.Serializable]
public class ObjectiveSave
{
    public string objectiveID;
    public int currentAmount;
    public bool isCompleted;
}