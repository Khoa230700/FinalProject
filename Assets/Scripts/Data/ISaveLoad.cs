using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveLoad
{
    void SaveToData(GameData data);
    void LoadFromData(GameData data);
}
