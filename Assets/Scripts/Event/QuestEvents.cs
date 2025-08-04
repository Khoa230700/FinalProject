using System;

public class QuestEvents
{
    public event Action<string> OnStartQuest;
    public void StartQuest(string questId)
    {
        OnStartQuest?.Invoke(questId);
    }

    public event Action<string> OnAdvanceQuest;
    public void AdvanceQuest(string questId)
    {
        OnAdvanceQuest?.Invoke(questId);
    }

    public event Action<string> OnCompleteQuest;
    public void CompleteQuest(string questId)
    {
        OnCompleteQuest?.Invoke(questId);
    }

    public event Action<Quest> OnQuestStateChange;
    public void QuestStateChange(Quest quest)
    {
        OnQuestStateChange?.Invoke(quest);
    }

    public event Action<string, int, QuestStepState> OnQuestStepStateChange;
    public void QuestStepStateChanged(string questId, int stepIndex, QuestStepState questStepState)
    {
        OnQuestStepStateChange?.Invoke(questId, stepIndex, questStepState);
    }
}
