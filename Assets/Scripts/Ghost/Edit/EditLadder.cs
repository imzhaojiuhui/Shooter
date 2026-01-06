using UnityEngine;

namespace Ghost.Edit
{
    [DisallowMultipleComponent]
    public class EditLadder : MonoBehaviour
    {
        public Transform floorA;
        public Transform floorB;
        public float enterWidth = 2.0f;
        public float leaveMaxHeight = 2.0f;

        public Color gizmoColor = Color.gold;

        private void OnDrawGizmos()
        {
            if (floorA == null || floorB == null)
            {
                return;
            }

            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(PosHigh, PosLow);

            Gizmos.DrawWireCube(EnterDownRect.center, EnterDownRect.size);
            Gizmos.DrawWireCube(EnterUpRect.center, EnterUpRect.size);

            Gizmos.DrawWireCube(LeaveUpRect.center, LeaveUpRect.size);
            Gizmos.DrawWireCube(LeaveDownRect.center, LeaveDownRect.size);
        }

        public float PosX => transform.position.x;
        public Vector2 PosHigh => new Vector2(PosX, Mathf.Max(floorA.position.y, floorB.position.y));
        public Vector2 PosLow => new Vector2(PosX, Mathf.Min(floorA.position.y, floorB.position.y));

        public Rect EnterUpRect
        {
            get { return new Rect(PosLow.x - enterWidth / 2f, PosLow.y - .1f, enterWidth, .2f); }
        }

        public Rect EnterDownRect => new Rect(PosHigh.x - enterWidth / 2f, PosHigh.y - .1f, enterWidth, .2f);

        public Rect LeaveUpRect => new Rect(PosHigh.x - .1f, PosHigh.y - leaveMaxHeight + .1f, .2f, leaveMaxHeight);
        public Rect LeaveDownRect => new Rect(PosLow.x - .1f, PosLow.y - .1f, .2f, leaveMaxHeight);
    }
}