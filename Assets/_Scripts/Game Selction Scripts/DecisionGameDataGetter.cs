using UnityEngine;
using System.Collections.Generic;
public class DecisionGameDataGetter : GameDataGetter
{
    public DecisionSO advancedEvalSO;
    public DecisionSO quickEvalSO;
    public List<DecisionSO> trainingSOlevel;

    public DecisionManager manager;

    protected override void SetGame()
    {
        switch (_gameMode)
        {
            case GameMode.Training:
                int level = PlayerPrefs.GetInt("LevelSelected");
                SetGameSettings(trainingSOlevel[level - 1]);
                break;
            case GameMode.AdvancedEval:
                SetGameSettings(advancedEvalSO);
                break;
            case GameMode.QuickEval:
                SetGameSettings(quickEvalSO);
                break;
        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveDecisionSO(val as DecisionSO);
    }
}
