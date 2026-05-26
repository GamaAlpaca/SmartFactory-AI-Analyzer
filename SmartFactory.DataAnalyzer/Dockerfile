# 階段 1：使用微軟官方的 .NET 8 SDK 進行程式碼編譯
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# 複製專案檔並還原 NuGet 套件 (包含 SQLite 與 AI 套件)
COPY *.csproj ./
RUN dotnet restore

# 複製其餘所有程式碼，並編譯成生產環境發佈版本
COPY . ./
RUN dotnet publish -c Release -o out

# 階段 2：使用最輕量、不佔記憶體的 Runtime 執行環境
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# 設定貨櫃啟動時，自動執行妳的 C# 執行檔
ENTRYPOINT ["dotnet", "SmartFactory.DataAnalyzer.dll"]