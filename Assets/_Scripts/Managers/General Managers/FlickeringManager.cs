using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlickeringManager : MonoBehaviour
{
    [SerializeField] private Image black_screen;
    public bool isflickering { get; set; }
    public float flickeringspeed { get; set; }

    


    public IEnumerator Flickering()
    {
        float flickerTimer = 0f;

        while (true)
        {

            if (isflickering)
            {
                flickerTimer += Time.deltaTime;
                if (flickerTimer >= flickeringspeed)
                {
                    black_screen.enabled = !black_screen.enabled;
                    flickerTimer = 0f;
                }
            }
            else
            {
                if (black_screen.enabled)
                    black_screen.enabled = false;
            }

            yield return null;
        }


    }
}
