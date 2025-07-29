using System.Collections.Generic;
using UnityEngine;

public class FocusGameDataGetter : GameDataGetter
{
    public FocusSO advancedevalSO;
    public FocusSO quickevalSO;
    public List<FocusSO> trainingSOlevel;

    public FocusManager manager;


    protected override void SetGame()
    {
        switch (_gameMode)
        {
            case GameMode.Training:
                int level = PlayerPrefs.GetInt("LevelSelected");
                SetGameSettings(trainingSOlevel[level - 1]);
                break;

            case GameMode.QuickEval:
                SetGameSettings(quickevalSO);
                break;

            case GameMode.AdvancedEval:
                SetGameSettings(advancedevalSO);
                break;


        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveFocusSO(val as FocusSO);
    }
}
