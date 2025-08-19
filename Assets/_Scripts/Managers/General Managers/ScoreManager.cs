using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [HideInInspector] public float user_score;
    [HideInInspector] public float total_score;
    [HideInInspector] public float final_score;
    [HideInInspector] public float misses;

    public void CalculateScore()
    {
        final_score = (user_score / total_score) * 100;
        if (final_score <= 0)
        {
            final_score = 0;
        }
    }
}
