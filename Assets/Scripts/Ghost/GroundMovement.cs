using System;
using UnityEngine;

namespace Ghost
{
    public class GroundMovement:MonoBehaviour
    {
        public Vector2 Velocity {get; set; }
        public float Speed {get; set; }

        private void Start()
        {
            Speed = 1;
        }

        private void Update()
        {
            transform.Translate(Velocity*Speed*Time.deltaTime);
        }
    }
}