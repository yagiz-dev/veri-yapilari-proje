FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve bağımlılıkları yükle
COPY ["DomEngine.Api/DomEngine.Api.csproj", "DomEngine.Api/"]
COPY ["DomEngine.Core/DomEngine.Core.csproj", "DomEngine.Core/"]
RUN dotnet restore "DomEngine.Api/DomEngine.Api.csproj"

# Tüm kaynak kodları kopyala ve build al
COPY . .
WORKDIR "/src/DomEngine.Api"
RUN dotnet build "DomEngine.Api.csproj" -c Release -o /app/build

# Publish (Yayınlama) işlemi
FROM build AS publish
RUN dotnet publish "DomEngine.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime imajı (Sadece çalıştırmak için hafif imaj)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# .NET 8 varsayılan olarak HTTP için 8080 portunu dinler
EXPOSE 8080

ENTRYPOINT ["dotnet", "DomEngine.Api.dll"]
