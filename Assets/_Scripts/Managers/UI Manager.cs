using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private string scene_name;
    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Restart()
    {
        scene_name = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene_name);
    }
} 