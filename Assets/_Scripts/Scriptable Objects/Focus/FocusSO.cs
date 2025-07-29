using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FocusSO", menuName = "Game Type/Focus", order = 2)]
public class FocusSO : AbsGameSO
{
    public List<FocusVariables> focuslevels = new List<FocusVariables>();
}


[Serializable]
public class FocusVariables 
{
    public float starttime;
    public float lifespan;
    public bool isflickering;
    public float flickeringspeed;
}
