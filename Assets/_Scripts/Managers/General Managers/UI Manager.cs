using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Mathematics;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timer_txt;
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private TextMeshProUGUI next_game_title;
    [SerializeField] private TextMeshProUGUI lives_txt;

    [Header("CI Index Scores")]
    [SerializeField] private TextMeshProUGUI vision_score_txt;
    [SerializeField] private TextMeshProUGUI anticipation_score_txt;
    [SerializeField] private TextMeshProUGUI reaction_time_score_txt;
    [SerializeField] private TextMeshProUGUI inhibition_control_score_txt;
    [SerializeField] private TextMeshProUGUI ci_index_score_txt;
    [SerializeField] private TextMeshProUGUI lowest_1_txt;
    [SerializeField] private TextMeshProUGUI lowest_2_txt;
    [SerializeField] private TextMeshProUGUI lowest_3_txt;
    [SerializeField] private Image vision_circle;
    [SerializeField] private Image anticipation_circle;
    [SerializeField] private Image reaction_time_circle;
    [SerializeField] private Image inhibiton_circle;
    [SerializeField] private Color weak_color;
    [SerializeField] private Color normal_color;
    [SerializeField] private Color good_color;
 

    [Header("Panels")]
    [SerializeField] private GameObject ending_panel;
    [SerializeField] private GameObject sequence_ended_panel;
    [SerializeField] private GameObject generaleval_ending_panel;

    [Header("Managers")]
    [SerializeField] private SaveAndLoadManager save_and_load_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private AbstractGameManager manager;

    [SerializeField] private GameObject sequence_manager_prefab;
    [SerializeField] private TextMeshProUGUI game_title_txt;
    [SerializeField] private TextMeshProUGUI game_description_txt;
    [SerializeField] private TextMeshProUGUI game_tutorial_txt;
    [SerializeField] private Image game_image;

    [Header("User Data")]
    [SerializeField] private TMP_InputField name_input;
    [SerializeField] private TMP_InputField age_input;
    [SerializeField] private TMP_InputField sport_input;
    [SerializeField] private TMP_InputField pos_input;
    [SerializeField] private TextMeshProUGUI name_txt;
    [SerializeField] private TextMeshProUGUI age_txt;
    [SerializeField] private TextMeshProUGUI sport_txt;
    [SerializeField] private TextMeshProUGUI pos_txt;


    [Header("Video Mode")]
    [SerializeField] private bool isvideo;

    [HideInInspector] public double remaining;

    public VideoPlayer video_player;

    public List<GameDescriptionSO> games_description = new List<GameDescriptionSO>();

    private string chosen_mode;
    



    private void Start()
    {
        chosen_mode = PlayerPrefs.GetString("GameMode");

        if (chosen_mode == GameMode.Timeless.ToString() && timer_txt != null && lives_txt != null)
        {
            timer_txt.enabled = false;
            lives_txt.enabled = true;  
        }
        if (ending_panel != null)
        {
            ending_panel.SetActive(false);
        } 
    }

    // ================== Menu Buttons ==================
    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }

    // ================== Game End ==================
    public void GameEnd()
    {
        StopAllCoroutines();

        if (score_manager != null)
            score_txt.text = score_manager.final_score.ToString("F2") + "%";

        if (timer_txt != null)
            timer_txt.enabled = false;

        if (ending_panel != null)
            ending_panel.SetActive(true);
    }

    public void StartSequencePanel()
    {
        if (sequence_ended_panel != null)
        {
            sequence_ended_panel.SetActive(true);
        }

        if (next_game_title != null)
        {
            next_game_title.text = "Next Game Is " + SequenceManager.Instance.drillScenes[SequenceManager.Instance.currentIndex].ToString().Replace("_"," ");
        }
    }

    // ================== Sequence End ==================
    public void SequenceEnded()
    {
        StopAllCoroutines();


        if (timer_txt != null)
            timer_txt.enabled = false;

        if (SequenceManager.Instance.currentIndex < SequenceManager.Instance.drillScenes.Count - 1)
        {
            if (sequence_ended_panel != null)
                sequence_ended_panel.SetActive(true);

            if (next_game_title != null)
            {
                string nextScene = SequenceManager.Instance.drillScenes[SequenceManager.Instance.currentIndex + 1].Replace("_"," ");
                next_game_title.text = "Next Game Is " + nextScene;
            }
        }
        else if (SequenceManager.Instance.currentIndex >= SequenceManager.Instance.drillScenes.Count - 1)
        {
            score_manager.CalculateTraitsScores();
            score_manager.GetAverageScore();
            SetAnticipationCircle();
            SetInhibitionControlCircle();
            SetReactionTimeCircle();
            SetVisionCircle();
            ShowGeneralEvalEnding();
        }
    }

    // ================== General Eval Ending ==================
    private void ShowGeneralEvalEnding()
    {
        if (generaleval_ending_panel != null)
        {
            generaleval_ending_panel.SetActive(true);
        }

        if (ci_index_score_txt != null)
        {
            ci_index_score_txt.text = "CI Index :" + " " + score_manager.average.ToString("F2");
        }

        if (vision_score_txt != null)
        {
            vision_score_txt.text = score_manager.vision_score.ToString("F2") + "%";
        }

        if (anticipation_score_txt != null)
        {
            anticipation_score_txt.text = score_manager.anticipation_score.ToString("F2") + "%";
        }

        if (reaction_time_score_txt != null)
        {
            reaction_time_score_txt.text = score_manager.reaction_time_score.ToString("F2") + "%";
        }

        if (inhibition_control_score_txt != null)
        {
            inhibition_control_score_txt.text = score_manager.inhibition_control_score.ToString("F2") + "%";
        }

        if (lowest_1_txt != null && lowest_2_txt != null && lowest_3_txt != null)
        {
            lowest_1_txt.text = score_manager.improvement_needed[score_manager.improvement_needed.Count - 1].Replace("_", " ");
            lowest_2_txt.text = score_manager.improvement_needed[score_manager.improvement_needed.Count - 2].Replace("_", " ");
            lowest_3_txt.text = score_manager.improvement_needed[score_manager.improvement_needed.Count - 3].Replace("_", " ");
        }
        GetUserData();
    }


    // ================== Start General Eval ==================
    public void StartGeneralEval()
    {
        PlayerPrefs.SetString("GameMode", GameMode.GeneralEval.ToString());
        SequenceManager.Instance.StartSequence();
    }

    public void ReadUserData()
    {
        if (save_and_load_manager != null && name_input != null && age_input != null && sport_input != null && pos_input != null)
        {
            save_and_load_manager.user_data = new UserVariables
            {
                name = name_input.text,
                age = age_input.text,
                sport = sport_input.text,
                position = pos_input.text
            };

            save_and_load_manager.SaveData();
        }
    }

    public void GetUserData()
    {
        if (save_and_load_manager == null) return;

        var data = save_and_load_manager.LoadData();

        if (data != null)
        {
            if (name_txt != null)
            {
                name_txt.text = "Name :"+" "+data.name;
            }
            if (age_txt != null)
            {
                age_txt.text = "Age :" + " " + data.age;
            }
            if (sport_txt != null)
            {
                sport_txt.text = "Sport :" + " " + data.sport;
            }
            if (pos_txt != null)
            {
                pos_txt.text = "Position :" + " " + data.position;
            }
        }
    }

    public void SetDescription(int indexVal)
    {
        if (game_title_txt != null)
        {
            game_title_txt.text = games_description[indexVal].game_title;
        }

        if (game_tutorial_txt != null)
        {
            game_tutorial_txt.text = games_description[indexVal].game_tutorial;
        }

        if (game_description_txt != null)
        {
            game_description_txt.text = games_description[indexVal].game_description;
        }

        if (game_image != null)
        {
            game_image.sprite = games_description[indexVal].game_image;
        }
    }


    public void SetVisionCircle()
    {
        if (vision_circle != null)
        {
            vision_circle.fillAmount = score_manager.vision_score / 100;

            if (score_manager.vision_score >= 0 && score_manager.vision_score <= 30)
            {
                vision_circle.color = weak_color;
            }

            else if (score_manager.vision_score >= 30.1f && score_manager.vision_score <= 70)
            {
                vision_circle.color = normal_color;
            }

            else if (score_manager.vision_score >= 70.1f && score_manager.vision_score <= 100)
            {
                vision_circle.color = good_color;
            }
        }
    }

    public void SetAnticipationCircle()
    {
        if (anticipation_circle != null)
        {
            anticipation_circle.fillAmount = score_manager.anticipation_score / 100;

            if (score_manager.anticipation_score >= 0 && score_manager.anticipation_score <= 30)
            {
                anticipation_circle.color = weak_color;
            }

            else if (score_manager.anticipation_score >= 30.1f && score_manager.anticipation_score <= 70)
            {
                anticipation_circle.color = normal_color;
            }

            else if (score_manager.anticipation_score >= 70.1f && score_manager.anticipation_score <= 100)
            {
                anticipation_circle.color = good_color;
            }
        }
    }

    public void SetReactionTimeCircle()
    {
        if (reaction_time_circle != null)
        {
            reaction_time_circle.fillAmount = score_manager.reaction_time_score / 100;

            if (score_manager.reaction_time_score >= 0 && score_manager.reaction_time_score <= 30)
            {
                reaction_time_circle.color = weak_color;
            }

            else if (score_manager.reaction_time_score >= 30.1 && score_manager.reaction_time_score <= 70)
            {
                reaction_time_circle.color = normal_color;
            }

            else if (score_manager.reaction_time_score >= 70.1f && score_manager.reaction_time_score <= 100)
            {
                reaction_time_circle.color = good_color;
            }
        }
    }

    public void SetInhibitionControlCircle()
    {
        if (inhibiton_circle != null)
        {
            inhibiton_circle.fillAmount = score_manager.inhibition_control_score / 100;

            if (score_manager.inhibition_control_score >= 0 && score_manager.inhibition_control_score <= 30)
            {
                inhibiton_circle.color = weak_color;
            }

            else if (score_manager.inhibition_control_score >= 30.1f && score_manager.inhibition_control_score <= 70)
            {
                inhibiton_circle.color = normal_color;
            }

            else if (score_manager.inhibition_control_score >= 70.1f && score_manager.inhibition_control_score <= 100)
            {
                inhibiton_circle.color = good_color;
            }
        }
    }

    public IEnumerator Lives()
    {
        while (true)
        {
            lives_txt.text = "Tries:" + " " + score_manager.lives.ToString();
            yield return null;
        }
    }

    // ================== Timer ==================
    public IEnumerator Timer()
    {
        while (true)
        {
            if (!isvideo)
            {
                int minutes = Mathf.FloorToInt(manager.initial_timer / 60f);
                int seconds = Mathf.FloorToInt(manager.initial_timer % 60f);
                timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            else if (isvideo && video_player != null)
            {
                remaining = video_player.length - video_player.time;
                int minutes = Mathf.FloorToInt((float)remaining / 60f);
                int seconds = Mathf.FloorToInt((float)remaining % 60f);
                timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            yield return null;
        }
    }

}
