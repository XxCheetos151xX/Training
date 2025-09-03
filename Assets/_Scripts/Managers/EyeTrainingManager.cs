using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EyeTrainingManager : MonoBehaviour
{
    [SerializeField] private UIManager uimanager;
    [SerializeField] private UnityEvent VideoEnded;

    private void Start()
    {
        StartCoroutine(uimanager.Timer());
        StartCoroutine(CheckTimer());
    }


    IEnumerator CheckTimer()
    {
        while (true)
        {
            if (uimanager.video_player != null && uimanager.video_player.isPrepared)
            {
                if (uimanager.remaining <= 0.1) // small buffer
                {
                    VideoEnded.Invoke();
                    yield break; // stop checking after firing
                }
            }
            yield return null;
        }
    }

}
