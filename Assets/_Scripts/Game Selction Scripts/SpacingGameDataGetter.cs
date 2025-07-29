using UnityEngine;
using System.Collections.Generic;

public class SpacingGameDataGetter : GameDataGetter
{
    public SpacingSO advancedevalSO;
    public SpacingSO quickevalSO;
    public List<SpacingSO> traininglevelSO = new List<SpacingSO>();

    public SpacingManager manager;


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
        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveSpacingSO(val as SpacingSO);
    }
}
