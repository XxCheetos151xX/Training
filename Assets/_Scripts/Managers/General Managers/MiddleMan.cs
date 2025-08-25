using UnityEngine;

public class MiddleMan : MonoBehaviour
{
    public static MiddleMan Instance;

    #region Singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    #endregion

    public NoisyFocusManager noisyfocus_manager;
    public ScoreManager score_manager;
}