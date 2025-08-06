using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

public class DepthManager : MonoBehaviour
{
    [Header("Game Refrences")]
    [SerializeField] private GameObject player;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Volume volume;
    [SerializeField] private InputActionReference TouchActionRef;

    private DepthSO activeDepthSO;
    private DepthOfField depth_of_field;
    private ClickableObject player_clickable;
    private float timer;
    private float initial_time;
    private float angle;
    private float minX, maxX, minY, maxY;
    private float life_time;
    private float distance;
    private float flickering_speed;
    private bool is_flickering;
    private bool is_touched;
    private Vector3 choosed_pos;
    private List<float> _starttime = new List<float>();
    private List<float> _lifetime = new List<float>();
    private List<float> _distance = new List<float>();
    private List<float> _flickeringspeed = new List<float>();
    private List<bool> _isflickering = new List<bool>();

    private void Start()
    {
        GameSetup();
        StartCoroutine(GameLoop());
    }

    private void OnEnable()
    {
        TouchActionRef.action.performed += OnTouchPerformed;
        TouchActionRef.action.Enable();
    }

    private void OnDisable()
    {
        TouchActionRef.action.performed -= OnTouchPerformed;
        TouchActionRef.action.Disable();
    }
    

    void GameSetup()
    {
        player_clickable = player.GetComponent<ClickableObject>();

        is_touched = false;


        timer = 0;

        initial_time = activeDepthSO.timer;

        if (volume.profile.TryGet<DepthOfField>(out var dof))
        {
            depth_of_field = dof;
        }

        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = Camera.main.orthographicSize * 2;
        float horizontalSize = verticalSize * aspectRatio;
        float halfWidth = horizontalSize / 2f;
        float halfHeight = verticalSize / 2f;
        minY = Camera.main.transform.position.y - halfHeight;
        minX = Camera.main.transform.position.x - halfWidth;
        maxX = Camera.main.transform.position.x + halfWidth;
        maxY = Camera.main.transform.position.y + halfHeight;

        for (int i = 0; i < activeDepthSO.depthlevels.Count; i++)
        {
            _starttime.Add(activeDepthSO.depthlevels[i].starttime);
            _lifetime.Add(activeDepthSO.depthlevels[i].lifetime);
            _distance.Add(activeDepthSO.depthlevels[i].distance);
            _flickeringspeed.Add(activeDepthSO.depthlevels[i].flickeringspeed);
            _isflickering.Add(activeDepthSO.depthlevels[i].isflickering);
        }
    }



    public void PlayerTouched()
    {
        is_touched = true;
    }

    public void PlayerReleased()
    {
        is_touched = false;
    }



    void OnTouchPerformed(InputAction.CallbackContext ctx)
    {
        if (is_touched)
        {
            Vector2 screenPos = ctx.ReadValue<Vector2>();
            Vector3 touchPos = new Vector3(screenPos.x, screenPos.y, 0);
            player.transform.position = mainCam.ScreenToWorldPoint(touchPos);
        }
    }


    void PickDirection()
    {
        angle = Random.Range(0, 360) * Mathf.Deg2Rad;

        choosed_pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized * distance;
    }

    public void SetActiveDepthSO(DepthSO val) => activeDepthSO = val;


    IEnumerator GameLoop()
    {
        while (true)
        {
            timer += Time.deltaTime;

            initial_time -= Time.deltaTime;

            for (int i = 0; i < _starttime.Count; i++)
            {
                if (timer >= _starttime[i])
                {
                    life_time = _lifetime[i];
                    distance = _distance[i];
                    flickering_speed = _flickeringspeed[i];
                    is_flickering = _isflickering[i];
                }
            }
            yield return null;
        }
    }
}
