using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor
{
    /// <summary>
    /// 运行时简单的 VFX 对象池管理器
    /// </summary>
    public static class VFXPoolManager
    {
        // Key: Prefab InstanceID, Value: Stack of inactive instances
        private static Dictionary<int, Stack<GameObject>> pools = new Dictionary<int, Stack<GameObject>>();
        
        // Key: Instance, Value: Prefab InstanceID (for return lookup)
        private static Dictionary<GameObject, int> activeInstances = new Dictionary<GameObject, int>();
        
        private static Transform poolRoot;

        private static void EnsureRoot()
        {
            if (poolRoot == null)
            {
                GameObject rootObj = new GameObject("VFX_Runtime_Pool");
                Object.DontDestroyOnLoad(rootObj);
                poolRoot = rootObj.transform;
            }
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;

            EnsureRoot();
            int prefabId = prefab.GetInstanceID();
            GameObject instance = null;

            if (pools.TryGetValue(prefabId, out var stack) && stack.Count > 0)
            {
                instance = stack.Pop();
                if (instance == null) // Handle destroyed object case
                {
                    return Spawn(prefab, position, rotation, parent);
                }
            }
            else
            {
                instance = Object.Instantiate(prefab);
            }

            // Reset transform
            instance.transform.SetPositionAndRotation(position, rotation);
            if (parent != null)
            {
                instance.transform.SetParent(parent);
            }
            else
            {
                instance.transform.SetParent(null); // Ensure it's not stick to pool root if parent is null
            }
            
            instance.SetActive(true);
            
            // Restart particles
            var particles = instance.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                ps.Clear(true);
                ps.Play(true);
            }

            activeInstances[instance] = prefabId;
            return instance;
        }

        public static void Return(GameObject instance)
        {
            if (instance == null) return;
            
            if (activeInstances.TryGetValue(instance, out int prefabId))
            {
                activeInstances.Remove(instance);
                
                // Stop particles
                var particles = instance.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particles)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                instance.SetActive(false);
                instance.transform.SetParent(poolRoot);

                if (!pools.TryGetValue(prefabId, out var stack))
                {
                    stack = new Stack<GameObject>();
                    pools[prefabId] = stack;
                }
                stack.Push(instance);
            }
            else
            {
                // Not managed by pool, just destroy
                Object.Destroy(instance);
            }
        }

        public static void Clear()
        {
            pools.Clear();
            activeInstances.Clear();
            if (poolRoot != null)
            {
                Object.Destroy(poolRoot.gameObject);
                poolRoot = null;
            }
        }
    }
}
