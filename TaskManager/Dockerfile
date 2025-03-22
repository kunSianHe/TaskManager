# 使用 .NET 8 SDK 來建置應用程式
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製 csproj 並還原相依套件
COPY *.csproj ./
RUN dotnet restore

# 複製其他所有檔案並建置應用程式
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# 使用 .NET Runtime 執行應用程式
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "TaskManager.dll"]
