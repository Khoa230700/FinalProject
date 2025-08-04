using UnityEngine;

public class CollectCoinsQuestStep : QuestStep
{
    [SerializeField] private int coinsToComplete = 5;
    private int coinsCollected = 0;

    //Test
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // Simulate coin collection for testing
        {
            Debug.Log("Collecting a coin...");
            CollectCoin();
        }
    }

    public void CollectCoin()
    {
        if (coinsCollected < coinsToComplete)
        {
            coinsCollected++;
            UpdateState();
        }

        if (coinsCollected >= coinsToComplete)
        {
            CompletedQuestStep();
        }
    }

    private void UpdateState()
    {
        string state = coinsCollected.ToString();
        ChangeState(state);
    }

    protected override void SetQuestStepState(string questStepState)
    {
        this.coinsCollected = int.Parse(questStepState);
        UpdateState();
    }

    public override string GetStepStateDescription()
    {
        return $"{coinsCollected} / {coinsToComplete} Coins";
    }
}
