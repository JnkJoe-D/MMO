using System.Collections;
using System.Collections.Generic;
using Game.FSM;
using UnityEngine;
namespace Game.Logic.Player{
public class Test_Player : MonoBehaviour
{
    private FSMManager _fsmManager;
    private Game.Input.InputManager _inputManager;
    private Game.Camera.GameCameraManager _cameraManager;
    // Start is called before the first frame update
    void Awake()
    {
        _fsmManager = gameObject.GetComponent<FSMManager>();
        if (_fsmManager == null)
        {
            _fsmManager = gameObject.AddComponent<FSMManager>();
        }
        _fsmManager.Initialize();
        // ── Step 9: 输入管理器 ────────────────────
        _inputManager = new Game.Input.InputManager();
        _inputManager.Initialize();
        Debug.Log("[GameRoot] [9/11] Input ... OK");

        // ── Step 10: 相机管理器 ───────────────────
        _cameraManager = new Game.Camera.GameCameraManager();
        _cameraManager.Initialize();
        Debug.Log("[GameRoot] [10/11] Camera ... OK");
        // ── Step 11: 全局动画库 ───────────────────
            var animConfigManager = new Game.Logic.Player.Config.AnimationConfigManager();
            animConfigManager.Initialize();
            Debug.Log("[GameRoot] [11/11] Animation Configs ... OK");
    }
}
}