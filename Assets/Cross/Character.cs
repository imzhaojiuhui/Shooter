using KISS;
using UnityEngine;

namespace Ghost
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterGroundMovement))]
    public class Character : MonoSingleton<Character>
    {
        private CharacterGroundMovement _groundMovement;
        public CharacterGroundMovement GroundMovement => _groundMovement;

        private void Start()
        {
            _groundMovement = GetComponent<CharacterGroundMovement>();
            MJoyStick.OnMove += OnMove;
        }

        private void OnDestroy()
        {
            MJoyStick.OnMove -= OnMove;
        }

        private void LateUpdate()
        {
            var cPos = transform.position;
            cPos.z = Camera.main.transform.position.z;
            Camera.main.transform.position = cPos;
        }

        private void OnMove(Vector2 dir)
        {
            _groundMovement.InputVelocity = dir;
        }
    }
}