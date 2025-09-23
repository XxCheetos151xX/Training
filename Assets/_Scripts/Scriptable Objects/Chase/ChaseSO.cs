using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ChaseSO", menuName = "Game Type/Chase", order = 1)]
public class ChaseSO : AbsGameSO
{
    public List<ChaseVariables> ChaseLevels = new List<ChaseVariables>();
}



[Serializable]
public class ChaseVariables
{
    public float startTime;
    public float ballSpeed;
    public float scoreratio;
    public bool isFlickering;
    public float flickerSpeed;
}
