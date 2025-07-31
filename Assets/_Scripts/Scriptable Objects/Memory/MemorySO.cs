using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MemorySO", menuName = "Game Type/Memory")]
public class MemorySO : AbsGameSO
{
    public List<MemoryVariables> memorylevels = new List<MemoryVariables>();
}

[Serializable]

public class MemoryVariables
{
    public float starttime;
    public float usertimewindow;
    public int patternsize;
    public int rows;
    public int cols;
    public bool isflickering;
    public float flickeringspeed;
    [Range(1, 10)] public float nottodoprob;
}
