using System;
using Cysharp.Threading.Tasks;
using KISS;
using TMPro;
using UnityEngine;

namespace Ghost.UI
{
    [DisallowMultipleComponent]
    public class Tips: MonoSingleton<Tips>
    {
        public TextMeshProUGUI _tmp;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
            gameObject.SetActive(false);
        }

        public async UniTaskVoid Pop(string text)
        {
            gameObject.SetActive(true);
            _tmp.text = text;
            await UniTask.Delay(1000);
            gameObject.SetActive(false);
        }
    }
}