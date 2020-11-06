# ---
# BUILD
# ---
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out

# ---
# RUNTIME
# ---
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app/out .

ENV JOURNALSETTINGS__DBCONNECTION="mongodb://127.0.0.1:27017"
ENV JOURNALSETTINGS__DBNAME="aoraki"
ENV JOURNALSETTINGS__DBPOSTSCOLLECTION="posts"

ENTRYPOINT ["dotnet", "Aoraki.Web.dll"]
