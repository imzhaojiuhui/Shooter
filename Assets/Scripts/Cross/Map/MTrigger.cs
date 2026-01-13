using System.Collections;
using UnityEngine;

namespace Ghost.Terrain
{
    public class MTrigger : MonoBehaviour
    {
        private bool _stay = false;
        private float _timer = 0;

        private IEnumerator Start()
        {
            while (true)
            {
                if (_stay)
                {
                    Debug.Log(_timer);
                }

                yield return new WaitForSeconds(1);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _timer = 0;
            _stay = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _timer = 0;
            _stay = false;
        }

        private void Update()
        {
            if (_stay)
            {
                _timer += Time.deltaTime;
            }
        }
    }
}