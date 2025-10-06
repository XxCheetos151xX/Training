using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{

    [SerializeField] private SaveAndLoadManager saveandloadmanager;

    [HideInInspector] public float user_score;
    [HideInInspector] public float total_score;
    [HideInInspector] public float final_score;
    [HideInInspector] public float misses;
    [HideInInspector] public float average;
    [HideInInspector] public int lives;
    private string chosen_mode;

    private void Start()
    {
        chosen_mode = PlayerPrefs.GetString("GameMode");
        lives = 3;
    }

    public void CalculateScore()
    {
        final_score = (user_score / total_score) * 100;
        if (final_score <= 0)
        {
            final_score = 0;
        }

        if (chosen_mode == GameMode.GeneralEval.ToString())
        {

            string scene_name = SceneManager.GetActiveScene().name;

            saveandloadmanager.AddScore(scene_name, final_score);
        }
    }

    public void GetAverageScore()
    {
        var saved = saveandloadmanager.LoadScore();

        float sum = 0f;
        foreach (var entry in saved.wrapper)
        {
            sum += entry.score;
        }

        average =  (sum / saved.wrapper.Count);
    }

    public void LoseALife()
    {
        lives -= 1;
        if (lives <= 0)
        {
            lives = 0;
        }
    }
}
