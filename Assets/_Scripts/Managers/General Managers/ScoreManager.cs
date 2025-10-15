using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{

    [SerializeField] private SaveAndLoadManager saveandloadmanager;

    [HideInInspector] public float user_score;
    [HideInInspector] public float total_score;
    [HideInInspector] public float final_score;
    [HideInInspector] public float misses;
    [HideInInspector] public float average;
    [HideInInspector] public int lives;
    [HideInInspector] public float vision_score;
    [HideInInspector] public float anticipation_score;
    [HideInInspector] public float reaction_time_score;
    [HideInInspector] public float inhibition_control_score;
    [HideInInspector] public List<string> improvement_needed = new List<string>(); 

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

        var lowestThree = saved.wrapper
       .OrderBy(entry => entry.score)
       .Take(3);

        foreach (var entry in saved.wrapper)
        {
            improvement_needed.Add(entry.scene_name);
        }

        improvement_needed.Sort((a,b) => a.Length.CompareTo(b.Length));

    }



    public void LoseALife()
    {
        lives -= 1;
        if (lives <= 0)
        {
            lives = 0;
        }
    }

    public void CalculateTraitsScores()
    {
        var saved = saveandloadmanager.LoadScore();

        float focus = 0;
        float spacing = 0;
        float flickering = 0;
        float memory = 0;
        float chase = 0;
        float recognition = 0;
        float piriority = 0;
        float noisy = 0;
        float decision = 0;
        float complex = 0;

        foreach (var entry in saved.wrapper)
        {
            string scene_name = entry.scene_name;
            
            if (scene_name == "Peripheral_Field")
            {
                focus = entry.score;
            }
            if (scene_name == "Space_Recognition")
            {
                spacing = entry.score;
            }
            if (scene_name == "Flickering")
            {
                flickering = entry.score;
            }
            if (scene_name == "Memory")
            {
                memory = entry.score;
            }
            if (scene_name == "Eye_Tracking")
            {
                chase = entry.score;
            }
            if (scene_name == "Recognition_Speed")
            {
                recognition = entry.score;
            }
            if (scene_name == "Risk_Management")
            {
                piriority = entry.score;
            }
            if (scene_name == "Noisy_Focus")
            {
                noisy = entry.score;
            }
            if (scene_name == "Decision_Making")
            {
                decision = entry.score;
            }
            if (scene_name == "Complex_Functions")
            {
                complex = entry.score;
            }
        }

        vision_score = (focus + flickering + spacing) / 3;
        anticipation_score = (memory + chase + recognition) / 3;
        reaction_time_score = (piriority + noisy) / 2;
        inhibition_control_score = (complex + decision) / 2;

    }
}
