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
        public void PlayAnim(AnimationClip clip, float fadeDuration = 0.2f)
        {
            if (clip != null && _lastAnim != clip.name)
            {
                _lastAnim = clip.name;
                _animComponent.Play(clip, fadeDuration);
                Debug.Log($"[动画测试桩] 角色动画已切换为 ---> {clip.name}");
            }
        }
}
}