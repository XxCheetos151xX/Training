using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataSetter : MonoBehaviour
{
    [SerializeField] private List<int> game_types = new List<int>();
    [SerializeField] private List<int> game_levels = new List<int>();

    public GameType gameType;
    public GameMode gameMode;



    public void SetGameType(int Val)
    {
        gameType = (GameType)Val;
    }

    public void SetGameMode(int Val)
    {
        gameMode = (GameMode)Val;

    }

    public void SelectLevel(int Level)=>PlayerPrefs.SetInt("LevelSelected",Level); 
    public void LoadScene()
    {
        PlayerPrefs.SetString("GameMode", gameMode.ToString());
        SceneManager.LoadScene(gameType.ToString());
    }

    public void LoadRandomScene()
    {
        // 1. Random game type from list
        if (game_types.Count > 0)
        {
            int randomGameTypeIndex = UnityEngine.Random.Range(0, game_types.Count);
            gameType = (GameType)game_types[randomGameTypeIndex];
        }

        // 2. Force game mode to Training
        gameMode = GameMode.Training;

        // 3. Random level from list
        if (game_levels.Count > 0)
        {
            int randomLevelIndex = UnityEngine.Random.Range(0, game_levels.Count);
            int selectedLevel = game_levels[randomLevelIndex];
            PlayerPrefs.SetInt("LevelSelected", selectedLevel);
        }

        // 4. Save game mode
        PlayerPrefs.SetString("GameMode", gameMode.ToString());

        // 5. Load scene based on GameType enum name
        SceneManager.LoadScene(gameType.ToString());
    }

}

[Serializable]
public enum GameType
{
    None,
    Chase,
    Focus,
    Spacing,
    Decision,
    Memory,
    Piriority,
    NoisyFocus,
    RecognitionSpeed,
    Flickering,
    ComplexFunctions,
    EyeTraining,
}
[Serializable]
public enum GameMode
{
    None,
    Training,
    QuickEval,
    AdvancedEval,
}

