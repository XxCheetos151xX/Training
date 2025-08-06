using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DepthSO", menuName = "Game Type/ Depth", order = 6)]
public class DepthSO : AbsGameSO
{
    public List<DepthVariables> depthlevels = new List<DepthVariables>();
}

[Serializable]
public class DepthVariables 
{
    public float starttime;
    public float lifetime;
    public float distance;
    public bool isflickering;
    public float flickeringspeed;
}

