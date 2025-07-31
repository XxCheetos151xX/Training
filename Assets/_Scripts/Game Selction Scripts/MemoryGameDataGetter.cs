using UnityEngine;
using System.Collections.Generic;


public class MemoryGameDataGetter : GameDataGetter
{
    public MemorySO advancedEvalSO;
    public MemorySO quickEvalSO;
    public List<MemorySO> traininglevelSO = new List<MemorySO>();

    public MemoryManager manager;

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
        }
    }

    public override void SetGameSettings(AbsGameSO val)
    {
        manager.SetActiveMemorySO(val as MemorySO);
    }
}
