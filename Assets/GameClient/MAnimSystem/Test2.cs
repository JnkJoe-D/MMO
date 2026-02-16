using System.Collections;
using UnityEngine;
namespace Game.MAnimSystem
{
public class Test2 : MonoBehaviour
{
    public AnimComponent animComponent;
    [Header("动画片段 (Clip)")]
    public AnimationClip clip1;
    public AnimationClip clip2;
    public AnimationClip clip3;
    public AnimationClip clip4;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animComponent.Play(clip1, 0.2f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animComponent.Play(clip2, 0.2f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animComponent.Play(clip3, 0.2f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            animComponent.Play(clip4, 0.2f);
        }
    }
}
}