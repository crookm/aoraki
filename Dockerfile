# ---
# BUILD
# ---
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# This will be exposed via logs, but this is only used inside the VM in production
ARG CertificatePassword

COPY . ./
RUN dotnet restore
RUN dotnet publish --nologo -c Release -o out --no-restore
RUN dotnet test --nologo -c Release --no-build

RUN dotnet dev-certs https -ep /aoraki.pfx -p $CertificatePassword

# ---
# RUNTIME
# ---
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out .
COPY --from=build /aoraki.pfx /https/aoraki.pfx

ARG CertificatePassword

ENV ASPNETCORE_ENVIRONMENT="production"
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aoraki.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=$CertificatePassword
ENV JOURNALSETTINGS__DBNAME="aoraki"
ENV JOURNALSETTINGS__DBPOSTSCOLLECTION="posts"
ENV APPINSIGHTS_INSTRUMENTATIONKEY="aa859422-2927-407b-b221-0084262458d1"

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Aoraki.Web.dll"]
