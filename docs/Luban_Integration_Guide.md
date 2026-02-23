# Luban 配置系统接入指南

本文档介绍如何在 Unity 项目中从零开始接入 **Luban** 配置系统，涵盖环境搭建、导表配置、Unity 运行时接入以及常见问题修复。

---

## 1. 核心流程总览

Luban 的工作模式是：**Excel/JSON 数据 -> Luban 工具 (+ Schema) -> 生成 C# 代码 & 生成 JSON 数据 -> Unity 加载**。

---

## 2. 环境搭建与目录结构

建议在项目根目录下建立如下结构（以本项目为例）：

```text
ProjectRoot/
├── Assets/                 # Unity 工程
└── Configs/                # 配置工程目录
    ├── Datas/              # 存放 Excel 数据文件 (.xlsx)
    ├── Defines/            # 存放自定义结构定义 (.xml)
    ├── luban.conf          # Luban 项目主配置文件
    └── gen.bat             # 导出脚本 (Windows)
    └── Tool/
│   	├── Luban/              # 存放 Luban.exe 及其 DLL
```

### 2.1 获取 Luban
1. 前往 [Luban Release](https://github.com/focus-creative-games/luban/releases) 下载最新的发布包。
2. 将其中的二进制文件解压到 `Tools/Luban/` 下。

### 2.2 导入 Unity 插件包
1. 在 Luban 仓库或发布包中找到 https://github.com/focus-creative-games/luban_unity.git。
2. 在 Unity Editor 中选择 `Window -> Package Manager -> Add package from git`，导入该包。
   - **注意**：导入此包后，Unity 才能够识别 `Luban` 命名空间和 `SimpleJSON` 类型。

---

## 3. 配置工程设置

### 3.1 luban.conf (主配置)
定义数据目录、定义文件以及导出目标。

```json
{
  "groups": [
    {"names":["c"], "default":true},
    {"names":["s"], "default":true}
  ],
  "schemaFiles": [
    {"fileName":"Defines", "type":""},
    {"fileName":"Datas/__tables__.xlsx", "type":"table"}
  ],
  "dataDir": "Datas",
  "targets": [
    {"name":"client", "manager":"Tables", "groups":["c"], "topModule":"cfg"}
  ]
}
```

### 3.2 定义一个表 (Excel 模式)
在 `Datas/__tables__.xlsx` 中定义表名、代码类名及数据文件路径：

| full_name | value_type | read_schema_from_file | input |
| :--- | :--- | :--- | :--- |
| TbItem | Item | true | item_data.xlsx |

### 3.3 代码生成路径、文件夹与命名空间
很多同学困惑生成的代码为什么在 `demo` 文件夹下，这里由两个核心参数决定：

1. **`topModule` (在 luban.conf 中定义)**：这是 C# 代码的顶层命名空间（如 `cfg`）。
2. **`full_name` (在 __tables__.xlsx 中定义)**：
   - 格式通常为 `模块名.类名`。
   - 例如：`demo.TbItem`。
   - **结果**：Luban 会在输出目录下创建一个 `demo` 文件夹，且生成的脚本路径为 `cfg.demo.TbItem`。
   - **结论**：文件夹名对应的是你的 **模块名**，而不是 Excel 的文件名。

---

### 3.3 数据表 Excel 结构说明 (Header)
Luban 通过 Excel 前几行的特殊标识符来解析结构。最常见的编排方式如下：

| 标识符 | 说明 | 示例 |
| :--- | :--- | :--- |
| **##var** | **变量名行**（必须）。生成 C# 代码时的字段名。 | `id`, `name`, `count` |
| **##type** | **类型行**。定义字段类型（若在 schema 中定义了，此处可省略）。 | `int`, `string`, `long` |
| **##group** | **分组行**（可选）。用于区分客户端/服务器逻辑，如 `c` 或 `s`。 | `c`, `s` |
| **##desc** | **描述行**（可选）。生成代码时的注释内容。 | `道具ID`, `道具名称` |
| **##** | **注释标识**。位于行首表示注释整行；位于列首表示注释整列。 | |

**标准的 Excel 布局示例：**

| ##desc | 道具ID | 道具名称 | 道具描述 | 堆叠数量 |
| :--- | :--- | :--- | :--- | :--- |
| **##var** | id | name | desc | count |
| **##type** | int | string| string | int |
| **##group**| c,s | c,s | c | c,s |
| | 1001| 道具1 | 这是一个测试道具 | 10 |
| | 1002| 道具2 | 另一个测试道具 | 99 |

> [!TIP]
>
> 1. 标识符行的顺序可以调整，但 `##var` 是识别字段的核心。Luban 会根据 `luban.conf` 或命令行参数来确定哪些行作为 Header 处理。
> 2. 第一列会作为Table字典表如<string,item>的Key

---

## 4. 导出脚本 (gen.bat)

通过命令行驱动 `Luban.exe` 执行导出。**关键参数**：

- WORKSPACE工作区变量，".."上级目录，“.”同级目录，以gen.bat为基准

- `-c cs-simple-json`：生成适配 SimpleJSON 的 C# 代码。
- `-d json`：生成 JSON 格式的数据文件。
- `-x outputCodeDir`：生成的 C# 代码存放路径。
- `-x outputDataDir`：生成的 JSON 数据存放路径。

```batch
@echo off
set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\Configs\Tool\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t all ^
    -d json ^
    -c cs-simple-json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=%WORKSPACE%\Assets\Configs ^
    -x outputCodeDir=%WORKSPACE%\Assets\GameClient\Generated\Config

pause
```

---

## 5. Unity 运行时接入

### 5.1 实现 ConfigManager
Luban 生成的代码需要一个“加载器”来读取文件。我们需要实现一个 `ConfigManager` 来对接资源系统（如 YooAsset）。

```csharp
public class ConfigManager : Singleton<ConfigManager>
{
    public cfg.Tables Tables { get; private set; }

    public async Task InitializeAsync()
    {
        // 实例化 Tables 并注入加载委托
        Tables = new cfg.Tables(LoadConfigJson);
        Debug.Log("[ConfigManager] 初始化完成");
    }

    private JSONNode LoadConfigJson(string file)
    {
        // 需提供 Assets/... 开始的完整路径及扩展名
        string path = $"Assets/Configs/{file}.json";
        var asset = ResourceManager.Instance.LoadAsset<TextAsset>(path);
        return JSONNode.Parse(asset.text);
    }
}
```

### 5.2 理解 Item 与 TbItem 的区别 (核心概念)
这是新手最容易混淆的地方：

- **`Item` (Bean/记录体/行)**：
  - 对应 Excel 中的 **一行数据**（一条记录）。
  - 它是一个简单的类，包含 `id`, `name` 等属性字段。
- **`TbItem` (Table/容器/表)**：
  - 对应 **整个 Excel 表**（所有数据的集合）。
  - 它负责管理所有的 `Item` 实例。
  - 它提供了查询接口（如 `Get`, `DataList`）。

### 5.3 数据读取方式大全 (示例)

一旦通过 `ConfigManager.Instance.Tables` 拿到了表实例，你可以通过以下方式操作数据：

```csharp
var tables = ConfigManager.Instance.Tables;

// 1. 直取法 (要求 Key 必须存在，否则抛出 KeyNotFoundException)
// 适用于：你 100% 确定 ID 在表里，通常用于测试或强逻辑关联
var item = tables.TbItem.Get(1001);

// 2. 安全获取法 (推荐)
// 适用于：ID 可能由后端传过来，或者存在不确定性
if (tables.TbItem.TryGetValue(1001, out var safeItem)) {
    Debug.Log($"找到道具: {safeItem.Name}");
}

// 3. 遍历整张表 (List)
// 适用于：需要显示列表 UI，或进行某种全局搜索/过滤
foreach (var record in tables.TbItem.DataList) {
    Debug.Log($"遍历中: {record.Id}");
}

// 4. 获取某一个字典形式 (Map)
// 适用于：需要根据 ID 快速查找逻辑
var dict = tables.TbItem.DataMap;
if (dict.ContainsKey(1002)) { /* ... */ }
```

---

## 6. 常见问题 (Troubleshooting)

### 6.1 404 Not Found (BuiltinCatalog.bytes)
- **原因**：YooAsset 在 StreamingAssets 下找不到资源包清单。
- **解决**：确保 `ResourceManager` 中的内置根路径精确指向 `StreamingAssets/yoo/<PackageName>` 目录。

### 6.2 location is invalid
- **原因**：提供的加载路径不符合 YooAsset 规范。
- **解决**：确保路径以 `Assets/` 开头，并且包含 `.json` 后缀。

### 6.3 Singleton 缺失
- **原因**：Luban 本身不提供单例基类。
- **解决**：手动实现一个 `Singleton<T>` 泛型类供 `ConfigManager` 使用。

## 7.参考

[Luban+Unity使用，看这一篇文章就够了_unity luban-CSDN博客](https://blog.csdn.net/Blueberry124/article/details/149123903)

官方文档：https://www.datable.cn/docs/beginner/quickstart
