#!/usr/bin/env node
/**
 * 生成 docs 目录文章索引（article-index.md）。
 *
 * 用法：npm run docs:index
 *
 * 写作工作流：写文章前先查阅 article-index.md 确认目标内容是否已有文章覆盖，
 * 避免内容重复；写完后重新运行本脚本刷新索引。
 */
import { readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const DOCS_DIR = join(ROOT, 'docs');
const OUTPUT_FILE = join(ROOT, 'article-index.md');
const EXCLUDE_DIRS = new Set(['guide']);

/** 递归收集 docs 下的 .md/.mdx 文件 */
function collectMarkdownFiles(dir) {
  const files = [];
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      if (!EXCLUDE_DIRS.has(entry.name)) {
        files.push(...collectMarkdownFiles(join(dir, entry.name)));
      }
    } else if (entry.isFile() && /\.mdx?$/i.test(entry.name)) {
      files.push(join(dir, entry.name));
    }
  }
  return files;
}

/** 解析 frontmatter 中的 title/description */
function parseFrontmatter(filePath) {
  // docs 下文档可能为 CRLF 行尾，先归一化再匹配 frontmatter
  const content = readFileSync(filePath, 'utf8').replace(/\r\n/g, '\n');
  const match = content.match(/^---\n([\s\S]*?)\n---/);
  if (!match) {
    return {};
  }
  const fm = {};
  for (const line of match[1].split('\n')) {
    const kv = line.match(/^(\w+):\s*(.*)$/);
    if (kv) {
      fm[kv[1]] = kv[2].trim().replace(/^["']|["']$/g, '');
    }
  }
  return fm;
}

/** 转义表格单元格中的竖线 */
function escapeCell(text) {
  return text.replace(/\|/g, '\\|');
}

/** 按二级目录计算分组键 */
function groupKey(relDir) {
  if (relDir === '.') {
    return '(根目录)';
  }
  return relDir.split(/[\\/]/).slice(0, 2).join('/');
}

const files = collectMarkdownFiles(DOCS_DIR)
  .map((absPath) => {
    const { title, description } = parseFrontmatter(absPath);
    // Windows 路径分隔符统一为正斜杠，保证 Markdown 链接有效
    const relPath = relative(DOCS_DIR, absPath).replace(/\\/g, '/');
    return {
      name: relPath.replace(/\.mdx?$/i, ''),
      title: title || relPath.replace(/\.mdx?$/i, ''),
      description: description ?? null,
    };
  })
  .sort((a, b) => a.name.localeCompare(b.name, 'en'));

const missingDesc = files.filter((f) => f.description === null);
for (const f of missingDesc) {
  console.warn(`⚠️  警告: docs/${f.name} 缺少 description`);
}

const groups = new Map();
for (const f of files) {
  const key = groupKey(dirname(f.name));
  if (!groups.has(key)) {
    groups.set(key, []);
  }
  groups.get(key).push(f);
}

const now = new Date();
const dateStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;

const lines = [];
lines.push('# 文章索引');
lines.push('');
lines.push('本站 docs 目录下全部文章的标题、描述与链接索引。');
lines.push('');
lines.push('## 使用说明');
lines.push('');
lines.push(
  '- 写作前请先查阅本文档，确认目标内容是否已有文章覆盖，避免内容重复；引用已有文章时使用本文档中的链接。',
);
lines.push('- 修改 docs 后运行 `npm run docs:index` 重新生成本文档。');
lines.push(
  '- 本文档链接为项目根目录相对路径（无扩展名），在文章内引用时需去掉 `docs/` 前缀，并按文章所在层级添加 `../`。',
);
lines.push(
  '  例如：本文档链接 `./docs/basic/oop/function`，在 `docs/basic/types/record.md` 中引用时应写作 `./../oop/function`。',
);
lines.push('');
lines.push(`共收录 ${files.length} 篇文章，最后更新：${dateStr}`);
lines.push('');

for (const [key, items] of [...groups.entries()].sort((a, b) =>
  a[0].localeCompare(b[0], 'en'),
)) {
  lines.push(`## ${key}`);
  lines.push('');
  lines.push('| 标题 | 描述 | 链接 |');
  lines.push('| :--- | :--- | :--- |');
  for (const item of items) {
    const desc = item.description
      ? escapeCell(item.description)
      : '（无描述，请补充）';
    lines.push(
      `| ${escapeCell(item.title)} | ${desc} | [./docs/${item.name}](./docs/${item.name}) |`,
    );
  }
  lines.push('');
}

writeFileSync(OUTPUT_FILE, lines.join('\n'));
console.log(
  `✔ 已生成 ${relative(ROOT, OUTPUT_FILE)}（${files.length} 篇文章，${groups.size} 个分组）`,
);
if (missingDesc.length > 0) {
  console.warn(`⚠️  共 ${missingDesc.length} 篇文章缺少 description`);
}
