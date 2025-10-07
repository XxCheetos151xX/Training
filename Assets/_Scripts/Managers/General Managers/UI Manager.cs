using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timer_txt;
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private TextMeshProUGUI next_game_title;
    [SerializeField] private TextMeshProUGUI lives_txt;

    [Header("Result Prefab & Container")]
    [SerializeField] private GameObject resultTextPrefab;   // Prefab with TextMeshProUGUI
    [SerializeField] private Transform resultsContainer;
    [SerializeField] private Transform lowestResultsContainer;
    [SerializeField] private TextMeshProUGUI average_score_txt;

    [Header("Panels")]
    [SerializeField] private GameObject ending_panel;
    [SerializeField] private GameObject sequence_ended_panel;
    [SerializeField] private GameObject generaleval_ending_panel;

    [Header("Managers")]
    [SerializeField] private SaveAndLoadManager save_and_load_manager;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private AbstractGameManager manager;

    [SerializeField] private GameObject sequence_manager_prefab;
    [SerializeField] private TextMeshProUGUI game_description_txt;
    [SerializeField] private Image game_image;

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
            next_game_title.text = "Next Game Is " + SequenceManager.Instance.drillScenes[SequenceManager.Instance.currentIndex].ToString();
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
                string nextScene = SequenceManager.Instance.drillScenes[SequenceManager.Instance.currentIndex + 1];
                next_game_title.text = "Next Game Is " + nextScene;
            }
        }
        else if (SequenceManager.Instance.currentIndex >= SequenceManager.Instance.drillScenes.Count - 1)
        {
            score_manager.GetAverageScore();
            ShowGeneralEvalEnding();
        }
    }

    // ================== General Eval Ending ==================
    private void ShowGeneralEvalEnding()
    {
        if (generaleval_ending_panel != null)
        {
            generaleval_ending_panel.SetActive(true);

            // Clear old children first
            foreach (Transform child in resultsContainer)
                Destroy(child.gameObject);

            foreach (Transform child in lowestResultsContainer)
                Destroy(child.gameObject);

            if (save_and_load_manager != null)
            {
                var saved = save_and_load_manager.LoadScore();

                if (saved.wrapper.Count == 0)
                {
                    var noResultObj = Instantiate(resultTextPrefab, resultsContainer);
                    var noResultText = noResultObj.GetComponent<TextMeshProUGUI>();
                    noResultText.text = "No results recorded.";
                }
                else
                {
                    // --- Show all results ---
                    var validScenes = SequenceManager.Instance.drillScenes;

                    var filteredResults = saved.wrapper
                        .Where(entry => validScenes.Contains(entry.scene_name))
                        .ToList();

                    foreach (var entry in filteredResults)
                    {
                        var textObj = Instantiate(resultTextPrefab, resultsContainer);
                        var textInstance = textObj.GetComponent<TextMeshProUGUI>();
                        textInstance.enabled = true;
                        textInstance.text = $"{entry.scene_name}: {entry.score:F1}%";
                    }

                    // --- Show 3 lowest scores (only from valid general eval scenes) ---
                    var lowestThree = saved.wrapper
                        .Where(entry => validScenes.Contains(entry.scene_name))
                        .OrderBy(entry => entry.score)
                        .Take(3);


                    foreach (var entry in lowestThree)
                    {
                        var textObj = Instantiate(resultTextPrefab, lowestResultsContainer);
                        var textInstance = textObj.GetComponent<TextMeshProUGUI>();
                        textInstance.enabled = true;
                        textInstance.text = $"{entry.scene_name}";
                    }

                    // --- Average ---
                    average_score_txt.text = "CI Index: \n" + score_manager.average.ToString("F1") + "%";
                }
            }
        }
    }


    // ================== Start General Eval ==================
    public void StartGeneralEval()
    {
        PlayerPrefs.SetString("GameMode", GameMode.GeneralEval.ToString());
        SequenceManager.Instance.StartSequence();
    }



    public void SetDescription(int indexVal)
    {
        game_description_txt.text = games_description[indexVal].game_description;
        game_image.sprite = games_description[indexVal].game_image;
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
