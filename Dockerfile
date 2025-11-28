# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj c?a c? 3 project v?o tr??c ?? restore (t?n d?ng cache layer)
COPY ["BasicEnglishPlatform.Web/BasicEnglishPlatform.Web.csproj", "BasicEnglishPlatform.Web/"]
COPY ["BasicEnglishPlatform.Services/BasicEnglishPlatform.Services.csproj", "BasicEnglishPlatform.Services/"]
COPY ["BasicEnglishPlatform.Data/BasicEnglishPlatform.Data.csproj", "BasicEnglishPlatform.Data/"]

# Restore dependencies
RUN dotnet restore "BasicEnglishPlatform.Web/BasicEnglishPlatform.Web.csproj"

# Copy to?n b? code c?n l?i
COPY . .

# Build project Web
WORKDIR "/src/BasicEnglishPlatform.Web"
RUN dotnet build "BasicEnglishPlatform.Web.csproj" -c Release -o /app/build

# Publish ra file ch?y
FROM build AS publish
RUN dotnet publish "BasicEnglishPlatform.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime (Ch?y app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# C?u h?nh c?ng m?c ??nh cho Render (Render th??ng d?ng port 80 ho?c bi?n PORT)
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "BasicEnglishPlatform.Web.dll"]