using Ghost.Terrain;
using UnityEngine;

namespace Ghost
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CrossMovementV2))]
    public class Character : MonoBehaviour
    {
        private CrossMovementV2 _movement;

        private void Start()
        {
            _movement = GetComponent<CrossMovementV2>();
            MJoyStick.OnMove += OnMove;
        }

        private void OnDestroy()
        {
            MJoyStick.OnMove -= OnMove;
        }

        private void Update()
        {
            var cPos = transform.position;
            cPos.z = Camera.main.transform.position.z;
            Camera.main.transform.position = cPos;
        }

        private void OnMove(Vector2 dir)
        {
            _movement.InputVelocity = dir;
        }
    }
}