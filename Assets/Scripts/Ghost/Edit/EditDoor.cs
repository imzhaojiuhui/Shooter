using UnityEngine;

namespace Ghost.Edit
{
    [DisallowMultipleComponent]
    public class EditDoor : MonoBehaviour
    {
        public Color gizmoColor = Color.blanchedAlmond;

        private void OnDrawGizmos()
        {
            // var pos = transform.position;
            // pos.y = transform.parent.position.y;

            Gizmos.color = gizmoColor;
            Gizmos.DrawRay(DoorPos, Vector2.up * .5f);
        }

        public Vector2 DoorPos => new Vector2(transform.position.x, transform.parent.position.y);
    }
}