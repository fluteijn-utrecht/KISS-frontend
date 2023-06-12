#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 4433
ENV ASPNETCORE_URLS=http://*:8080
RUN addgroup --group kiss --gid 2000 \
    && adduser \    
    --uid 1000 \
    --gid 2000 \
    "kiss" 
RUN chown kiss:kiss  /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
RUN apt-get update
RUN apt-get install unzip
WORKDIR /app/certs
RUN wget https://developers.kvk.nl/cms/api/uploads/Private_Chain_api_kvk_nl.zip
RUN unzip -j Private_Chain_api_kvk_nl.zip
WORKDIR /src
COPY ["Kiss.Bff/Kiss.Bff.csproj", "Kiss.Bff/"]
RUN dotnet restore "Kiss.Bff/Kiss.Bff.csproj"
COPY . .
WORKDIR "/src/Kiss.Bff"
RUN dotnet build "Kiss.Bff.csproj" -c Release -o /app/build

FROM node:18-alpine AS frontend
WORKDIR /app
COPY . .
RUN npm ci
RUN npm run build --if-present

FROM build AS publish
COPY --from=frontend /app/dist ./wwwroot
RUN dotnet publish "Kiss.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/certs /usr/local/share/ca-certificates
RUN update-ca-certificates
USER kiss:kiss
ENTRYPOINT ["dotnet", "Kiss.Bff.dll"]