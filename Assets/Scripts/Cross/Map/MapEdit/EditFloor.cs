using UnityEngine;

namespace Ghost.Edit
{
    [DisallowMultipleComponent]
    public class EditFloor : MonoBehaviour
    {
        // public Vector2 start;
        public float length;

        public Vector2 Start => transform.position;
        public Vector2 End => new Vector2(transform.position.x + length, transform.position.y);
    }
}