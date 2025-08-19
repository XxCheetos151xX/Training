using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataSetter : MonoBehaviour
{

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
    Depth,
}
[Serializable]
public enum GameMode
{
    None,
    Training,
    QuickEval,
    AdvancedEval,
}

