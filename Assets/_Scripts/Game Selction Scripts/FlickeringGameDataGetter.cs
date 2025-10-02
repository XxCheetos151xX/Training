using UnityEngine;
using System.Collections.Generic;
public class FlickeringGameDataGetter : GameDataGetter
{
    public FlickeringSO advancedEvalSO;
    public FlickeringSO quickEvalSO;
    public FlickeringSO generalEvalSO;
    public List<FlickeringSO> traininglevelSO = new List<FlickeringSO>();

    public FlickeringGameManager manager;

    protected override void SetGame()
    {
        switch (_gameMode)
        {
            case GameMode.Training:
                int level = PlayerPrefs.GetInt("LevelSelected");
                SetGameSettings(traininglevelSO[level - 1]);
                break;
            case GameMode.AdvancedEval:
                SetGameSettings(advancedEvalSO);
                break;
            case GameMode.QuickEval:
                SetGameSettings(quickEvalSO);
                break;
            case GameMode.GeneralEval:
                SetGameSettings(generalEvalSO);
                break;
        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveFlickeringSO(val as FlickeringSO);
    }
}
