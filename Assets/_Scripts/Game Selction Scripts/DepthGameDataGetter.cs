using UnityEngine;
using System.Collections.Generic;

public class DepthGameDataGetter : GameDataGetter
{
    public DepthSO advancedevalSO;
    public DepthSO quickevalSO;
    public List<DepthSO> traininglevels = new List<DepthSO>();

    public DepthManager manager;

    protected override void SetGame()
    {
        switch (_gameMode) 
        {
            case GameMode.AdvancedEval:
                SetGameSettings(advancedevalSO);
                break;
            case GameMode.QuickEval:
                SetGameSettings(quickevalSO);
                break;
            case GameMode.Training:
                int level = PlayerPrefs.GetInt("LevelSelected");
                SetGameSettings(traininglevels[level - 1]);
                break;
        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveDepthSO(val as DepthSO);
    }
}
