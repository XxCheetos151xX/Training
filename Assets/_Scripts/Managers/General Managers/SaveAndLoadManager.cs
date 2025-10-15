using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

public class SaveAndLoadManager : MonoBehaviour
{
    public Wrapper saved_scores;

    public UserVariables user_data;

    public string scores_path => Path.Combine(Application.persistentDataPath, "results.json");
    public string userdata_path => Path.Combine(Application.persistentDataPath, "UserData.json");

    public void SaveScores()
    {
        if (saved_scores == null) return;

        string json = JsonUtility.ToJson(saved_scores, true); 
        File.WriteAllText(scores_path, json);
    }




    public void AddScore(string scene_name, float score)
    {
        var currentData = LoadScore();

        ScoreVaraibles new_save = new ScoreVaraibles
        {
            scene_name = scene_name,
            score = score
        };

        
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
        if (File.Exists(scores_path))
        {
            string json = File.ReadAllText(scores_path);
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

    public void SaveData()
    {
        if (user_data == null) return;

        string json = JsonUtility.ToJson(user_data);

        File.WriteAllText(userdata_path, json);
    }

    public UserVariables LoadData()
    {
        if (File.Exists(userdata_path))
        {
            string json = File.ReadAllText(userdata_path);

            var laoded = JsonUtility.FromJson<UserVariables>(json);

            if (laoded != null)
            {
                user_data = laoded;
            }
            else
                user_data = new UserVariables();

        }
        return user_data;
    }
}



[Serializable]
public class UserVariables
{
    public string name;
    public string age;
    public string sport;
    public string position;
}


[Serializable]
public class ScoreVaraibles
{
    public float score;
    public string scene_name;
}

[Serializable]
public class Wrapper
{
    public List<ScoreVaraibles> wrapper = new List<ScoreVaraibles>();
}
