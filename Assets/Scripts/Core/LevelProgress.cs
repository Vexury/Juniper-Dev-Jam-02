using UnityEngine;

public static class LevelProgress
{
    private const string Key = "LevelProgress_Highest";

    public static int RequestedLevel { get; set; }

    public static int HighestUnlocked
    {
        get => PlayerPrefs.GetInt(Key, 0);
        private set
        {
            PlayerPrefs.SetInt(Key, value);
            PlayerPrefs.Save();
        }
    }

    public static void Unlock(int index)
    {
        if (index > HighestUnlocked)
            HighestUnlocked = index;
    }

    private const string AllCompleteKey = "AllLevelsCompleted";

    public static bool AllLevelsCompleted => PlayerPrefs.GetInt(AllCompleteKey, 0) == 1;

    public static void MarkAllLevelsCompleted()
    {
        PlayerPrefs.SetInt(AllCompleteKey, 1);
        PlayerPrefs.Save();
    }

    public static int GetStarHighscore(int levelIndex) =>
        PlayerPrefs.GetInt($"StarHighscore_{levelIndex}", 0);

    public static void SaveStarHighscore(int levelIndex, int count)
    {
        if (count <= GetStarHighscore(levelIndex)) return;
        PlayerPrefs.SetInt($"StarHighscore_{levelIndex}", count);
        PlayerPrefs.Save();
    }
}
