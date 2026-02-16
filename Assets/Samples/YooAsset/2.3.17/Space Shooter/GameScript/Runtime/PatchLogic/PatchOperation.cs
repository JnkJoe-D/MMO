using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;
using YooAsset;

public class PatchOperation : GameAsyncOperation
{
    private enum ESteps
    {
        None,
        Update,
        Done,
    }

    private readonly EventGroup _eventGroup = new EventGroup();  //用事件组批量管理事件监听
    private readonly StateMachine _machine; // 状态机
    private readonly string _packageName;   //包名
    private ESteps _steps = ESteps.None;  // operation内部当前步骤

    public PatchOperation(string packageName, EPlayMode playMode)
    {
        _packageName = packageName;

        // 注册监听事件
        _eventGroup.AddListener<UserEventDefine.UserTryInitialize>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserBeginDownloadWebFiles>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryRequestPackageVersion>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryUpdatePackageManifest>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryDownloadWebFiles>(OnHandleEventMessage);

        // 创建状态机
        _machine = new StateMachine(this);
        _machine.AddNode<FsmInitializePackage>();
        _machine.AddNode<FsmRequestPackageVersion>();
        _machine.AddNode<FsmUpdatePackageManifest>();
        _machine.AddNode<FsmCreateDownloader>();
        _machine.AddNode<FsmDownloadPackageFiles>();
        _machine.AddNode<FsmDownloadPackageOver>();
        _machine.AddNode<FsmClearCacheBundle>();
        _machine.AddNode<FsmStartGame>();

        _machine.SetBlackboardValue("PackageName", packageName);
        _machine.SetBlackboardValue("PlayMode", playMode);
    }
    /// <summary>
    /// 操作开始时调用
    /// </summary>
    protected override void OnStart()
    {
        _steps = ESteps.Update;
        _machine.Run<FsmInitializePackage>();
    }
    /// <summary>
    /// 操作Start后，框架每帧调用，直到Status不等于EOperationStatus.Processing
    /// </summary>
    protected override void OnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.Update)
        {
            _machine.Update();
        }
    }
    /// <summary>
    /// 操作被中止时调用
    /// </summary>
    protected override void OnAbort()
    {
    }
    /// <summary>
    /// 设置操作完成
    /// </summary>
    public void SetFinish()
    {
        _steps = ESteps.Done;
        _eventGroup.RemoveAllListener();
        Status = EOperationStatus.Succeed;
        Debug.Log($"Package {_packageName} patch done !");
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is UserEventDefine.UserTryInitialize)
        {
            _machine.ChangeState<FsmInitializePackage>();
        }
        else if (message is UserEventDefine.UserBeginDownloadWebFiles)
        {
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
        else if (message is UserEventDefine.UserTryRequestPackageVersion)
        {
            _machine.ChangeState<FsmRequestPackageVersion>();
        }
        else if (message is UserEventDefine.UserTryUpdatePackageManifest)
        {
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
        else if (message is UserEventDefine.UserTryDownloadWebFiles)
        {
            _machine.ChangeState<FsmCreateDownloader>();
        }
        else
        {
            throw new System.NotImplementedException($"{message.GetType()}");
        }
    }
}