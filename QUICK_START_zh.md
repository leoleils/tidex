# Shary NPC模组 - 快速入门指南

## 项目结构
```
tidex/
├── SharyMod/              # Content Patcher模组（角色定义）
│   ├── assets/            # 角色精灵图和肖像
│   ├── i18n/              # 对话文件
│   ├── content.json       # 角色数据和资源
│   ├── manifest.json      # 模组元数据
│   └── README.md          # 模组描述和使用说明
│
├── SharySMAPI/            # SMAPI模组（高级功能）
│   ├── Services/          # 服务类（记忆系统、Qwen集成等）
│   ├── Models/            # 数据模型
│   ├── ModEntry.cs        # 主模组类
│   ├── ModConfig.cs       # 配置模型
│   ├── manifest.json      # 模组元数据
│   ├── config.json        # 配置文件
│   ├── SharySMAPI.csproj  # 项目文件
│   └── README.md          # 技术文档
│
├── README.md              # 英文主README
├── README_zh.md           # 中文主README
└── package.sh             # 打包脚本
```

## 安装方法

1. 确保已安装SMAPI和Content Patcher
2. 下载或克隆此仓库
3. 将`SharyMod`和`SharySMAPI`文件夹复制到`StardewValley/Mods`目录
4. 使用SMAPI运行游戏

## 配置选项

要启用Qwen AI集成：

1. 从[DashScope](https://dashscope.aliyun.com/)获取API密钥
2. 为您的账户启用qwen3-235b-a22b-instruct-2507模型
3. 编辑`StardewValley/Mods/SharySMAPI/config.json`：
   ```json
   {
     "QwenApiKey": "your-api-key-here"
   }
   ```

## 功能概览

### Content Patcher模组 (SharyMod)
- 角色定义和基本行为
- 日程安排和对话
- 节日集成
- 视觉资源

### SMAPI模组 (SharySMAPI)
- Qwen AI集成实现动态对话
- 记忆系统实现上下文感知
- 基于友谊度的资源收集
- 多轮对话支持
- 交互日志记录

## 开发说明

从源码构建：

1. 在Visual Studio或VS Code中打开`tidex.sln`
2. 设置`StardewPath`环境变量为您的游戏安装目录
3. 构建解决方案

## 文档资料

- `SharyMod/README.md` - 角色功能和安装说明
- `SharySMAPI/README.md` - 技术实现细节
- `SharySMAPI/Qwen-Integration-Guide.md` - Qwen集成详情
- `SharySMAPI/Memory-System-Guide.md` - 记忆系统实现
- `SharySMAPI/Resource-Collection-Guide.md` - 资源收集实现
- `Shary-Complete-Guide.md` - 完整项目概述