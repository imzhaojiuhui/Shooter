using System;
using System.Collections.Generic;
using Ghost;
using Ghost.UI;
using KISS;
using UnityEngine;

/// <summary>
/// Unity 纯2D正交场景 终极完整版
/// 全部功能：
/// 1. 鼠标左键拖动 → 绘制【纯水平横线/垂直竖线】+ 自动存储所有线段数据
/// 2. Ctrl+鼠标左键拖动 → 平移相机视角
/// 3. 按下鼠标滚轮(中键)+拖动 → 平移相机视角 (★新增核心功能★)
/// 4. 滑动鼠标滚轮 → 相机视野缩放(收缩/扩张)
/// 5. 按住D键+鼠标拖动 → 精准删除鼠标路径经过的所有线段
/// 6. 支持清空线段、筛选横竖线、删除指定线段等数据操作
/// </summary>
public class WorldLineDraw2D : MonoBehaviour
{
    [Header("2D画线配置")] public Color lineColor = Color.red;
    public float lineWidth = 0.1f;
    public Camera mainCamera;
    public float minLineLength = 0.1f;

    [Header("相机控制配置【重要】")] public float cameraMoveSpeed = 5f; // 相机平移速度
    public float cameraZoomSpeed = 0.5f; // 滚轮缩放速度
    public float minCameraSize = 1f; // 相机最小视野尺寸
    public float maxCameraSize = 20f; // 相机最大视野尺寸

    [Header("删除线条配置")] public float lineCheckRadius = 0.2f; // 删除时鼠标检测半径
    public Color deleteLineHintColor = Color.yellow;

    [Header("内部状态")] private bool isDrawing = false;
    private bool isMoveCamera = false; // 是否正在平移相机
    private bool isDeleteLine = false; // 是否正在删除线条
    private Vector2 startWorldPos;
    private Vector2 endWorldPos;
    private Vector2 lastMouseScreenPos; // 上一帧鼠标屏幕坐标(相机平移专用)
    private LineRenderer previewLine;
    private LineRenderer deletePreviewLine;

    #region 线段数据存储结构

    [Serializable]
    public class Line2DData
    {
        public Vector2 startPos;

        public Vector2 endPos;

        // public LineType lineType;
        // public float lineLength;
        public GameObject lineObj;
    }

    public enum LineType
    {
        Horizontal,
        Vertical
    }

    public List<Line2DData> all2DLineDatas = new List<Line2DData>();

    #endregion

    void Start()
    {
        // 自动获取主相机，无需手动拖拽赋值
        if (mainCamera == null) mainCamera = Camera.main;
        // 创建画线预览线和删除轨迹预览线
        CreatePreviewLine();
        CreateDeletePreviewLine();

        // init data
        var saveLines = GhostLocalSave.Instance.GetRoadLines();
        if (saveLines != null)
        {
            foreach (var line in saveLines)
            {
                Create2DFinalLine(line.start.ToVector2(), line.end.ToVector2());
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            List<LineSave> lines = new();
            foreach (var line in all2DLineDatas)
            {
                lines.Add(new LineSave()
                {
                    start = line.startPos.ToArray(),
                    end = line.endPos.ToArray(),
                });
            }

            GhostLocalSave.Instance.SaveRoadLines(lines);

            Tips.Instance.Pop("saved").Forget();
            return;
        }

        #region ★优先级最高：相机平移 (两种方式：Ctrl+左键 、 滚轮中键按下拖动)★

        if ((Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0)) || Input.GetMouseButton(2))
        {
            CameraMove();
            return; // 相机移动时，屏蔽所有其他功能，无任何误触
        }

        #endregion

        #region ★滚轮缩放相机视野 (无冲突，随时可用)

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            CameraZoom();
        }

        #endregion

        #region ★优先级次之：D键+拖动 删除线条

        if (Input.GetKey(KeyCode.D))
        {
            isDeleteLine = true;
            DeleteLineByMouseDrag();
        }
        else
        {
            if (isDeleteLine)
            {
                isDeleteLine = false;
                deletePreviewLine.gameObject.SetActive(false);
            }
        }

        #endregion

        #region ★基础功能：鼠标绘制纯横竖线 (删除模式下屏蔽)

        if (isDeleteLine) return;

        if (Input.GetMouseButtonDown(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics2D.Raycast(ray.origin, ray.direction))
            {
                isDrawing = true;
                startWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                // startWorldPos.z = 0; // 2D场景强制Z轴为0，防止坐标偏移
                previewLine.SetPosition(0, startWorldPos);
                previewLine.gameObject.SetActive(true);
            }
        }

        if (isDrawing && Input.GetMouseButton(0))
        {
            endWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // endWorldPos.z = 0;
            Update2DPreviewLine(startWorldPos, endWorldPos);
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            previewLine.gameObject.SetActive(false);
            if (Vector2.Distance(startWorldPos, endWorldPos) > minLineLength)
            {
                Create2DFinalLine(startWorldPos, endWorldPos);
            }
        }

        #endregion
    }

    #region ========== 画线核心方法 ==========

    private void CreatePreviewLine()
    {
        GameObject previewObj = new GameObject("2D_Preview_Line");
        previewObj.transform.SetParent(this.transform);
        previewLine = previewObj.AddComponent<LineRenderer>();
        previewLine.material = new Material(Shader.Find("Sprites/Default"));
        previewLine.startColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0.5f);
        previewLine.endColor = previewLine.startColor;
        previewLine.startWidth = lineWidth;
        previewLine.endWidth = lineWidth;
        previewLine.positionCount = 2;
        previewLine.loop = false;
        previewLine.gameObject.SetActive(false);
    }

    // 核心逻辑：只绘制纯水平横线/垂直竖线，无任何斜线
    private void Update2DPreviewLine(Vector2 start, Vector2 end)
    {
        float offsetX = Mathf.Abs(end.x - start.x);
        float offsetY = Mathf.Abs(end.y - start.y);
        Vector2 realEndPos = offsetX > offsetY ? new Vector2(end.x, start.y) : new Vector2(start.x, end.y);
        previewLine.SetPosition(1, realEndPos);
    }

    // 生成最终线段并存储数据
    private void Create2DFinalLine(Vector2 start, Vector2 end)
    {
        float offsetX = Mathf.Abs(end.x - start.x);
        float offsetY = Mathf.Abs(end.y - start.y);
        Vector2 realEndPos = Vector2.zero;
        LineType currLineType = LineType.Horizontal;

        if (offsetX > offsetY)
        {
            realEndPos = new Vector2(end.x, start.y);
            currLineType = LineType.Horizontal;
        }
        else
        {
            realEndPos = new Vector2(start.x, end.y);
            currLineType = LineType.Vertical;
        }

        var lineObj = DrawNewLine(start, realEndPos);
        Line2DData lineData = new Line2DData();
        lineData.startPos = start;
        lineData.endPos = realEndPos;
        // lineData.lineType = currLineType;
        // lineData.lineLength = Vector2.Distance(start, realEndPos);
        var lineLength = Vector2.Distance(start, realEndPos);
        lineData.lineObj = lineObj;

        all2DLineDatas.Add(lineData);
        Debug.Log($"绘制【{currLineType}】线段 | 长度：{lineLength:F2} | 总线段数：{all2DLineDatas.Count}");
    }

    private int _lineId;

    private GameObject DrawNewLine(Vector2 start, Vector2 realEndPos)
    {
        GameObject lineObj = new GameObject($"2D_Line_{_lineId++}");
        lineObj.transform.SetParent(this.transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, realEndPos);

        return lineObj;
    }

    #endregion

    #region ========== 相机控制核心方法 (平移+缩放) ==========

    /// <summary>
    /// 相机平移核心方法
    /// 触发方式：① Ctrl + 鼠标左键拖动  ② 按下鼠标滚轮(中键) + 拖动
    /// </summary>
    private void CameraMove()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
        {
            lastMouseScreenPos = Input.mousePosition;
            isMoveCamera = true;
            return;
        }

        if (isMoveCamera && (Input.GetMouseButton(0) || Input.GetMouseButton(2)))
        {
            // 计算鼠标移动偏移量
            Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMouseScreenPos;
            // 反向平移：鼠标拖动方向 = 相机移动方向，符合直觉
            Vector3 cameraDelta = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * cameraMoveSpeed * Time.deltaTime;
            // 适配相机缩放：视野越大，平移越快，操作手感统一
            cameraDelta *= mainCamera.orthographicSize / 5f;

            mainCamera.transform.Translate(cameraDelta);
            lastMouseScreenPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(2))
        {
            isMoveCamera = false;
        }
    }

    /// <summary>
    /// 鼠标滚轮缩放相机视野
    /// 滚轮向上 → 视野缩小(放大场景)  滚轮向下 → 视野扩大(缩小场景)
    /// </summary>
    private void CameraZoom()
    {
        float wheelValue = Input.GetAxis("Mouse ScrollWheel");
        float newCameraSize = mainCamera.orthographicSize - wheelValue * cameraZoomSpeed;
        // 限制缩放范围，防止无限缩放导致场景丢失
        mainCamera.orthographicSize = Mathf.Clamp(newCameraSize, minCameraSize, maxCameraSize);
    }

    #endregion

    #region ========== 删除线条核心方法 ==========

    private void CreateDeletePreviewLine()
    {
        GameObject deleteObj = new GameObject("2D_Delete_Preview_Line");
        deleteObj.transform.SetParent(this.transform);
        deletePreviewLine = deleteObj.AddComponent<LineRenderer>();
        deletePreviewLine.material = new Material(Shader.Find("Sprites/Default"));
        deletePreviewLine.startColor = deleteLineHintColor;
        deletePreviewLine.endColor = deleteLineHintColor;
        deletePreviewLine.startWidth = lineWidth * 1.5f;
        deletePreviewLine.endWidth = lineWidth * 1.5f;
        deletePreviewLine.positionCount = 2;
        deletePreviewLine.loop = false;
        deletePreviewLine.gameObject.SetActive(false);
    }

    private void DeleteLineByMouseDrag()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        // mouseWorldPos.z = 0;

        deletePreviewLine.gameObject.SetActive(true);
        deletePreviewLine.SetPosition(0, mouseWorldPos);
        deletePreviewLine.SetPosition(1, mouseWorldPos);

        // 反向遍历，防止删除元素导致索引错乱
        for (int i = all2DLineDatas.Count - 1; i >= 0; i--)
        {
            Line2DData lineData = all2DLineDatas[i];
            if (lineData.lineObj == null) continue;

            if (IsPointInLineRange(mouseWorldPos, lineData.startPos, lineData.endPos, lineCheckRadius))
            {
                Destroy(lineData.lineObj);
                all2DLineDatas.RemoveAt(i);
                Debug.Log($"删除线段，剩余线段数：{all2DLineDatas.Count}");
            }
        }
    }

    // 精准检测：鼠标点是否在线段的检测半径范围内
    private bool IsPointInLineRange(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float checkRadius)
    {
        float closestPointX =
            Mathf.Clamp(point.x, Mathf.Min(lineStart.x, lineEnd.x), Mathf.Max(lineStart.x, lineEnd.x));
        float closestPointY =
            Mathf.Clamp(point.y, Mathf.Min(lineStart.y, lineEnd.y), Mathf.Max(lineStart.y, lineEnd.y));
        Vector2 closestPoint = new Vector2(closestPointX, closestPointY);
        float distance = Vector2.Distance(point, closestPoint);
        return distance <= checkRadius;
    }

    #endregion

    #region ========== 线段数据操作工具方法 ==========

    // 清空所有线段和数据
    public void ClearAll2DLines()
    {
        foreach (var lineData in all2DLineDatas) Destroy(lineData.lineObj);
        all2DLineDatas.Clear();
        Debug.Log("已清空所有2D线段和数据");
    }

    // 获取所有水平横线
    // public List<Line2DData> GetAllHorizontalLines()
    // {
    //     return all2DLineDatas.FindAll(data => data.lineType == LineType.Horizontal);
    // }
    //
    // // 获取所有垂直竖线
    // public List<Line2DData> GetAllVerticalLines()
    // {
    //     return all2DLineDatas.FindAll(data => data.lineType == LineType.Vertical);
    // }

    // 根据下标删除指定线段
    public void DeleteLineByIndex(int index)
    {
        if (index >= 0 && index < all2DLineDatas.Count)
        {
            Destroy(all2DLineDatas[index].lineObj);
            all2DLineDatas.RemoveAt(index);
        }
    }

    #endregion
}