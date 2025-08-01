using UnityEngine;

public class CollectCoinsQuestStep : QuestStep
{
    private int coinsCollected = 0;
    private int coinsToComplete = 5;

    public void CollectCoin()
    {
        
        if(coinsCollected < coinsToComplete)
        {
            coinsCollected++;
        }

        if (coinsCollected >= coinsToComplete)
        {
            CompletedQuestStep();
        }
    } 
}
