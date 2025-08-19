using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "PirioritySO", menuName = "Game Type/ Piriority", order = 6)]
public class PirioritySO : AbsGameSO
{
    public List<PiriorityVariables> pirioritylevels = new List<PiriorityVariables>();
}


[Serializable]

public class PiriorityVariables 
{
    public float starttime;
    public float targetlifetime;
    public float delaybetweentargets;
    public bool isflickering;
    public float flickeringspeed;
}