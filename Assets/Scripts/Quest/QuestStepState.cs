using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestStepState
{
    public string state;

    public QuestStepState(string state)
    {
        this.state = state;
    }

    public QuestStepState()
    {
        this.state = "";
    }
}
