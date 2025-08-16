using UnityEngine;

public enum QuestObjectiveType
{
    Kill,        //Giết quái vật
    Collect,     //Thu thập vật phẩm
    Interact,    //Tương tác với NPC/Object
    Reach        //Đến một vị trí
}

[System.Serializable]
public class QuestObjective
{
    [HideInInspector] public string objectiveID;
    public string description;
    public QuestObjectiveType type;
    public string targetID; //ID của target (Enemy, Item,...)
    public int currentAmount;
    public int requiredAmount;
    public bool isCompleted;
}