using System.Collections.Generic;
using UnityEngine;

public class ChaseGameDataGetter : GameDataGetter
{
    public ChaseSO advancedEvalSO;
    public ChaseSO quickEvalSO;
    public ChaseSO generalEvalSO;
    public ChaseSO timelessSO;
    public List<ChaseSO> trainingSOLevel;

    public ChaseManager manager;
    protected override void SetGame()
    {
        switch (_gameMode)
        {
            case GameMode.Training:
                int level = PlayerPrefs.GetInt("LevelSelected");
                SetGameSettings(trainingSOLevel[level-1]);
                break;

            case GameMode.QuickEval:
                SetGameSettings(quickEvalSO);
                break;

            case GameMode.AdvancedEval:
                SetGameSettings(advancedEvalSO);
                break;
            case GameMode.GeneralEval:
                SetGameSettings(generalEvalSO);
                break;
            case GameMode.Timeless:
                SetGameSettings(timelessSO);
                break;
        }
    }


    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveChaseSO(val as ChaseSO);
    }

}
