using SkillEditor;
using UnityEngine;
namespace Game.Adapters
{
public class DamageHandler : ISkillDamageHandler
{
	public void OnDamageDetect(DamageData damageData)
	{
        var colliders = damageData.targets;

        foreach (var c in colliders)
        {
            Debug.Log($"{c.gameObject.name}:<color=orange>Damage Triggered!</color>");
        }
	}
}
}