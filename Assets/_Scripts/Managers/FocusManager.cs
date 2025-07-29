using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class FocusManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private Transform focus_point;
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private GameObject end_panel;
    [SerializeField] private TextMeshProUGUI timer_txt;
    [SerializeField] private TextMeshProUGUI score_txt;
    [SerializeField] private Image black_screen;
    [SerializeField] private List<float> radius;
    [SerializeField] private UnityEvent GameEnded;
    [SerializeField] private float targetSize = 1;

    private FocusSO activeFocusSO;
    private GameObject spawned_target1;
    private GameObject spawned_target2;
    private Vector3 spawn_pos;
    private float angle;
    private float initial_time;
    private float timer;
    private float life_span;
    private float score;
    private float spawned_targets;
    private float captured_targets;
    private float ellipse_width;
    private float ellipse_height;
    private int missed_targets;
    private int streak;
    private int used_radius;
    private List<float> levelstarttime = new List<float>();
    private List<float> lifespan = new List<float>();
    private List<float> flickeringspeed = new List<float>();
    private List<bool> flickeringenabled = new List<bool>();
    private Dictionary<int, float> left_eye_data = new Dictionary<int, float>();
    private Dictionary<int, float> right_eye_data = new Dictionary<int, float>();

    public Queue<GameObject> targetQueue = new Queue<GameObject>();



    private void Start()
    {
        GameSetup();
        StartCoroutine(GameLoop());
        StartCoroutine(Spawntarget());
        StartCoroutine(ChangeRadius());
        StartCoroutine(Flickering());
    }

    void GameSetup()
    {
        // t1 is the left eye
        // t2 is the right eye

        initial_time = activeFocusSO.timer;
        timer = 0;

        var t1 = Instantiate(target_prefab);
        var t2 = Instantiate(target_prefab);

        t1.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        t2.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;

        t1.GetComponent<ClickableObject>().OnClick.AddListener(TargetCaptured);
        t2.GetComponent<ClickableObject>().OnClick.AddListener(TargetCaptured);

        targetQueue.Enqueue(t1);
        targetQueue.Enqueue(t2);

        t1.SetActive(false);
        t2.SetActive(false);

        if (activeFocusSO != null)
        {
            for (int i = 0; i < activeFocusSO.focuslevels.Count; i++)
            {
                levelstarttime.Add(activeFocusSO.focuslevels[i].starttime);
                lifespan.Add(activeFocusSO.focuslevels[i].lifespan);
                flickeringspeed.Add(activeFocusSO.focuslevels[i].flickeringspeed);
                flickeringenabled.Add(activeFocusSO.focuslevels[i].isflickering);
            }
        }
    }

    public void TargetCaptured(GameObject clickedObj)
    {
        captured_targets++;
        streak++;
        targetQueue.Enqueue(clickedObj);
        clickedObj.SetActive(false);
    }

    public void GameEnd()
    {
        StopAllCoroutines();
        timer_txt.enabled = false;
        end_panel.SetActive(true);

        Destroy(spawned_target1);
        Destroy(spawned_target2);

        score = (captured_targets / spawned_targets) * 100f;
        score_txt.text = score.ToString("F2") + "%";

    }

    public void SetActiveFocusSO(FocusSO val) => activeFocusSO = val;

    IEnumerator ChangeRadius()
    {
        while (true)
        {
            if (streak >= 4)
            {
                if (used_radius < radius.Count - 1)
                {
                    used_radius++;
                }
                streak = 0;
            }
            yield return null;
        }
    }

    IEnumerator Spawntarget()
    {
        float x, y;

        while (true)
        {
            // Ensure at least 2 targets are available
            while (targetQueue.Count < 2)
                yield return null;

            angle = Random.Range(90, 270) * Mathf.Deg2Rad;

            ellipse_width = radius[used_radius];
            ellipse_height = radius[used_radius] * 0.6f;

            x = Mathf.Cos(angle) * ellipse_width;
            y = Mathf.Sin(angle) * ellipse_height;

            spawn_pos = focus_point.position + new Vector3(x, y, 0);
            focus_point.transform.localScale = new Vector3(targetSize, targetSize, targetSize);

            spawned_target1 = targetQueue.Dequeue();
            spawned_target2 = targetQueue.Dequeue();

            spawned_target1.SetActive(true);
            spawned_target2.SetActive(true);

            spawned_target1.transform.position = spawn_pos;
            spawned_target2.transform.position = -spawn_pos;

            spawned_target1.transform.localScale = new Vector3(targetSize, targetSize, targetSize);
            spawned_target2.transform.localScale = new Vector3(targetSize, targetSize, targetSize);

            spawned_targets += 2;

            yield return new WaitForSeconds(life_span);

         
            if (spawned_target1.activeSelf)
            {
                missed_targets++;
                spawned_target1.SetActive(false);
                targetQueue.Enqueue(spawned_target1);
                left_eye_data.Add(missed_targets, timer);
            }

            if (spawned_target2.activeSelf)
            {
                missed_targets++;
                spawned_target2.SetActive(false);
                targetQueue.Enqueue(spawned_target2);
                right_eye_data.Add(missed_targets, timer);
            }
        }
    }

    IEnumerator GameLoop()
    {
        while (timer <= activeFocusSO.timer)
        {
            timer += Time.deltaTime;
            initial_time -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(initial_time / 60f);
            int seconds = Mathf.FloorToInt(initial_time % 60f);
            timer_txt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            for (int i = levelstarttime.Count - 1; i >= 0; i--)
            {
                if (timer >= levelstarttime[i])
                {
                    life_span = lifespan[i];
                    break;
                }
            }

            if (initial_time <= 0)
            {
                GameEnded.Invoke();
            }

            yield return null;
        }
    }

    IEnumerator Flickering()
    {
        float flickerTimer = 0f;
        float currentFlickerSpeed = 0f;
        bool isFlickering = false;

        while (true)
        {
            for (int i = levelstarttime.Count - 1; i >= 0; i--)
            {
                if (timer >= levelstarttime[i])
                {
                    isFlickering = flickeringenabled[i];
                    currentFlickerSpeed = flickeringspeed[i];
                    break;
                }
            }

            if (isFlickering)
            {
                flickerTimer += Time.deltaTime;
                if (flickerTimer >= currentFlickerSpeed)
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
