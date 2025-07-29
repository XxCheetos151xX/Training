using System;
using UnityEngine;

public abstract class GameDataGetter : MonoBehaviour
{
    protected GameMode _gameMode;

    private void Awake()
    {
        string saved = PlayerPrefs.GetString("GameMode", "None");
        Enum.TryParse(saved, out _gameMode); 
        SetGame();
    }

    protected virtual void SetGame()
    {
        switch (_gameMode)
        {
            case GameMode.Training:
                
                break;

            case GameMode.QuickEval:
                
                break;

            case GameMode.AdvancedEval:

                break;

        }
    }


    public virtual void SetGameSettings(AbsGameSO val)
    {
    }
}
