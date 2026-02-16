using System.Collections.Generic;
using System;
using XLua;
using System.Reflection;

//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class XLuaGenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。

    //如果你要生成Lua调用CSharp的代码，加这个标签
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(object),//语言类
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(Action),
        typeof(Action<string>),
        typeof(UnityEngine.Object),//引擎类
        typeof(UnityEngine.Vector2),
        typeof(UnityEngine.Vector3),
        typeof(UnityEngine.Vector4),
        typeof(UnityEngine.Quaternion),
        typeof(UnityEngine.Color),
        typeof(UnityEngine.Ray),
        typeof(UnityEngine.Bounds),
        typeof(UnityEngine.Ray2D),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.GameObject),
        typeof(UnityEngine.Component),
        typeof(UnityEngine.Behaviour),
        typeof(UnityEngine.Transform),
        typeof(UnityEngine.RectTransform),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.TextAsset),
        typeof(UnityEngine.Keyframe),
        typeof(UnityEngine.AnimationCurve),
        typeof(UnityEngine.AnimationClip),
        typeof(UnityEngine.MonoBehaviour),
        typeof(UnityEngine.ParticleSystem),
        typeof(UnityEngine.SkinnedMeshRenderer),
        typeof(UnityEngine.Renderer),
        typeof(UnityEngine.Networking.UnityWebRequest),
        typeof(UnityEngine.Light),
        typeof(UnityEngine.Mathf),
        typeof(UnityEngine.Debug),
        typeof(UnityEngine.WaitForSeconds),
        typeof(UnityEngine.WaitForEndOfFrame),
        typeof(UnityEngine.WaitForSecondsRealtime),
        typeof(UnityEngine.UI.Button),//UGUI
        typeof(UnityEngine.UI.InputField),//UGUI
        typeof(UnityEngine.UI.Text),//UGUI
        typeof(UnityEngine.UI.Image),//UGUI
        typeof(UnityEngine.UI.RawImage),//UGUI
        typeof(UnityEngine.UI.Scrollbar),//UGUI
        typeof(UnityEngine.UI.ScrollRect),//UGUI
        typeof(UnityEngine.UI.ScrollRect.ScrollRectEvent),
        typeof(UnityEngine.UI.Toggle),//UGUI
        typeof(UnityEngine.UI.ToggleGroup),//UGUI
        typeof(UnityEngine.UI.Dropdown),//UGUI
        typeof(UnityEngine.Texture),
        typeof(UnityEngine.Texture2D),
        typeof(UnityTools),
        // typeof(CarTools),
        typeof(IList<int>),//data struct
        typeof(IList<string>),//data struct
        typeof(IList<float>),//data struct
        typeof(IList<long>),//data struct
        typeof(BItemData),//data struct
        typeof(IList<BItemData>),//data struct                
        typeof(BTypeItemData),//data struct
        typeof(IList<BTypeItemData>),//data struct
        typeof(UiConfig),//UI
        typeof(UiConfigManger),//UI
        typeof(UiView),//UI
        typeof(UiLayer),//UI
        typeof(ViewId),//UI        
        typeof(SuperScrollView.LoopListView2),//SuperScrollView
        typeof(SuperScrollView.LoopListViewItem2),//SuperScrollView
        typeof(SuperScrollView.LoopGridView),//SuperScrollView
        typeof(SuperScrollView.LoopGridViewItem),//SuperScrollView
        typeof(SuperScrollView.LoopStaggeredGridView),//SuperScrollView
        typeof(SuperScrollView.LoopStaggeredGridViewItem),//SuperScrollView
        // typeof(GameModuleManager),
        typeof(System.IO.File),
        // typeof(King),
        // typeof(KingAssetNode),
        // typeof(MessageHandler),
        // typeof(MessageSender),
        typeof(UnityEngine.EventSystems.PointerEventData),
    };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(Action),
        typeof(Action<int>),
        typeof(Action<string>),
        typeof(Action<bool>),
        typeof(Action<float>),
        typeof(Action<double>),
        typeof(Action<int, byte[]>),
        typeof(Action<int,bool>),
        typeof(Action<int,int,long>),
        typeof(Action<UnityEngine.Vector2>),
        typeof(UnityEngine.Events.UnityAction),
        typeof(UnityEngine.Events.UnityAction<bool>),
        typeof(UnityEngine.Events.UnityAction<float>),
        typeof(UnityEngine.Events.UnityAction<int>),
        typeof(UnityEngine.Events.UnityAction<UnityEngine.Vector2>),
        typeof(System.Collections.IEnumerator),
        typeof(Func<SuperScrollView.LoopListView2, int, SuperScrollView.LoopListViewItem2>),
        typeof(Func<SuperScrollView.LoopGridView, int, int, int, SuperScrollView.LoopGridViewItem>),
        typeof(Func<SuperScrollView.LoopStaggeredGridView, int, SuperScrollView.LoopStaggeredGridViewItem>),
        typeof(BItemData),//data struct
        typeof(IList<BItemData>),//data struct        
        typeof(BTypeItemData),//data struct
        typeof(IList<BItemData>),//data struct
    };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()
    {
        new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
        new List<string>(){"UnityEngine.WWW", "movie"},
#if UNITY_WEBGL
        new List<string>(){"UnityEngine.WWW", "threadPriority"},
#endif
        new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
        new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
        new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
        new List<string>(){"UnityEngine.Light", "areaSize"},
        new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
        new List<string>(){"UnityEngine.WWW", "MovieTexture"},
        new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
        new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
#if !UNITY_WEBPLAYER
        new List<string>(){"UnityEngine.Application", "ExternalEval"},
#endif
        new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
        new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
        new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
        new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
        new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
        new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
        new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
        new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
        new List<string>(){"UnityEngine.Light", "shadowRadius"},
        new List<string>(){"UnityEngine.Light", "SetLightDirty"},
        new List<string>(){"UnityEngine.Light", "shadowAngle"},
        new List<string>(){"UnityEngine.Light", "shadowAngle"},
        new List<string>(){ "UnityEngine.UI.Text", "OnRebuildRequested"},
        new List<string>(){ "UnityEngine.Texture", "imageContentsHash" }
    };

#if UNITY_2018_1_OR_NEWER
    [BlackList]
    public static Func<MemberInfo, bool> MethodFilter = (memberInfo) =>
    {
        if (memberInfo.DeclaringType.IsGenericType && memberInfo.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            if (memberInfo.MemberType == MemberTypes.Constructor)
            {
                ConstructorInfo constructorInfo = memberInfo as ConstructorInfo;
                var parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length > 0)
                {
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(parameterInfos[0].ParameterType))
                    {
                        return true;
                    }
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Method)
            {
                var methodInfo = memberInfo as MethodInfo;
                if (methodInfo.Name == "TryAdd" || methodInfo.Name == "Remove" && methodInfo.GetParameters().Length == 2)
                {
                    return true;
                }
            }
        }
        return false;
    };
#endif
}