using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveAndLoadManager : MonoBehaviour
{
    public Wrapper saved_scores;
    public string path => Path.Combine(Application.persistentDataPath, "results.json");

    public void SaveScores()
    {
        if (saved_scores == null) return;

        string json = JsonUtility.ToJson(saved_scores, true); 
        File.WriteAllText(path, json);
    }

    public void AddScore(string scene_name, float score)
    {
        // Always load from disk first
        var currentData = LoadScore();

        SaveVaraibles new_save = new SaveVaraibles
        {
            scene_name = scene_name,
            score = score
        };

        // Check if score for this scene already exists → update instead of duplicating
        var existing = currentData.wrapper.Find(x => x.scene_name == scene_name);
        if (existing != null)
        {
            existing.score = score;
        }
        else
        {
            currentData.wrapper.Add(new_save);
        }

        saved_scores = currentData;
        SaveScores();
    }

    public Wrapper LoadScore()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var loaded = JsonUtility.FromJson<Wrapper>(json);

            if (loaded != null)
            {
                saved_scores = loaded;
            }
            else
            {
                saved_scores = new Wrapper();
            }
        }
        else
        {
            saved_scores = new Wrapper();
        }

        return saved_scores;
    }
}

[Serializable]
public class SaveVaraibles
{
    public float score;
    public string scene_name;
}

[Serializable]
public class Wrapper
{
    public List<SaveVaraibles> wrapper = new List<SaveVaraibles>();
}
