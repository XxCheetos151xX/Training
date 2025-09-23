using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "ComplexFunctionsSO", menuName = "Game Type/ Complex Functions", order = 10)]
public class ComplexFunctionsSO : AbsGameSO
{
    public List<ComplexFunctionVariables> complexfunctionslevels = new List<ComplexFunctionVariables>();
}
[Serializable]
public class ComplexFunctionVariables
{
    public float starttime;
    public float flickeringtime;
    public float minflickeringcooldown;
    public float maxflickeringcooldown;
    public float targetlifespan;
    public float mindelaybetweentargets;
    public float maxdelaybetweentargets;
    public float scoreratio;
    public bool isflickeringtogether;
}
