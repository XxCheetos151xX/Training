using UnityEngine;
using System.Collections.Generic;

public class RecognitionSpeedDataGetter : GameDataGetter
{
    public RecognitionSpeedSO advancedEvalSO;
    public RecognitionSpeedSO quickEvalSO;
    public List<RecognitionSpeedSO> trainingSOlevel = new List<RecognitionSpeedSO>();

    public RecognitionSpeedManager manager;

    protected override void SetGame()
    {
        switch (_gameMode)
        {
            case GameMode.Training:
                int level = PlayerPrefs.GetInt("LevelSelected");
                SetGameSettings(trainingSOlevel[level - 1]);
                break;

            case GameMode.QuickEval:
                SetGameSettings(quickEvalSO);
                break;

            case GameMode.AdvancedEval:
                SetGameSettings(advancedEvalSO);
                break;

        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveRecognitionSpeedSO(val as RecognitionSpeedSO);
    }
}
