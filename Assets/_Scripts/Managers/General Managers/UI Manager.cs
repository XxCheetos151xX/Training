using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timer_txt;
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private GameObject ending_panel;
    [SerializeField] private ScoreManager score_manager;
    [SerializeField] private AbstractGameManager manager;
    [SerializeField] private bool isvideo;
    
    public VideoPlayer video_player;

    [HideInInspector] public double remaining;
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


    public IEnumerator Timer()
    {
        while (true)
        {
            if (!isvideo)
            {
                manager.initial_timer -= Time.deltaTime;
                int minutes = Mathf.FloorToInt(manager.initial_timer / 60f);
                int seconds = Mathf.FloorToInt(manager.initial_timer % 60f);
                timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                yield return null;
            }
            else if (isvideo && video_player != null)
            {
                remaining = video_player.length - video_player.time;

                int minutes = Mathf.FloorToInt((float)remaining / 60f);
                int seconds = Mathf.FloorToInt((float)remaining % 60f);
                timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                yield return null;
            }
        }
    }
}