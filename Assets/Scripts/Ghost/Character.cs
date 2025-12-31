using Ghost.Terrain;
using UnityEngine;

namespace Ghost
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CrossMovement))]
    public class Character : MonoBehaviour
    {
        private CrossMovement _movement;

        private void Start()
        {
            _movement = GetComponent<CrossMovement>();
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