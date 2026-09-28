FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChannelCenter.API.csproj", "./"]
RUN dotnet restore "ChannelCenter.API.csproj"
COPY . .
RUN dotnet build "ChannelCenter.API.csproj" -c Release -o /app/build
RUN dotnet publish "ChannelCenter.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChannelCenter.API.dll"]
