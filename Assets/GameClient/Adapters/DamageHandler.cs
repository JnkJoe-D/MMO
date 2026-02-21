using System.Collections;
using System.Collections.Generic;
using SkillEditor;
using UnityEngine;
namespace Game.Adapters{
public class DamageHandler : MonoBehaviour,ISkillDamageHandler
{
	public void OnDamageDetect(DamageData damageData)
	{
        var colliders = damageData.targets;

        foreach (var c in colliders)
        {
            Debug.Log($"{c.gameObject.name}:take damage");
        }
	}
}
}