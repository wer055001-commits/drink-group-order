# VibeCoding 專案 AI 協作指引

## 語言規則

- 一律使用**繁體中文**回覆、思考、撰寫文件與註解
- 所有網站介面、提示訊息、按鈕文字、錯誤訊息皆使用繁體中文
- HTML 語系設定為 `zh-TW`

## 使用者背景

使用者**完全沒有軟體開發知識**，請嚴格遵守以下原則：

### 絕對不要

- 對使用者提及任何技術名詞（API、CORS、proxy、component、route、middleware、endpoint 等）
- 在對話中展示程式碼片段
- 要求使用者手動修改任何檔案
- 使用「前端」「後端」等詞彙（改用「畫面」「伺服器」等日常用語）

### 需要使用者做決定時

用淺顯易懂的比喻來解釋，例如：

- 資料庫 → 「像是一個電子檔案櫃，幫你把資料分類存好」
- 登入驗證 → 「像大樓門禁卡，確認身份才能進去」
- 快取 → 「像是把常翻的資料放在桌上，不用每次都去檔案櫃找」
- 部署 → 「像是把做好的作品從工作室搬到展覽廳讓大家看到」

### 回報進度時

- 用「完成了什麼功能」來說明，而非「改了哪些檔案」
- 例：「✅ 已經做好登入頁面，輸入帳號密碼就能進入系統了」
- 不要說：「已修改 LoginView.vue 並新增 /api/auth endpoint」

## 專案結構

```
VibeTemplate/
├── .vscode/                  ← 編輯器設定（F5 一鍵啟動）
│   ├── launch.json
│   └── tasks.json
│
├── Backend/                  ← 伺服器程式（.NET 10，C#）
│   ├── Program.cs            ← 主程式，所有伺服器邏輯寫在這裡
│   ├── Backend.csproj        ← 專案設定
│   ├── appsettings.json      ← 應用程式設定
│   └── Properties/
│       └── launchSettings.json  ← 啟動設定（port 5248）
│
├── Frontend/                 ← 畫面程式（Vue.js 3 + TypeScript）
│   ├── src/
│   │   ├── App.vue           ← 根元件（整個網站的外框）
│   │   ├── main.ts           ← 進入點
│   │   ├── assets/           ← 靜態資源（CSS、圖片）
│   │   ├── components/       ← 可重複使用的小元件
│   │   ├── views/            ← 頁面（每個網址對應一個檔案）
│   │   ├── router/           ← 頁面網址對應設定
│   │   └── stores/           ← 跨頁面共享的資料
│   ├── vite.config.ts        ← 開發伺服器設定（含代理轉發至後端）
│   └── package.json          ← 套件清單
│
├── CLAUDE.md                 ← 本檔案（AI 協作指引）
├── setup.ps1                 ← 環境自動安裝腳本
└── 安裝指引.md               ← 給使用者的安裝說明
```

## 技術規範

### 後端（Backend）

- 框架：.NET 10 Minimal API
- 所有 API 路徑以 `/api/` 開頭
- 執行於 http://localhost:5248
- 已啟用 CORS 允許前端存取
- 已開啟 TreatWarningsAsErrors

### 前端（Frontend）

- 框架：Vue 3 + TypeScript + Vite
- 使用 Composition API（`<script setup>` 語法）
- 樣式使用原生 CSS（不使用 Tailwind 等框架）
- 呼叫伺服器用 `fetch("/api/...")`，Vite 代理自動轉發至後端
- 路由設定在 `src/router/index.ts`
- 共享狀態用 Pinia，放在 `src/stores/`
- 頁面元件放在 `src/views/`，可複用元件放在 `src/components/`

### 啟動方式

使用者按 **F5** 即可一鍵啟動全部環境（自動清理佔用的 port → 安裝套件 → 啟動前後端 → 開啟瀏覽器）。

### 開發注意事項

- 新增頁面時記得同步更新 `router/index.ts`
- 新增 API 時記得加在 `Program.cs` 中，路徑以 `/api/` 開頭
- 所有使用者可見的文字必須是繁體中文
- CSS 寫在各 `.vue` 檔案的 `<style scoped>` 區塊內
