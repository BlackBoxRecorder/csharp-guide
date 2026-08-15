import * as path from 'node:path';
import { defineConfig } from '@rspress/core';

export default defineConfig({
  root: path.join(__dirname, 'docs'),
  lang: 'zh',
  title: 'C# Guide',
  // GitHub Pages 项目站点（https://BlackBoxRecorder.github.io/csharp-guide/）
  // 需要子路径 base；本地 dev 时保持根路径，避免影响开发体验
  base: process.env.NODE_ENV === 'production' ? '/csharp-guide/' : '/',
  siteOrigin: 'https://BlackBoxRecorder.github.io',
  icon: '/rspress-icon.png',
  logo: {
    light: '/rspress-light-logo.png',
    dark: '/rspress-dark-logo.png',
  },
  themeConfig: {
    socialLinks: [
      {
        icon: 'github',
        mode: 'link',
        content: 'https://github.com/web-infra-dev/rspress',
      },
    ],
  },
});
