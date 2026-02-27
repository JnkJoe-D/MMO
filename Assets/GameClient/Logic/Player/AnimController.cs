using Game.MAnimSystem;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Game.Logic.Player
{
[RequireComponent(typeof(PlayerEntity))]
public class AnimController:MonoBehaviour, IAnimController
    {
    AnimComponent _animComponent;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _animComponent = gameObject.GetComponent<AnimComponent>();
        if(_animComponent==null)
            {
                _animComponent= gameObject.AddComponent<AnimComponent>();
            }
    }
    
    private string _lastAnim = "";
        public void PlayAnim(AnimationClip clip, float fadeDuration = 0.2f, System.Action onFadeComplete = null, System.Action onAnimEnd = null)
        {
            if (clip != null)
            {
                // 如果是同一个动画要求重复播，且目前处于播放中，可以直接 return (按需要这里也可强制重播)
                if (_lastAnim == clip.name) return;

                _lastAnim = clip.name;
                AnimState state = _animComponent.Play(clip, fadeDuration);

                // 闭包适配器：隔离 MAnimSystem 污染
                if (state != null)
                {
                    if (onFadeComplete != null)
                    {
                        state.OnFadeComplete += (s) => onFadeComplete.Invoke();
                    }
                    if (onAnimEnd != null)
                    {
                        state.OnEnd += (s) => onAnimEnd.Invoke();
                    }
                }
                
                Debug.Log($"[动画测试桩] 角色动画已切换为 ---> {clip.name}");
            }
        }
    }
}