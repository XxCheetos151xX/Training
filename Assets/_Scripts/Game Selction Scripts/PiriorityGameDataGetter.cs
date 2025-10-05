using UnityEngine;
using System.Collections.Generic;

public class PiriorityGameDataGetter : GameDataGetter
{
    public PirioritySO advancedevalSO;
    public PirioritySO quickevalSO;
    public PirioritySO generalEvalSO;
    public PirioritySO timelessSO;
    public List<PirioritySO> traininglevelSO = new List<PirioritySO>();

    public PiriorityManager manager;


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
                SetGameSettings(traininglevelSO[level - 1]);
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
        manager.SetActivePirioritySO(val as PirioritySO);
    }
}
