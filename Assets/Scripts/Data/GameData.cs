using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public QuestData questData = new();
    public List<CharacterData> characterDatas = new List<CharacterData>();
    public int coins;
    public string saveTime;
    public string totalPlayTime;

    public CharacterData GetCharacterData(int characterIndex)
    {
        var charData = characterDatas.Find(c => c.characterIndex == characterIndex);
        if (charData == null)
        {
            charData = new CharacterData { characterIndex = characterIndex, weaponData = new WeaponData() };
            characterDatas.Add(charData);
        }
        return charData;
    }
}
