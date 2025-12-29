using System;
using KISS;
using UnityEngine;

namespace Ghost
{
    public class CameraManager: MonoSingleton<CameraManager>
    {
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            
        }
    }
}