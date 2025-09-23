using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MemorySO", menuName = "Game Type/Memory", order = 5)]
public class MemorySO : AbsGameSO
{
    public List<MemoryVariables> memorylevels = new List<MemoryVariables>();
}

[Serializable]

public class MemoryVariables
{
    public float flickerstarttime;
    public float patternspeed;
    public float usertimewindow;
    public float scoreratio;
    public int patternsize;
    public int rows;
    public int cols;
    public bool isflickering;
    public float flickeringspeed;
}
