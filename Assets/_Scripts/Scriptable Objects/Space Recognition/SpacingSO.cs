using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpacingSO", menuName = "Game Type/Spacing", order = 3)]
public class SpacingSO : AbsGameSO
{
    public List<SpacingVariables> spacinglevels = new List<SpacingVariables>();
}

[Serializable]

public class SpacingVariables
{
    public float starttime;
    public float speed;
    public float scoreratio;
    public int rows;
    public int columns;
    public bool isflickering;
    public float flickeringspeed;
}
