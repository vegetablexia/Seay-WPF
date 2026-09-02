# Seay 源代码审计系统（WPF 重构版）
ai真好用
基于经典 PHP 代码审计工具 **Seay 源代码审计系统** 的界面重构版本，使用 WPF 重写了全部界面，采用 VS Code 风格深色主题。

> 原版工具及规则库版权归作者（Seay / www.cnseay.com）所有，本项目仅用于学习交流。

## 主要功能

- **代码审计**：打开 PHP 项目后一键自动审计，基于内置规则库扫描可疑漏洞点；结果双击直达对应代码行并自动选中
- **代码编辑器**：语法高亮、深色主题、函数/变量侧栏；右键支持全文追踪、定位函数、全局搜索、调试选中等操作
- **全局搜索**：按关键字或正则搜索整个项目，结果双击打开对应文件
- **工具箱**：
  - PHP 调试：直接运行 PHP 代码片段并查看输出
  - 正则测试：实时正则匹配
  - 编解码：URL / Base64 / Hex / MD5 / ASCII / Unicode
  - 临时记录：随手记录审计笔记

## 环境要求

- Windows 10/11（推荐）
- .NET Framework 4.8
- Visual Studio 2019 或更高版本（含“.NET 桌面开发”工作负载）

## 构建与运行

1. 克隆本仓库
2. 用 Visual Studio 打开根目录下的 `Seay代码审计工具.sln`
3. 解决方案配置选择 **Release**（Release 输出目录已包含审计规则与 PHP 运行时，Debug 目录不包含）
4. 生成解决方案并启动
5. 程序启动后点击左侧「打开项目」选择一个 PHP 项目文件夹，即可开始审计

## 目录结构

```
Seay代码审计工具/            主程序（WPF）
  ├─ Views/                  各功能界面（欢迎页/文件树/编辑器/审计/搜索/工具箱）
  ├─ Controls/               标题栏、侧边栏、编辑器控件
  ├─ Themes/                 深色主题资源字典
  └─ bin/Release/            运行时依赖（php.exe、审计规则 rule.bin、报告模板）
Project/                     ICSharpCode.TextEditor 代码编辑控件库
CSPluginKernel/              插件内核库
```


## 致谢

- [Seay 源代码审计系统](http://www.cnseay.com/) — 原版工具与审计规则
- [ICSharpCode.TextEditor](https://github.com/icsharpcode/SharpDevelop) — 代码编辑控件
