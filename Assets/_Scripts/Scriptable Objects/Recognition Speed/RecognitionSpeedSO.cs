using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecognitionSpeedSO", menuName = "Game Type/ Recognition Speed", order = 8)]
public class RecognitionSpeedSO : AbsGameSO
{
    public List<RecognitionSpeedVariables> recognitionspeedlevels = new List<RecognitionSpeedVariables>(); 
}

[Serializable]
public class RecognitionSpeedVariables
{
    public float starttime;
    public float lifespan;
    public float speed;
    public int size;
    public bool isflickering;
    public float flickeringspeed;
}
