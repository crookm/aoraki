FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app

COPY . ./
RUN dotnet restore Aoraki.sln

FROM base AS build
RUN dotnet build --no-restore -c Release

FROM build AS test
RUN dotnet test --no-build -c Release --verbosity minimal

FROM build AS publish
RUN dotnet publish --no-build src/Aoraki.Web/Aoraki.Web.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS run
WORKDIR /app

COPY --from=publish /app/out .

EXPOSE 80

ENTRYPOINT ["dotnet", "Aoraki.Web.dll"]