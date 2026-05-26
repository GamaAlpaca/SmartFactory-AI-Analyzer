# 階段 1：使用 .NET 8 SDK 編譯網頁與後端
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# 核心：把兩個專案的專案檔都複製進來，進行 NuGet 還原
COPY SmartFactory.DataAnalyzer/*.csproj ./SmartFactory.DataAnalyzer/
COPY SmartFactory.WebApi/*.csproj ./SmartFactory.WebApi/
RUN dotnet restore ./SmartFactory.WebApi/SmartFactory.WebApi.csproj

# 複製所有程式碼並指定編譯網頁端（它會自動把相依的後端一起編出來）
COPY . .
RUN dotnet publish ./SmartFactory.WebApi/SmartFactory.WebApi.csproj -c Release -o out

# 階段 2：使用 Runtime 執行環境
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# ⚠️ 網頁版貨櫃必備：告訴 Docker 這個貨櫃在虛擬世界裡要對外開放 8080 連接埠
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SmartFactory.WebApi.dll"]