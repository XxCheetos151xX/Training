using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FollowShapeSO", menuName = "Game Type/ Follow Shape", order = 11)]
public class FollowShapeSO : AbsGameSO
{
    public List<FollowShapeVariables> followshapelevels = new List<FollowShapeVariables>();
}

[Serializable]
public class FollowShapeVariables
{
    public float starttime;
    public float shapetimer;
}