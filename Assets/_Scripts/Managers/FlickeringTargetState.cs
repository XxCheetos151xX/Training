using UnityEngine;

public class FlickeringTargetState : MonoBehaviour
{
    public bool isclicked = false;
    public bool isleft;
    public CircleCollider2D invertedCollider;

    [Header("Offsets")]
    public float invertedColliderOffset = 1f; // customize per prefab or via manager

    public void UpdateInvertedCollider()
    {
        if (invertedCollider == null)
            return;

        // flip collider offset depending on direction
        invertedCollider.offset = isleft
            ? new Vector2(invertedColliderOffset, 0)
            : new Vector2(-invertedColliderOffset, 0);
    }
}
