using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/QuestInfo", fileName = "NewQuestInfo", order = 1)]
public class QuestInfoSO : ScriptableObject
{
    [field: SerializeField] public string questId { get; private set; }
    
    [Header("General")]
    public string displayName;

    [Header("Requirements")]
    public int requiredLevel;
    public QuestInfoSO[] questPrerequisites;

    [Header("Steps")]
    public GameObject[] questStepsPrefabs;

    [Header("Rewards")]
    public int pointReward;
    public int experienceReward;

    private void OnValidate()
    {
#if UNITY_EDITOR
        questId = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
