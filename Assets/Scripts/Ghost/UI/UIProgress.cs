using KISS;
using UnityEngine;
using UnityEngine.UI;

namespace Ghost.UI
{
    public class UIProgress : MonoSingleton<UIProgress>
    {
        private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
        }

        public float Progress
        {
            set { _image.fillAmount = value; }
        }

        public void SetPosition(Transform trans)
        {
        }
    }
}