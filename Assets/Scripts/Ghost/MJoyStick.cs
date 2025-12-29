using System;
using KISS;
using TMPro;
using UnityEngine;

public class MJoyStick : MonoBehaviour
{
    private RectTransform _bg;
    private RectTransform _knob;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _tmp;
    void Start()
    {
        _bg = transform.GetChild(0).GetComponent<RectTransform>();
        _tmp = _bg.GetChild(1).GetComponent<TextMeshProUGUI>();
        _knob = _bg.GetChild(0).GetComponent<RectTransform>();
        _rectTransform = GetComponent<RectTransform>();
        _bg.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        bool w =Input.GetKey(KeyCode.W);
        bool a =Input.GetKey(KeyCode.A);
        bool d =Input.GetKey(KeyCode.D);
        bool s = Input.GetKey(KeyCode.S);
        var moveK = Vector2.zero;
        if (w)
        {
            moveK += Vector2.up;
        }
        if (a)
        {
            moveK += Vector2.left;
        }
        if (d)
        {
            moveK += Vector2.right;
        }
        if (s)
        {
            moveK += Vector2.down;
        }

        if (moveK != Vector2.zero)
        {
            OnMove?.Invoke(moveK);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var uiPos = ScreenToRectTransformPos(Input.mousePosition, _rectTransform);
            _bg.anchoredPosition = uiPos;
            _bg.gameObject.SetActive(true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _bg.gameObject.SetActive(false);
        }

        if (_bg.gameObject.activeSelf)
        {
            var uiPos = ScreenToRectTransformPos(Input.mousePosition, _bg);
            uiPos = Vector2.ClampMagnitude(uiPos, _bg.rect.width*.5f);
            _knob.anchoredPosition = uiPos;

            var dir = uiPos / (_bg.rect.width * .5f);
            _tmp.text = dir.ToString();

            if (dir.sqrMagnitude > .1)
            {
                OnMove?.Invoke(dir);
                // bool vertical = Mathf.Abs(dir.y) > .3f || dir.y > dir.x;
                // bool horizontal = Mathf.Abs(dir.x) > .1f;
                // var move = Vector2.zero;
                // if (horizontal)
                // {
                //     move += Math.Sign(dir.x) * Vector2.right;
                // }
                //
                // if (vertical)
                // {
                //     move += Math.Sign(dir.y) * Vector2.up;
                // }
                //
                // OnMove?.Invoke(move);
            }
            else
            {
                OnMove?.Invoke(Vector2.zero);
            }
        }
        else
        {
            OnMove?.Invoke(Vector2.zero);
        }
    }

    public static event Action<Vector2> OnMove; 
    
    /// <summary>
    /// 屏幕坐标 转 RectTransform 锚点坐标（核心方法）
    /// </summary>
    /// <param name="screenPos">屏幕坐标：Input.mousePosition</param>
    /// <param name="targetRect">目标UI的RectTransform组件</param>
    /// <returns>可直接赋值的 RectTransform 本地坐标</returns>
    public static Vector2 ScreenToRectTransformPos(Vector2 screenPos, RectTransform targetRect)
    {
        // 获取UI的根画布
        Canvas canvas = targetRect.GetComponentInParent<Canvas>();
        // 声明接收转换结果的变量
        Vector2 uiPos = Vector2.zero;
        
        // 核心API：Unity官方提供的坐标转换，一行搞定所有渲染模式适配
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRect,          // 目标UI的RectTransform
            screenPos,           // 待转换的屏幕坐标
            canvas.worldCamera,  // Canvas的相机（Overlay模式下自动为null）
            out uiPos            // 输出：转换后的RectTransform坐标
        );
        
        return uiPos;
    }
}
