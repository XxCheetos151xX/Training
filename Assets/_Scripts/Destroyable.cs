using UnityEngine;

public class Destroyable : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
