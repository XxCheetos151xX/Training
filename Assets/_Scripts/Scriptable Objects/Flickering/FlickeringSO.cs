using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlickeringSO", menuName = "Game Type/ Flickering", order = 9)]
public class FlickeringSO : AbsGameSO
{
    public List<FlickeringVariables> flickeringlevels = new List<FlickeringVariables>();
}

[Serializable]
public class FlickeringVariables
{
    public float starttime;
    public float minspeed;
    public float maxspeed;
    public float delay;
    public float scoreratio;
    public bool isflickeing;
    public float flickeringspeed;
}
