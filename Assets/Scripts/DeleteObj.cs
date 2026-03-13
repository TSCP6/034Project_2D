using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 引用UI命名空间

public class DeleteObj : MonoBehaviour
{
    [Header("Deletion Settings")]
    public LayerMask deletableLayer; // 在Inspector中设置可删除的层

    [Header("UI Progress")]
    public Image progressBarFill; // 拖入你的进度条填充Image

    private List<GameObject> deletableObjects = new List<GameObject>();
    private int totalObjectCount;
    private int deletedObjectCount;

    void Start()
    {
        // 找到场景中所有属于可删除层的物体
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            // 检查物体的层是否在deletableLayer中
            if (((1 << obj.layer) & deletableLayer) != 0)
            {
                deletableObjects.Add(obj);
            }
        }
        totalObjectCount = deletableObjects.Count;
        deletedObjectCount = 0;

        // 初始化进度条
        UpdateProgressBar();
    }

    void Update()
    {
        // 检测鼠标左键点击，并确保没有点击到UI
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // 从鼠标位置发射一条射线
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, deletableLayer);

            // 如果射线击中了物体
            if (hit.collider != null)
            {
                GameObject objectToDelete = hit.collider.gameObject;
                // 检查这个物体是否在我们初始化的列表中
                if (deletableObjects.Contains(objectToDelete))
                {
                    DeleteObjectAndConnectedLinks(objectToDelete);

                    // 从列表中移除并更新计数
                    deletableObjects.Remove(objectToDelete);
                    deletedObjectCount++;

                    // 更新UI
                    UpdateProgressBar();

                    // 检查是否通关
                    if (deletableObjects.Count == 0)
                    {
                        LevelComplete();
                    }
                }
            }
        }
    }

    private void DeleteObjectAndConnectedLinks(GameObject target)
    {
        if (target == null) return;

        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null)
        {
            Destroy(target);
            return;
        }

        HingeJoint2D[] allHinges = FindObjectsOfType<HingeJoint2D>();
        List<GameObject> linksToDelete = new List<GameObject>();

        foreach (var hinge in allHinges)
        {
            if (hinge.connectedBody == targetRb)
            {
                if (!linksToDelete.Contains(hinge.gameObject))
                {
                    linksToDelete.Add(hinge.gameObject);
                }
            }
        }

        foreach (var link in linksToDelete)
        {
            Destroy(link);
        }

        Destroy(target);
    }

    private void UpdateProgressBar()
    {
        if (progressBarFill != null)
        {
            if (totalObjectCount > 0)
            {
                // 进度应该是已删除的数量 / 总数
                progressBarFill.fillAmount = (float)deletedObjectCount / totalObjectCount;
            }
            else
            {
                // 如果没有可删除的物体，进度条直接填满
                progressBarFill.fillAmount = 1f;
            }
        }
    }

    private void LevelComplete()
    {
        // 在这里编写通关逻辑
        Debug.Log("关卡完成！所有目标已清除。");
        // 例如：加载下一个场景、显示胜利UI等
        // SceneManager.LoadScene("NextLevel");
    }
}
