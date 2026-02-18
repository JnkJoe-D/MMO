using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillEditor;
using Game.MAnimSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 运行时播放测试脚本（无需 NUnit）
/// 挂载到场景物体上运行
/// </summary>
public class Test_Anim : MonoBehaviour
{
    public string skillPath = "Assets/新技能.json";
    public TextAsset skillAsset; // 直接拖入 TextAsset 资源（编辑器专用）
    [Range(0f, 3.0f)]
    public float speedMultiplier = 1.0f; // 用于测试不同的播放速度
    private AnimComponent animComp;
    private SkillRunner runner;
    private ProcessContext context;
    private SkillTimeline timeline;
    private float timer = 0f;
    public void Start()
    {
        animComp = gameObject.GetComponent<AnimComponent>();
        // 1. 初始化
        if (animComp == null) animComp = gameObject.AddComponent<AnimComponent>();
        animComp.Initialize();

        // 2. 准备上下文
        context = new ProcessContext(gameObject, SkillEditor.PlayMode.Runtime);
        runner = new SkillRunner(SkillEditor.PlayMode.Runtime);

        if (skillAsset == null)
        {
            // 3. 加载资源
            skillAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(skillPath);
            if (skillAsset == null)
            {
                Debug.LogError($"测试资源未找到: {skillPath}");
                return;
            }
        }
        timeline = ScriptableObject.CreateInstance<SkillTimeline>();
        JsonUtility.FromJsonOverwrite(skillAsset.text, timeline);
        timeline.isLoop = true;
        // 5. 开始播放
        runner.Play(timeline, context);
        Debug.Log($"播放开始: State={runner.CurrentState}");
    }
    void Update()
    {
        if (runner != null)        
        {
            context.GlobalPlaySpeed = speedMultiplier; // 动态调整全局播放速度
            timer += Time.deltaTime;
            float step = 1f / 30f;

            // 使用 while 处理单帧时间过长的情况（追帧）
            while (timer >= step)
            {
                timer -= step; // <--- 关键：减去步长，保留余数 (0.04 - 0.0333 = 0.0067)
                runner.Tick(step);
            }
        }
    }
    [ContextMenu("Test Runtime Play")]
    public void TestRuntimePlay()
    {
        StartCoroutine(TestRoutine());
    }

    private IEnumerator TestRoutine()
    {
        Debug.Log(">>> 开始测试: Runtime Speed Sync");

        // 1. 初始化
        if(animComp == null) animComp = gameObject.AddComponent<AnimComponent>();
        animComp.Initialize();

        // 2. 准备上下文
        context = new ProcessContext(gameObject, SkillEditor.PlayMode.Runtime);
        runner = new SkillRunner(SkillEditor.PlayMode.Runtime);

        // 3. 加载资源
#if UNITY_EDITOR
        TextAsset skillAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(skillPath);
        if (skillAsset == null)
        {
            Debug.LogError($"测试资源未找到: {skillPath}");
            yield break;
        }

        SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
        JsonUtility.FromJsonOverwrite(skillAsset.text, timeline);
#else
        Debug.LogError("请在编辑器下运行此测试以加载 TextAsset");
        yield break;
#endif

        // 5. 开始播放
        runner.Play(timeline, context);
        Debug.Log($"播放开始: State={runner.CurrentState}");

        // --- 测试阶段 1: 正常速度 ---
        // context.GlobalPlaySpeed = 1.0f;
        float dt = 0.1f;
        float startT = runner.CurrentTime;
        runner.Tick(dt);
        
        AssertFloat(runner.CurrentTime, startT + dt, "1.0x Speed");

        yield return null;

        // --- 测试阶段 2: 2倍速 ---
        // context.GlobalPlaySpeed = 2.0f;
        startT = runner.CurrentTime;
        runner.Tick(dt);

        AssertFloat(runner.CurrentTime, startT + dt * 2.0f, "2.0x Speed");

        yield return null;

        // --- 测试阶段 3: 0.5倍速 ---
        // context.GlobalPlaySpeed = 0.5f;
        startT = runner.CurrentTime;
        runner.Tick(dt);

        AssertFloat(runner.CurrentTime, startT + dt * 0.5f, "0.5x Speed");

        Debug.Log("<<< 测试完成");
        
        // 清理
        runner.Stop();
    }

    private void AssertFloat(float actual, float expected, string label)
    {
        if (Mathf.Abs(actual - expected) < 0.001f)
        {
            Debug.Log($"[Pass] {label}: Expected {expected:F3}, Got {actual:F3}");
        }
        else
        {
            Debug.LogError($"[Fail] {label}: Expected {expected:F3}, Got {actual:F3}");
        }
    }
}
