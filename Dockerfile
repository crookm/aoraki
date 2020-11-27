# ---
# BUILD
# ---
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out

# ---
# RUNTIME
# ---
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out .

ENV JOURNALSETTINGS__DBNAME="aoraki"
ENV JOURNALSETTINGS__DBPOSTSCOLLECTION="posts"
ENV APPINSIGHTS_INSTRUMENTATIONKEY="aa859422-2927-407b-b221-0084262458d1"

EXPOSE 80

ENTRYPOINT ["dotnet", "Aoraki.Web.dll"]
