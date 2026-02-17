using System;
using System.Collections.Generic;

namespace SkillEditor
{
    /// <summary>
    /// 技能播放器核心：驱动 Process 生命周期的状态机
    /// 纯 C# 类（不继承 MonoBehaviour），支持区间扫描、事件订阅、Seek、三层清理
    /// </summary>
    public class SkillRunner
    {
        /// <summary>
        /// 播放状态
        /// </summary>
        public enum State
        {
            Idle,
            Playing,
            Paused
        }

        // ─── 公开状态 ───

        /// <summary>
        /// 当前播放状态
        /// </summary>
        public State CurrentState { get; private set; } = State.Idle;

        /// <summary>
        /// 当前播放时间（秒）
        /// </summary>
        public float CurrentTime { get; private set; }

        /// <summary>
        /// 当前播放的时间轴
        /// </summary>
        public SkillTimeline Timeline { get; private set; }

        // ─── 事件 ───

        /// <summary>
        /// 播放开始时触发
        /// </summary>
        public event Action OnStart;

        /// <summary>
        /// 自然播放完毕或 Stop() 时触发
        /// </summary>
        public event Action OnEnd;

        /// <summary>
        /// 被新技能打断时触发（在旧技能清理后、新技能开始前）
        /// </summary>
        public event Action OnInterrupt;

        /// <summary>
        /// 暂停时触发
        /// </summary>
        public event Action OnPause;

        /// <summary>
        /// 恢复时触发
        /// </summary>
        public event Action OnResume;

        /// <summary>
        /// 循环播放一轮完成时触发
        /// </summary>
        public event Action OnLoopComplete;

        /// <summary>
        /// 每帧触发，参数为当前时间
        /// </summary>
        public event Action<float> OnTick;

        // ─── 内部 ───

        private PlayMode playMode;
        private ProcessContext context;
        private List<ProcessInstance> processes = new List<ProcessInstance>();

        /// <summary>
        /// Process 实例与其运行状态的绑定
        /// </summary>
        private struct ProcessInstance
        {
            public IProcess process;
            public ClipBase clip;
            public bool isActive;
        }

        public SkillRunner(PlayMode mode)
        {
            playMode = mode;
        }

        // ─── 播放控制 ───

        /// <summary>
        /// 开始播放（如正在播放或暂停则先打断旧技能）
        /// </summary>
        public void Play(SkillTimeline timeline, ProcessContext context)
        {
            if (CurrentState != State.Idle)
            {
                InterruptInternal();
            }

            this.Timeline = timeline;
            this.context = context;
            CurrentTime = 0f;
            CurrentState = State.Playing;

            BuildProcesses();

            foreach (var inst in processes)
            {
                inst.process.OnEnable();
            }

            OnStart?.Invoke();
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (CurrentState != State.Playing) return;
            CurrentState = State.Paused;
            OnPause?.Invoke();
        }

        /// <summary>
        /// 恢复播放
        /// </summary>
        public void Resume()
        {
            if (CurrentState != State.Paused) return;
            CurrentState = State.Playing;
            OnResume?.Invoke();
        }

        /// <summary>
        /// 停止播放（正常结束）
        /// </summary>
        public void Stop()
        {
            if (CurrentState == State.Idle) return;
            FullCleanup();
            CurrentState = State.Idle;
            OnEnd?.Invoke();
            ClearEvents();
        }

        /// <summary>
        /// 跳转到指定时间点（编辑器 Seek）
        /// 直接跳转：Exit 脱离区间的 Process，Enter 进入新区间的 Process
        /// </summary>
        public void Seek(float targetTime)
        {
            for (int i = 0; i < processes.Count; i++)
            {
                var inst = processes[i];
                bool willBeActive = targetTime >= inst.clip.startTime
                                 && targetTime < inst.clip.EndTime;

                if (inst.isActive && !willBeActive)
                {
                    inst.process.OnExit();
                    inst.isActive = false;
                }

                if (!inst.isActive && willBeActive)
                {
                    inst.process.OnEnter();
                    inst.isActive = true;
                }

                processes[i] = inst;
            }

            CurrentTime = targetTime;
            //刷新当前帧画面
            foreach (var inst in processes)
            {
                if (inst.isActive)
                {
                    inst.process.OnUpdate(CurrentTime, 0f); // deltaTime = 0 表示静态采样
                }
            }
        }

        // ─── 每帧驱动 ───

        /// <summary>
        /// 每帧驱动（由外部调用）
        /// 编辑器预览：由 EditorApplication.update 传入 editorDelta 或 1/frameRate
        /// 运行时：由 SkillLifecycleManager.Update 传入 Time.deltaTime
        /// 帧同步：由逻辑帧回调传入 fixedDelta
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (CurrentState != State.Playing) return;

            float speed = context.GlobalPlaySpeed;
            CurrentTime += deltaTime * speed;

            // 区间扫描
            for (int i = 0; i < processes.Count; i++)
            {
                var inst = processes[i];
                bool shouldBeActive = CurrentTime >= inst.clip.startTime
                                   && CurrentTime < inst.clip.EndTime;

                // 进入区间
                if (shouldBeActive && !inst.isActive)
                {
                    inst.process.OnEnter();
                    inst.isActive = true;
                }

                // 区间内更新
                if (shouldBeActive && inst.isActive)
                {
                    inst.process.OnUpdate(CurrentTime, deltaTime);
                }

                // 离开区间
                if (!shouldBeActive && inst.isActive)
                {
                    inst.process.OnExit();
                    inst.isActive = false;
                }

                processes[i] = inst;
            }

            OnTick?.Invoke(CurrentTime);

            // 播放结束检测
            if (Timeline != null && CurrentTime >= Timeline.duration)
            {
                if (Timeline.isLoop)
                {
                    ResetActiveProcesses();
                    CurrentTime = 0f;
                    OnLoopComplete?.Invoke();
                }
                else
                {
                    FullCleanup();
                    CurrentState = State.Idle;
                    OnEnd?.Invoke();
                    ClearEvents();
                }
            }
        }

        // ─── 私有方法 ───

        /// <summary>
        /// 内部打断：触发事件 → 清理 → 重置
        /// </summary>
        private void InterruptInternal()
        {
            OnInterrupt?.Invoke();
            FullCleanup();
            ClearEvents();
            CurrentState = State.Idle;
        }

        /// <summary>
        /// 为当前 Timeline 中所有启用的 Clip 创建 Process
        /// </summary>
        private void BuildProcesses()
        {
            processes.Clear();

            if (Timeline == null) return;

            foreach (var track in Timeline.AllTracks)
            {
                if (!track.isEnabled) continue;

                foreach (var clip in track.clips)
                {
                    if (!clip.isEnabled) continue;

                    var process = ProcessFactory.Create(clip, playMode);
                    if (process == null) continue;

                    process.Initialize(clip, context);
                    processes.Add(new ProcessInstance
                    {
                        process = process,
                        clip = clip,
                        isActive = false
                    });
                }
            }
        }

        /// <summary>
        /// 完整清理（三层 + 池归还）
        /// 级别 1: OnExit（实例级）→ 级别 2: OnDisable（进程级）→ 归还池 → 级别 3: SystemCleanup
        /// </summary>
        private void FullCleanup()
        {
            // 级别 1: 实例级清理
            foreach (var inst in processes)
            {
                if (inst.isActive)
                {
                    inst.process.OnExit();
                }
            }

            // 级别 2: 进程级清理
            foreach (var inst in processes)
            {
                inst.process.OnDisable();
            }

            // 归还对象池
            foreach (var inst in processes)
            {
                ProcessFactory.Return(inst.process);
            }

            processes.Clear();

            // 级别 3: 系统级清理
            context?.ExecuteCleanups();
        }

        /// <summary>
        /// 重置所有活跃 Process（循环播放时调用）
        /// </summary>
        private void ResetActiveProcesses()
        {
            for (int i = 0; i < processes.Count; i++)
            {
                var inst = processes[i];
                if (inst.isActive)
                {
                    inst.process.OnExit();
                    inst.isActive = false;
                    processes[i] = inst;
                }
            }
        }

        /// <summary>
        /// 清除所有事件订阅（Interrupt / Stop 后调用）
        /// </summary>
        private void ClearEvents()
        {
            OnStart = null;
            OnEnd = null;
            OnInterrupt = null;
            OnPause = null;
            OnResume = null;
            OnLoopComplete = null;
            OnTick = null;
        }
    }
}
