using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class GameDataSetter : MonoBehaviour
{
    [SerializeField] private UIManager ui_manager;


    public GameMode gameMode;
    public GameType gameType;


    public List<int> quickplay_game_types = new List<int>();
    public List<int> quickplay_game_levels = new List<int>();
    public List<int> antistress_game_types = new List<int>();
    public List<int> antistress_game_levels = new List<int>();
    public List<int> timeless_mode_levels = new List<int>();

    



    public void SetGameType(int Val)
    {
        gameType = (GameType)Val;
        if (Val <= ui_manager.games_description.Count - 1)
        {
            ui_manager.SetDescription(Val - 1);
        }
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

    public void QuickPlay()
    {
        // 1. Random game type from list
        if (quickplay_game_types.Count > 0)
        {
            int randomGameTypeIndex = UnityEngine.Random.Range(0, quickplay_game_types.Count);
            gameType = (GameType)quickplay_game_types[randomGameTypeIndex];
        }

        // 2. Force game mode to Training
        gameMode = GameMode.Training;

        // 3. Random level from list
        if (quickplay_game_levels.Count > 0)
        {
            int randomLevelIndex = UnityEngine.Random.Range(0, quickplay_game_levels.Count);
            int selectedLevel = quickplay_game_levels[randomLevelIndex];
            PlayerPrefs.SetInt("LevelSelected", selectedLevel);
        }

        // 4. Save game mode
        PlayerPrefs.SetString("GameMode", gameMode.ToString());

        // 5. Load scene based on GameType enum name
        SceneManager.LoadScene(gameType.ToString());
    }

    public void AntiStress()
    {
        // 1. Random game type from list
        if (quickplay_game_types.Count > 0)
        {
            int randomGameTypeIndex = UnityEngine.Random.Range(0, quickplay_game_types.Count);
            gameType = (GameType)quickplay_game_types[randomGameTypeIndex];
        }

        // 2. Force game mode to Training
        gameMode = GameMode.Training;

        // 3. Random level from list
        if (quickplay_game_levels.Count > 0)
        {
            int randomLevelIndex = UnityEngine.Random.Range(0, quickplay_game_levels.Count);
            int selectedLevel = quickplay_game_levels[randomLevelIndex];
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
    FollowShape,
}
[Serializable]
public enum GameMode
{
    None,
    Training,
    QuickEval,
    AdvancedEval,
    GeneralEval,
    Timeless
}

