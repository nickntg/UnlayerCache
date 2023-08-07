FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY ./Projects/UnlayerCache.API/ /app
WORKDIR /app
RUN dotnet clean -c Release
RUN dotnet build
RUN dotnet publish -c Release -r linux-x64 --self-contained false --output /app/publish/unlayer

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /var/task
COPY --from=build /app/publish/unlayer .
EXPOSE 80
ENTRYPOINT ["dotnet" ,"UnlayerCache.API.dll"]