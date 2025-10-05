using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataSetter : MonoBehaviour
{
    public List<int> quickplay_game_types = new List<int>();
    public List<int> quickplay_game_levels = new List<int>();
    public List<int> antistress_game_types = new List<int>();
    public List<int> antistress_game_levels = new List<int>();
    public List<int> timeless_mode_levels = new List<int>();
    public GameType gameType;
    public GameMode gameMode;

    public void IsTimeless(int val)
    {
        PlayerPrefs.SetInt("IsTimeless", val);
    }

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

    public void ChooseRandomLevel()
    {
        int lvlindex = UnityEngine.Random.Range(0, timeless_mode_levels.Count - 1);
        int lvl = timeless_mode_levels[lvlindex];
        PlayerPrefs.SetInt("LevelSelected", lvl);

        gameMode = GameMode.Training;

        print(lvl);
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
    GeneralEval
}

