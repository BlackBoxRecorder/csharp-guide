# C# Guide

基于 [Rspress](https://rspress.rs) 构建的 C# 学习指南文档站，源码托管于 GitHub Pages。

## 目录

- [快速开始](#快速开始)
  - [安装依赖](#安装依赖)
  - [本地开发](#本地开发)
  - [生产构建](#生产构建)
  - [本地预览](#本地预览)
- [Markdown 扩展功能指南](#markdown-扩展功能指南)
  - [标准 Markdown 与 GFM](#标准-markdown-与-gfm)
  - [Callout 提示块](#callout-提示块)
  - [代码块](#代码块)
  - [表格](#表格)
  - [文本与列表](#文本与列表)
  - [链接与图片](#链接与图片)
  - [Frontmatter 元数据](#frontmatter-元数据)
  - [MDX 与组件](#mdx-与组件)
  - [附录：需要插件的扩展能力](#附录需要插件的扩展能力)

## 快速开始

### 安装依赖

```bash
npm install
```

### 本地开发

```bash
npm run dev
```

默认端口 `5173`，可通过 `--port` / `--host` 指定端口或监听地址。

### 生产构建

```bash
npm run build
```

默认输出到 `doc_build` 目录。

### 本地预览

```bash
npm run preview
```

预览生产构建产物。

## Markdown 扩展功能指南

> 本指南基于 Rspress 2.0.x 实测验证（见 `package.json`），标注 ✅ 的语法均已在本项目构建产物中确认生效，未标注的以官方文档为准。

### 标准 Markdown 与 GFM

Rspress 完整支持 [CommonMark](https://commonmark.org/) 标准语法与大部分 [GFM（GitHub Flavored Markdown）](https://github.github.com/gfm/) 扩展，无需任何配置：

- 标题、**粗体**、*斜体*、~~删除线~~、`行内代码`、引用、无序/有序列表 ✅
- [任务列表](#任务列表)、[脚注](#脚注)、[表格](#表格)（详见下文）✅
- 行内 HTML（如 `<details>`、`<kbd>`）✅

**例外**：GFM 的 Emoji 短代码（如 `:smile:`）**不支持**，会原样输出，请直接使用 Unicode Emoji（✅ 实测确认）。

### Callout 提示块

Callout（提示块）用于在文档中标记重要信息，是本项目最常用的扩展语法。

#### 基本用法

用 `:::` 包裹块级内容，支持 **7 种类型**：

````markdown
:::note
This is a `note` callout
:::

:::tip
This is a `tip` callout
:::

:::important
This is an `important` callout
:::

:::info
This is an `info` callout
:::

:::warning
This is a `warning` callout
:::

:::danger
This is a `danger` callout
:::

:::details
This is a `details` callout
:::
````

✅ 实测：全部渲染为带图标和标题的提示卡片；`details` 类型渲染为可折叠面板。

**注意**：类型名必须**小写**，`:::Tip` 或 `:::Warning` 不会被识别。

#### 自定义标题

两种写法，任选其一：

````markdown
:::tip Custom Title
This is a callout with a custom title
:::
````

````markdown
:::tip{title="Custom Title"}
This is a callout with a custom title
:::
````

✅ 实测：两种写法均生效。**推荐第一种**——在 `.mdx` 文件中使用花括号写法时需要转义为 `:::tip\{title="..."}`，且花括号语法与 MDX 冲突，容易出错。

#### GitHub Alerts 语法

Rspress 兼容 GitHub 的 [Markdown Alerts](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#alerts) 引用语法，效果与 `:::` 相同：

```markdown
> [!NOTE]
> This is a note

> [!TIP]
> This is a tip

> [!WARNING]
> This is a warning

> [!DANGER]
> This is a danger
```

✅ 实测：同样渲染为提示卡片（标题为大写原文，如 `NOTE`）。注意与普通引用块的区别：`[!TYPE]` 必须放在引用第一行。

- 官方文档：[Container](https://rspress.rs/guide/use-mdx/container)

### 代码块

代码块使用 [Shiki](https://shiki.style) 在构建期完成语法高亮，运行时零开销。

#### 基本用法与标题

````markdown
```csharp
Console.WriteLine("Hello World");
```

```csharp title="Program.cs"
Console.WriteLine("Hello World");
```
````

✅ 实测：`title="..."` 在代码块顶部渲染文件名标题栏。语言支持列表见 [Shiki 支持的语言](https://shiki.style/languages)，构建期自动检测，无需预注册。

#### 常用 meta 属性

可在代码块围栏后追加空格分隔的 meta 属性，可任意组合：

````markdown
```csharp lineNumbers wrapCode title="Program.cs"
Console.WriteLine("Hello World");
var longLine = "This is a very long line that will wrap";
```
````

| 属性 | 作用 | ✅ 实测 |
| --- | --- | --- |
| `lineNumbers` | 显示行号 | ✅ |
| `wrapCode` | 长行自动换行（默认横向滚动） | ✅ |
| `fold` / `height="200"` | 折叠展开 / 固定高度滚动 | 官方文档 |
| `file="./path/to/file.cs"` | 引用外部文件内容，无需手写代码 | 官方文档 |

`lineNumbers`、`wrapCode` 也可通过 `rspress.config.ts` 的 `markdown.showLineNumbers` / `markdown.defaultWrapCode` 全局开启，单块用 `lineNumbers=false` 关闭。

#### 行高亮：默认不可用，需配置

**⚠️ 重要**：`{1,3-4}` 与 `// [!code highlight]` 两种行高亮语法默认**不生效**（✅ 实测确认：无任何效果），必须先在 `rspress.config.ts` 中注册 Shiki transformer：

```ts
// rspress.config.ts
import { defineConfig } from '@rspress/core';
import { transformerNotationHighlight } from '@shikijs/transformers';
import { transformerCompatibleMetaHighlight } from '@rspress/core/shiki-transformers';

export default defineConfig({
  markdown: {
    shiki: {
      transformers: [
        transformerNotationHighlight(), // 支持 // [!code highlight]
        transformerCompatibleMetaHighlight(), // 支持 {1,3-4}
      ],
    },
  },
});
```

配置后：

````markdown
```csharp {1,3-4}
Console.WriteLine("行 1 高亮");
Console.WriteLine("行 2 不高亮");
Console.WriteLine("行 3 高亮");
Console.WriteLine("行 4 高亮");
```
````

#### diff 差异代码块

````markdown
```diff
- Console.WriteLine("deleted");
+ Console.WriteLine("added");
  Console.WriteLine("unchanged");
```
````


#### 其他能力

- **自定义锚点/引用外部文件**：`file="./_demo.cs"`（相对路径）、`file="/demo.cs"`（相对 docs 目录）、`file="<root>/src/demo.cs"`（相对项目根目录）；配合 `_` 前缀文件可避免被路由收录
- **复制/换行按钮**：✅ 实测每个代码块自动带复制与换行切换按钮

- 官方文档：[Code blocks](https://rspress.rs/guide/use-mdx/code-blocks)

### 表格

Rspress 原生支持 GFM 表格语法，并自动包裹横向滚动容器（✅ 实测：窄屏下可横向滚动，不会撑破布局）。

#### 基本语法与对齐

````markdown
| 功能 | 语法 | 默认 |
| :--- | :---: | ---: |
| 左对齐 | `:---` | A |
| 居中 | `:---:` | B |
| 右对齐 | `---:` | C |
````

✅ 实测：分隔行中 `:---`（左）、`:---:`（中）、`---:`（右）三种对齐均生效。

#### 单元格内嵌套

表格单元格内可以放行内代码、链接、Emoji 等任意 Markdown 内容：

````markdown
| 指令 | 说明 |
| --- | --- |
| `npm run dev` | [开发服务器](/guide/start/getting-started) |
| `npm run build` | 生产构建 |
````

✅ 实测：行内代码与链接均正常渲染，链接自动解析为站内路由。

#### 合并单元格：语法层不支持，用 HTML 表格

**⚠️ GFM 表格语法不支持 `colspan` / `rowspan` 合并单元格**，Rspress 同样如此。需要合并时改用 HTML 表格（✅ 实测：HTML 表格原样保留，`colspan`/`rowspan` 全部生效）：

````html
<table>
  <tr>
    <th>合并单元格</th>
    <th>普通列</th>
  </tr>
  <tr>
    <td colspan="2">横向合并两个单元格</td>
  </tr>
  <tr>
    <td rowspan="2">纵向合并两行</td>
    <td>第一行</td>
  </tr>
  <tr>
    <td>第二行</td>
  </tr>
</table>
````

同理，需要自定义表格样式（如背景色、边框）时，可在 HTML 表格标签上加 `style` / `class` 属性，或在主题中覆写 CSS。`.md` 文件可直接写 HTML 表格；`.mdx` 文件同样支持（注意 MDX 中属性需遵循 JSX 规则，如 `className`）。

### 文本与列表

#### 脚注

````markdown
Rspress 支持脚注[^1]，可引用多次[^1]。

[^1]: 这是脚注内容。
````

✅ 实测：正文渲染为上标引用，文末自动生成 `Footnotes` 区块并带 ↩ 返回链接；同一脚注多次引用也正确。

#### 任务列表

````markdown
- [x] 已完成任务
- [ ] 未完成任务
````

✅ 实测：渲染为原生复选框（只读）。

#### Emoji

✅ 实测：短代码 `:smile:` **不生效**（原样输出）。直接用 Unicode：`😄 🚀 ⚠️`。

### 链接与图片

#### 站内链接：推荐文件路径格式

Rspress 支持两种站内链接格式，**推荐文件路径格式**（IDE 可跳转、文件移动自动更新、不受 `cleanUrls` 影响）：

````markdown
[相对路径](../basic/types/record.md)
[绝对路径](/basic/types/record.md)
[URL 格式](/basic/types/record)
````

✅ 实测：三种写法均正确解析为站内路由（自动加 `base` 前缀与 `.html` 后缀）。同一项目内请保持一种格式，保持一致风格。

#### 锚点

````markdown
[跳转到锚点](#代码块)

[跨页锚点](../basic/types/record.md#记录)
````

✅ 实测：中文标题自动生成锚点并可跳转（URL 自动编码）。

自定义锚点 ID（默认锚点由标题自动生成，可用 `{#id}` 覆盖；`.mdx` 中需写成 `\{#id}`）：

````markdown
## Hello World {#custom-anchor-id}
````

✅ 实测：`[跳转](#custom-anchor-id)` 正常定位。

#### 外部链接与图片

- ✅ 实测：外部链接自动添加 `target="_blank"` 与 `rel="noopener noreferrer"`（新窗口打开）
- 图片使用标准 Markdown 语法 `![alt](path)`；站内图片建议放 `docs/public/` 目录，用绝对路径 `/xxx.png` 引用（部署到子路径时仍可用）
- 可选的链接质量检查：`markdown.link.checkDeadLinks`（死链）、`markdown.link.checkAnchors`（死锚点）、`markdown.image.checkDeadImages`（死图）

- 官方文档：[Links](https://rspress.rs/guide/use-mdx/link)、[Static assets](https://rspress.rs/guide/basic/static-assets)

### Frontmatter 元数据

文件头部可用 YAML 定义页面元数据：

````yaml
---
title: 页面标题
description: 页面描述（用于 SEO 与分享）
---
````

- `title`：默认取 H1 标题，可用 frontmatter 覆盖
- `description`：默认取 H1 之后第一个段落，可用 frontmatter 覆盖
- 其他常用字段：`pageType`（`home` / `doc` / `doc-wide` / `custom` / `blank`）、`sidebar`、`outline`、`head`（自定义 meta 标签）等

- 官方文档：[Frontmatter](https://rspress.rs/guide/use-mdx/frontmatter)、[Frontmatter config](https://rspress.rs/api/config/config-frontmatter)

### MDX 与组件

Rspress 基于 [MDX](https://mdxjs.com/) 构建，**文件扩展名 `.mdx`** 即可在 Markdown 中直接使用 React 组件（`.md` 不行）。

#### Tabs 标签组（最常用）

````mdx
import { Tabs, Tab } from '@rspress/core/theme';

<Tabs>
<Tab label="npm">

```bash
npm install
```

</Tab>
<Tab label="pnpm">

```bash
pnpm install
```

</Tab>
</Tabs>
````

✅ 实测：正常渲染为可切换标签页。进阶用法：

- `groupId="xxx"`：页面内多处相同 `groupId` 的 Tabs 同步切换
- `values` + `defaultValue`：分离标签与内容，按值预选
- `tabPosition="left" | "center"`：标签对齐方式

#### 其他内置组件

`@rspress/core/theme` 还内置了：

| 组件 | 用途 |
| --- | --- |
| `Callout` | 提示块组件（Markdown `:::` 语法的组件版） |
| `Badge` | 行内小标签 |
| `Steps` | 分步指引块 |
| `PackageManagerTabs` | 多包管理器命令切换 |
| `PageTabs` / `SourceCode` / `Prompt` / `CodeBlockRuntime` | 页内分页 / 源码链接 / 提示条 / 运行时代码块 |

#### 组织约定

- 以 `_` 开头的 `.mdx` / `.tsx` 文件默认**不生成路由**，适合放可复用的 MDX 片段与组件
- 自定义组件建议放 `docs/` 之外（如项目根目录 `components/`），避免被文档路由扫描

- 官方文档：[MDX and React components](https://rspress.rs/guide/use-mdx/components)、[Doc Components](https://rspress.rs/ui/components/index)

### 附录：需要插件的扩展能力

以下能力**不属于默认语法**，需要安装插件或在 `rspress.config.ts` 中扩展：

| 能力 | 实现方式 |
| --- | --- |
| 数学公式（KaTeX） | 通过 `markdown.remarkPlugins` / `rehypePlugins` 接入 `remark-math` + `rehype-katex` |
| Mermaid / 图表 | 通过 remark 插件（如社区 `rspress-plugin-mermaid`）接入 |
| 组件实时预览 / 在线编辑 | `@rspress/plugin-preview` / `@rspress/plugin-playground` |
| API 文档自动生成 | `@rspress/plugin-api-docgen`（React 组件）/ `@rspress/plugin-typedoc`（TypeScript） |
| TypeScript 类型提示代码块 | `@rspress/plugin-twoslash` |
| 全文搜索接入（Algolia / Typesense） | `@rspress/plugin-algolia` / `rspress-plugin-typesense` |
| 站点 SEO（sitemap / RSS / llms.txt） | `@rspress/plugin-sitemap` / `@rspress/plugin-rss` / `@rspress/plugin-llms` |

- 官方文档：[Build extension](https://rspress.rs/guide/advanced/extend-build)、[Official plugins](https://rspress.rs/plugin/official-plugins/overview)
