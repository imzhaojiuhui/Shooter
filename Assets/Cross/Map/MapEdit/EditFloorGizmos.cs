using UnityEngine;

namespace Ghost.Edit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EditFloor))]
    public class EditFloorGizmos : MonoBehaviour
    {
        private EditFloor _editFloor;
        public Color gizmoColor = Color.red;

        private void OnDrawGizmos()
        {
            _editFloor ??= GetComponent<EditFloor>();
            Gizmos.color = gizmoColor;
            Gizmos.DrawRay(transform.position, Vector2.right * _editFloor.length);
        }
    }
}