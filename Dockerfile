# ── 第一步：編譯畫面 ──
FROM node:22-slim AS frontend-build
WORKDIR /app/Frontend
COPY Frontend/package*.json ./
RUN npm ci
COPY Frontend/ ./
RUN npm run build-only

# ── 第二步：編譯伺服器 ──
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app/Backend
COPY Backend/*.csproj ./
RUN dotnet restore
COPY Backend/ ./
RUN dotnet publish -c Release -o /out

# ── 第三步：合併運行 ──
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=backend-build /out ./
COPY --from=frontend-build /app/Frontend/dist ./wwwroot

# 建立資料目錄
RUN mkdir -p /app/Data

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Backend.dll"]
