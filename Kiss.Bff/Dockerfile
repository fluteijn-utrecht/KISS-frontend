#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
RUN addgroup --group kiss --gid 2000 \
    && adduser \    
    --uid 1000 \
    --gid 2000 \
    "kiss" 
RUN chown kiss:kiss  /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS certs
RUN apt-get update
RUN apt-get install unzip
WORKDIR /custom-certs
RUN wget https://developers.kvk.nl/cms/api/uploads/Private_Chain_api_kvk_nl.zip
RUN unzip -j Private_Chain_api_kvk_nl.zip

FROM certs AS restore
WORKDIR /src
COPY ["Kiss.Bff/Kiss.Bff.csproj", "Kiss.Bff/"]
RUN dotnet restore "Kiss.Bff/Kiss.Bff.csproj"
COPY ["Kiss.Bff.Test/Kiss.Bff.Test.csproj", "Kiss.Bff.Test/"]
RUN dotnet restore "Kiss.Bff.Test/Kiss.Bff.Test.csproj"

FROM restore as build
WORKDIR /src
COPY Kiss.Bff Kiss.Bff
WORKDIR "/src/Kiss.Bff"
RUN dotnet build "Kiss.Bff.csproj" -c Release --no-restore -o /app/build

FROM build as dotnettest
WORKDIR /src
COPY Kiss.Bff.Test Kiss.Bff.Test
WORKDIR "/src/Kiss.Bff.Test"
RUN dotnet build "Kiss.Bff.Test.csproj" -c Release --no-restore
RUN dotnet test -c Release --no-build --logger trx

FROM node:18-alpine AS npmci
WORKDIR /app
COPY package-lock.json package-lock.json
COPY package.json package.json
RUN npm ci

FROM npmci as frontend
COPY . .
RUN npm run test:unitci
RUN npm run build --if-present

FROM alpine as testresults
COPY --from=dotnettest /src/Kiss.Bff.Test/TestResults/*.trx results/
CMD cp -r results/* out

FROM build AS publish
WORKDIR "/src/Kiss.Bff"
COPY --from=frontend /app/dist ./wwwroot
RUN dotnet publish "Kiss.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /custom-certs /usr/local/share/ca-certificates
RUN update-ca-certificates
USER kiss:kiss
ENTRYPOINT ["dotnet", "Kiss.Bff.dll"]