using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class SequenceManager : MonoBehaviour
{
    public static SequenceManager Instance;

    [Header("Drill Order")]
    public List<string> drillScenes;

    public GameObject ending_panel;

    [HideInInspector] public int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        string chosenMode = PlayerPrefs.GetString("GameMode", "None");
        if (chosenMode != GameMode.GeneralEval.ToString())
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartSequence()
    {
        currentIndex = 0;

        SceneManager.LoadScene(drillScenes[currentIndex]);
    }

    public void LoadNextScene()
    {

        PlayerPrefs.SetString("GameMode", GameMode.GeneralEval.ToString());

        currentIndex++;

        SceneManager.LoadScene(drillScenes[currentIndex]);
    }

}
