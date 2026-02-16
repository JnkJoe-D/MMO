using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnityTools
{
    public static GameObject AddChild(GameObject parent, GameObject assetGameObject)
    {
        if (parent != null && assetGameObject != null)
        {
            GameObject prefabGameObject = Object.Instantiate(assetGameObject);
            if (prefabGameObject != null)
            {
                Transform prefabTransform = prefabGameObject.transform;
                prefabTransform.SetParent(parent.transform);
                prefabTransform.localPosition = Vector3.zero;
                prefabTransform.localRotation = Quaternion.identity;
                prefabTransform.localScale = Vector3.one;
                prefabGameObject.layer = parent.layer;
            }

            return prefabGameObject;
        }

        return null;
    }

    public static void SetParent(GameObject parentGameObject, GameObject childGameObject, bool resetTransform = true)
    {
        if (parentGameObject != null && childGameObject != null)
        {
            Transform childTransform = childGameObject.transform;
            childTransform.SetParent(parentGameObject.transform);

            if (resetTransform)
            {
                childTransform.localPosition = Vector3.zero;
                childTransform.localRotation = Quaternion.identity;
                childTransform.localScale = Vector3.one;
                childGameObject.layer = childGameObject.layer;
            }
        }
    }

    public static Transform ResetTransform(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            targetTransform.localPosition = Vector3.zero;
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.localScale = Vector3.one;
        }

        return targetTransform;
    }

    public static RectTransform ResetRectTransform(RectTransform rectTransform)
    {
        if (rectTransform != null)
        {
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        return rectTransform;
    }

    public static void LookAtDirection(Transform targetTransform, Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            targetTransform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public static void PrintArray(object[] messageObjectArray)
    {
        if (messageObjectArray != null)
        {
            int messageObjectCount = messageObjectArray.Length;
            for (int i = 0; i < messageObjectCount; ++i)
            {
                if (messageObjectArray[i] != null)
                {
                    Debug.Log(messageObjectArray[i]);
                }
            }
        }
    }

    public static void PrintList(List<string> messageList)
    {
        if (messageList != null)
        {
            int messageObjectCount = messageList.Count;
            for (int i = 0; i < messageObjectCount; ++i)
            {
                if (messageList[i] != null)
                {
                    Debug.Log(messageList[i]);
                }
            }
        }
    }

    public static string GetPersistentRelativePath(string pathFilename)
    {
        string persistentPathFilename = Path.Combine(Application.persistentDataPath, pathFilename);

        return persistentPathFilename;
    }

    //参数为相对于StreamingAssets的目录文件,路径开头不需要加/
    public static string GetStreamingAssetsUri(string pathFilename)
    {
        System.Uri uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, pathFilename));

        return uri.AbsoluteUri;
    }

    public static string GetStreamingAssetsPath(string pathFilename)
    {
        return Path.Combine(Application.streamingAssetsPath, pathFilename);
    }

    public static byte[] LoadPersistentFileBytes(string pathFilename)
    {
        string persistentPathFilename = GetPersistentRelativePath(pathFilename);

        if (File.Exists(persistentPathFilename))
        {
            byte[] bytes = File.ReadAllBytes(persistentPathFilename);

            return bytes;
        }

        return null;
    }

    //计算概率
    public static bool IsInRate(float rate)
    {
        float randomValue = Random.Range(0.0f, 1.0f);
        if (randomValue <= rate)
        {
            return true;
        }

        return false;
    }

    public static Canvas GetParentRootCanvas(GameObject startGameObject)
    {
        if (startGameObject != null)
        {
            Canvas[] canvasArray = startGameObject.GetComponentsInParent<Canvas>();
            if (canvasArray != null)
            {
                for (int i = 0; i < canvasArray.Length; i++)
                {
                    if (canvasArray[i] != null && canvasArray[i].isRootCanvas)
                    {
                        return canvasArray[i];
                    }
                }
            }
        }

        return null;
    }
}
