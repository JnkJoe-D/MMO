using UnityEngine;
namespace Game.Logic.Player
{
[RequireComponent(typeof(PlayerEntity))]
public class MovementController : MonoBehaviour, IMovementController
{
    public void Move(Vector3 velocity)
    {
        // 无碰撞的宇宙真空中强制位移
        transform.position += velocity;
    }

    public void FaceTo(Vector3 lookDirection)
    {
        if (lookDirection.sqrMagnitude > 0.001f)
        {
                // 瞬间转身（正式版应插值 Slerp）
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
}