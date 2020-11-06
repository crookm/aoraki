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

ENV JOURNALSETTINGS__DBNAME="aoraki"
ENV JOURNALSETTINGS__DBPOSTSCOLLECTION="posts"

EXPOSE 80

ENTRYPOINT ["dotnet", "Aoraki.Web.dll"]
