[System.Serializable]
public class GameData
{
    public QuestData questData = new();
    public WeaponData weaponData = new();
    // public PlayerData playerData = new();
    public int coins;
    public string saveTime;
}
