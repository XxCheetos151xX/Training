using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "DecisionSO", menuName = "Game Type/Decision", order = 4)]
public class DecisionSO : AbsGameSO
{
    public List<DecisionVariables> decisionlevels = new List<DecisionVariables>();
}


[Serializable]
public class DecisionVariables
{
    public float starttime;
    public float targetlifespan;
    public float color_changetimer;
    public bool isflickering;
    public float flickerspeed;
}
