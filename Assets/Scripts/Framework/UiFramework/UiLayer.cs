using System.Collections.Generic;
using UnityEngine;

public class UiLayer
{
    public int layerType;

    private List<UiView> mViewList = new List<UiView>();

    public void AddView(UiView uiView)
    {
        if (uiView != null)
        {
            if (!mViewList.Contains(uiView))
            {
                mViewList.Add(uiView);

                RearrangeSortingOrder();
            }
        }
    }

    public UiView GetView(int viewId)
    {
        int uiViewCount = mViewList.Count;
        for (int i = 0; i < uiViewCount; ++i)
        {
            UiView uiView = mViewList[i];
            if (uiView != null && uiView.GetViewId() == viewId)
            {
                return uiView;
            }
        }

        return null;
    }

    public int GetViewCount()
    {
        int viewCount = 0;
        for (int i = 0; i < mViewList.Count; i++)
        {
            if (mViewList[i] != null && mViewList[i].gameObject.activeSelf)
            {
                ++viewCount;
            }
        }

        return viewCount;
    }

    public void RemoveView(int viewId)
    {
        UiView uiView = GetView(viewId);
        RemoveView(uiView);
    }

    public void RemoveView(UiView uiView)
    {
        int uiCount = mViewList.Count;
        for (int i = uiCount - 1; i >= 0; --i)
        {
            if (mViewList[i] != null && mViewList[i] == uiView)
            {
                mViewList[i].OnRemove();
                Object.Destroy(mViewList[i].gameObject);
                mViewList.RemoveAt(i);

                RearrangeSortingOrder();

                break;
            }
        }
    }

    public bool HasView(int viewId)
    {
        for (int i = 0; i < mViewList.Count; i++)
        {
            if (mViewList[i] != null && mViewList[i].SelfViewId == viewId)
            {
                return true;
            }
        }

        return false;
    }

    public void SetLayerActive(bool active)
    {
        for (int i = 0; i < mViewList.Count; ++i)
        {
            if (mViewList[i] != null)
            {
                mViewList[i].SetActive(active);
            }
        }
    }

    public void ClearLayer()
    {
        for (int i = mViewList.Count - 1; i >= 0; --i)
        {
            RemoveView(mViewList[i]);
        }
    }

    //隐藏大ui,删除小ui
    public void OptimizeCloseLayer()
    {
        for (int i = mViewList.Count - 1; i >= 0; --i)
        {
            if (mViewList[i].IsLarge)
            {
                mViewList[i].SetActive(false);
            }
            else
            {
                RemoveView(mViewList[i]);
            }
        }
    }

    public void RearrangeSortingOrder()
    {
        for (int i = 0; i < mViewList.Count; ++i)
        {
            if (mViewList[i] != null)
            {
                int layerType = mViewList[i].LayerType;
                mViewList[i].SortingOrder = layerType + i * 10;
            }
        }
    }
}
