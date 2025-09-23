using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoisyFocusSO", menuName = "Game Type/ Noisy Focus", order = 7)]
public class NoisyFocusSO : AbsGameSO
{
    public List<NoisyFocusVariables> noisyfocuslevels = new List<NoisyFocusVariables>();
}

[Serializable]
public class NoisyFocusVariables
{
    public float starttime;
    public float delay;
    public float minSpeed;
    public float maxSpeed;
    public float scoreratio;
    public bool isflickering;
    public float flickeringspeed;
}