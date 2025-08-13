using UnityEngine;

public abstract class AbstractGameManager : MonoBehaviour
{
    [HideInInspector] public float initial_timer;

    public virtual void TargetClicked(GameObject t)
    {
    }
}
