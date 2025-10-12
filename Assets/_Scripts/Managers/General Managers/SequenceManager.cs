using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SequenceManager : MonoBehaviour
{
    public static SequenceManager Instance;

    [Header("Drill Order")]
    public List<string> drillScenes;

    public GameObject ending_panel;

    [HideInInspector] public int currentIndex = -1;

    private void Awake()
    {
        // --- Singleton pattern ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Listen to scene load events
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //private void OnDestroy()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    // When we reach main menu or any non-general eval scene,
    //    // destroy this SequenceManager if not in GeneralEval mode
    //    string chosenMode = PlayerPrefs.GetString("GameMode", "None");

    //    if (scene.name == "Main Menu" && chosenMode != GameMode.GeneralEval.ToString())
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    // --- Start the sequence ---
    public void StartSequence()
    {
        currentIndex = 0;
        PlayerPrefs.SetString("GameMode", GameMode.GeneralEval.ToString());
        SceneManager.LoadScene(drillScenes[currentIndex]);
    }

    // --- Load the next scene in the sequence ---
    public void LoadNextScene()
    {
        PlayerPrefs.SetString("GameMode", GameMode.GeneralEval.ToString());

        currentIndex++;

        if (currentIndex >= drillScenes.Count)
        {
            Debug.Log("Sequence finished!");
            return;
        }

        SceneManager.LoadScene(drillScenes[currentIndex]);
    }
}
