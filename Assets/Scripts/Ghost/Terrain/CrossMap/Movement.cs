using UnityEngine;

namespace Ghost.Terrain
{
    [DisallowMultipleComponent]
    public abstract class Movement : MonoBehaviour
    {
        public float baseSpeed = 3;
        public Vector2 InputVelocity { get; set; }

        public float Speed
        {
            get { return baseSpeed; }
        }
    }
}