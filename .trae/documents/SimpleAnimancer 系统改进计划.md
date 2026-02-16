# SimpleAnimancer 系统改进计划

## 改进项清单

### 高优先级
1. **AnimLayer 过渡系统重构** - 中断列表法，支持频繁切换场景
2. **AnimLayer 状态缓存机制** - Dictionary 缓存，避免频繁 GC
3. **AnimLayer 状态清理机制** - 延迟清理队列，解决内存泄漏

### 中优先级
4. **LinearMixerState 阈值自动排序** - 插入排序，确保插值正确
5. **BlendTreeState2D 数组预分配** - 预分配缓冲区，消除每帧 GC
6. **AnimState Playable 字段优化** - 语义明确化，消除冗余困惑

### 低优先级
7. **AnimState 时间归一化 API** - NormalizedTime、Pause/Resume

### 验证
8. **更新测试脚本** - 频繁切换测试、缓存验证

## 实施顺序
按依赖关系从底层到上层依次实施：AnimState 优化 → 过渡系统 → 缓存/清理 → Mixer 优化 → 测试验证