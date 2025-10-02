using UnityEngine;
using System.Collections.Generic;

public class NoisyFocusGameDataGetter : GameDataGetter
{
    public NoisyFocusSO advancedEvalSO;
    public NoisyFocusSO quickEvalSO;
    public NoisyFocusSO generalEvalSO;
    public List<NoisyFocusSO> traininglevelSO = new List<NoisyFocusSO>();

    public NoisyFocusManager manager;

    protected override void SetGame()
    {
        switch (_gameMode)
        {
            case GameMode.AdvancedEval:
                SetGameSettings(advancedEvalSO);
                break;
            case GameMode.QuickEval:
                SetGameSettings(quickEvalSO);
                break;
            case GameMode.Training:
                int level = PlayerPrefs.GetInt("LevelSelected");
                SetGameSettings(traininglevelSO[level - 1]);
                break;
            case GameMode.GeneralEval: 
                SetGameSettings(generalEvalSO);
                break;
        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveNoisyFocusSO(val as NoisyFocusSO);
    }
}
