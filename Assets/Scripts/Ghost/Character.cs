using System;
using UnityEngine;

namespace Ghost
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GroundMovement))]
    public class Character:MonoBehaviour
    {
        private GroundMovement _groundMovement;
        private void Start()
        {
            _groundMovement = GetComponent<GroundMovement>();
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
            _groundMovement.InputVelocity = dir;
        }
    }
}