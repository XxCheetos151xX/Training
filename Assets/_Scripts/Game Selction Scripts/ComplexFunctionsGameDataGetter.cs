using UnityEngine;
using System.Collections.Generic;

public class ComplexFunctionsGameDataGetter : GameDataGetter
{
    public ComplexFunctionsSO advancedEvalSO;
    public ComplexFunctionsSO quickEvalSO;
    public List<ComplexFunctionsSO> traininglevelSO = new List<ComplexFunctionsSO>();

    public ComplexFunctionManager manager;

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
        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveComplexFunctionsSO(val as ComplexFunctionsSO);
    }
}
