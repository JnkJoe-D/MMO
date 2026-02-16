using UnityEngine;

public class UiView : MonoBehaviour
{
    public Canvas canvas;
    public GameObject viewGameObject;

    private UiConfig mUIConfig;

    public int SelfViewId
    {
        get
        {
            if (mUIConfig != null)
            {
                return mUIConfig.viewId;
            }

            return 0;
        }
    }

    public int LayerType
    {
        get
        {
            if (mUIConfig != null)
            {
                return mUIConfig.layerType;
            }

            return UiConfigManger.None;
        }
    }

    public int SortingOrder
    {
        get
        {
            if (canvas != null)
            {
                return canvas.sortingOrder;
            }

            return 0;
        }
        set
        {
            if (canvas != null)
            {
                canvas.sortingOrder = value;
            }
        }
    }

    public bool IsLarge
    {
        get
        {
            if (mUIConfig != null)
            {
                return mUIConfig.isLarge;
            }

            return false;
        }
    }

    protected RectTransform mCanvasRectTransform;
    protected RectTransform mViewRectTransform;

    protected void InnerInitialize()
    {
        if (mCanvasRectTransform == null && canvas != null)
        {
            mCanvasRectTransform = canvas.GetComponent<RectTransform>();
        }

        if (mViewRectTransform == null && viewGameObject != null)
        {
            mViewRectTransform = viewGameObject.GetComponent<RectTransform>();
        }
    }

    public virtual void Awake()
    {
        InnerInitialize();

        UpdateBangHeight();

        //Debug.Log("Screen.width: " + Screen.width);
        //Debug.Log("Screen.height: " + Screen.height);
        //Debug.Log("Screen.safeArea.width: " + Screen.safeArea.width);
        //Debug.Log("Screen.safeArea.height: " + Screen.safeArea.height);
        //Debug.Log("mCanvasRectTransform.rect.width: " + mCanvasRectTransform.rect.width);
        //Debug.Log("mCanvasRectTransform.rect.height: " + mCanvasRectTransform.rect.height);
    }

    public void SetUIConfig(UiConfig uiConfig)
    {
        mUIConfig = uiConfig;
    }

    public virtual void OnLoad()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        if (mUIConfig != null)
        {
            canvas.sortingLayerName = "Ui";
            canvas.sortingOrder = mUIConfig.layerType;
        }
    }

    public virtual void OnShow()
    {

    }

    public virtual void OnHide()
    {

    }

    public virtual void OnRemove()
    {

    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);

        if (active)
        {
            OnShow();
        }
        else
        {
            OnHide();
        }
    }

    public void SetViewActive(bool active)
    {
        if (viewGameObject != null)
        {
            viewGameObject.SetActive(active);
        }
    }

    public int GetViewId()
    {
        if (mUIConfig != null)
        {
            return mUIConfig.viewId;
        }

        return 0;
    }

    public void RemoveSelf()
    {
        GameUi.Instance.RemoveView(this);
    }

    public void UpdateBangHeight()
    {
        if (mViewRectTransform != null)
        {
            int bangHeight = GetBangHeight();
            mViewRectTransform.offsetMax -= new Vector2(0, bangHeight);
        }
    }

    public int GetBangHeight()
    {
#if UNITY_IOS

        //if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone11)
        //{
        //    return 90;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone12)
        //{
        //    return 90;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone12Pro)
        //{
        //    return 90;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone12ProMax)
        //{
        //    return 85;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone12Mini)
        //{
        //    return 100;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX)
        //{
        //    return 90;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneXR)
        //{
        //    return 90;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneXS)
        //{
        //    return 90;
        //}
        //else if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneXSMax)
        //{
        //    return 80;
        //}

#elif UNITY_ANDROID

        if (!string.IsNullOrEmpty(SystemInfo.deviceModel))//HuaWeiP20
        {
            if (SystemInfo.deviceModel.Contains("HUAWEI EML-L29"))
            {
                return 80;
            }
        }

#endif

        if (mCanvasRectTransform != null && mViewRectTransform != null)
        {
            int bangHeight = Screen.height - (int)Screen.safeArea.height;

            return bangHeight;
        }

        return 0;
    }

}