using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DecisionManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private GameObject left_hand;
    [SerializeField] private GameObject right_hand;
    [SerializeField] private List<Color> colors = new List<Color>();
    [SerializeField] private DecisionSO activeDecisionSO;

    private Camera cam;
    private float timer;
    private float initial_timer;
    private float targetlifespan;
    private float colorchangetime;
    private float flickeringspeed;
    private float minY, minX, maxX, maxY;
    private int index1;
    private int index2;
    private bool isflickering;
    private GameObject spawned_target;
    private SpriteRenderer lefthand_renderer;
    private SpriteRenderer righthand_renderer;
    private SpriteRenderer spawnedTargetRenderer;
    private SpriteRenderer targetFollowHand;
    private List<float> start_time = new List<float>();
    private List<float> target_life_span = new List<float>();
    private List<float> color_change_time = new List<float>();
    private List<float> flickering_speed = new List<float>();
    private List<bool> _isflickering = new List<bool>();

    private void Start()
    {
        GameSetup();
        StartCoroutine(GameLoop());
        StartCoroutine(SwitchColors());
        StartCoroutine(SpawnTargets());
    }

    void GameSetup()
    {
        initial_timer = activeDecisionSO.timer;
        cam = Camera.main;
        timer = 0;

        lefthand_renderer = left_hand.GetComponent<SpriteRenderer>();
        righthand_renderer = right_hand.GetComponent<SpriteRenderer>();

   
        do
        {
            index1 = Random.Range(0, colors.Count);
            index2 = Random.Range(0, colors.Count);
        } while (index1 == index2);

        lefthand_renderer.color = colors[index1];
        righthand_renderer.color = colors[index2];

   
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = cam.transform.position.y - halfHeight;
        minX = cam.transform.position.x - halfWidth;
        maxX = cam.transform.position.x + halfWidth;
        maxY = cam.transform.position.y + halfHeight;

        
        for (int i = 0; i < activeDecisionSO.decisionlevels.Count; i++)
        {
            start_time.Add(activeDecisionSO.decisionlevels[i].starttime);
            target_life_span.Add(activeDecisionSO.decisionlevels[i].targetlifespan);
            color_change_time.Add(activeDecisionSO.decisionlevels[i].color_changetimer);
            flickering_speed.Add(activeDecisionSO.decisionlevels[i].flickerspeed);
            _isflickering.Add(activeDecisionSO.decisionlevels[i].isflickering);
        }
    }

    public void SetActiveDecisionSO(DecisionSO val) => activeDecisionSO = val;

    IEnumerator SwitchColors()
    {
        while (true)
        {
            index1 = Random.Range(0, colors.Count);
            index2 = Random.Range(0, colors.Count);

            if (index1 != index2)
            {
                lefthand_renderer.color = colors[index1];
                righthand_renderer.color = colors[index2];
                yield return new WaitForSeconds(colorchangetime);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator SpawnTargets()
    {
        while (true)
        {
            
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);

            
            bool useLeft = Random.value > 0.5f;
            targetFollowHand = useLeft ? lefthand_renderer : righthand_renderer;

            
            spawned_target = Instantiate(target_prefab, new Vector3(x, y, 0), Quaternion.identity);
            spawnedTargetRenderer = spawned_target.GetComponent<SpriteRenderer>();
            spawnedTargetRenderer.color = targetFollowHand.color;

            float elapsed = 0f;

            
            while (elapsed < targetlifespan)
            {
                if (spawned_target != null && targetFollowHand != null)
                {
                    spawnedTargetRenderer.color = targetFollowHand.color;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (spawned_target != null)
                Destroy(spawned_target);
        }
    }

    IEnumerator GameLoop()
    {
        while (timer != initial_timer)
        {
            initial_timer -= Time.deltaTime;
            timer += Time.deltaTime;

            for (int i = 0; i < start_time.Count; i++)
            {
                if (timer >= start_time[i])
                {
                    targetlifespan = target_life_span[i];
                    colorchangetime = color_change_time[i];
                    isflickering = _isflickering[i];
                    flickeringspeed = flickering_speed[i];
                }
            }

            yield return null;
        }
    }
}
