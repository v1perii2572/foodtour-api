# Giai đoạn build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj và khôi phục dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ mã nguồn và build
COPY . ./
RUN dotnet publish -c Release -o out

# Giai đoạn chạy
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Khai báo cổng
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "FoodTour.API.dll"]
