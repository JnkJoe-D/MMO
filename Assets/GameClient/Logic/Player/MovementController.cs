using UnityEngine;
namespace Game.Logic.Player
{
[RequireComponent(typeof(PlayerEntity))]
public class MovementController : MonoBehaviour, IMovementController
{
    Rigidbody _rb;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        if(_rb==null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 冻结由物理碰撞产生的意外旋转，保证角色的直立姿态由代码百分百掌控
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // 开启物理位置的渲染帧插值，解决由于渲染帧率>物理帧率造成的相机或本体抽搐抖动
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    public void Move(Vector3 velocity)
    {
        // 刚体位移
        _rb?.MovePosition(transform.position + velocity);
    }

    public float TurnSpeed = 15f; // 转身平滑度，数值越大越快

    public void FaceTo(Vector3 lookDirection)
    {
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            // 利用受帧率保护的球面线性插值实现自然转身
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);
        }
    }
}
}