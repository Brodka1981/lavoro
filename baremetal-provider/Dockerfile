FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
 
LABEL it.aruba.project.name=Aruba.CmpService.BaremetalProvider.Api

 
RUN apk add --update --no-cache \
    icu-libs \
    busybox-extras \
    curl \
    tcpdump \
    iputils \
    grep \
    net-tools \
    tzdata
WORKDIR /app
COPY "/src/Aruba.CmpService.BaremetalProvider.Api/bin/Debug/net8.0/" .
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_HTTP_PORTS=8080;9080
ENV Kestrel__EndpointDefaults__Protocols=Http1
ENV Observability__HttpProbingEndpointPort=9080
#ENV ASPNETCORE_URLS=http://*:80;http://*:8080
#ENV ObservabilityOptions__HttpProbingEndpointPort=8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
CMD ["dotnet", "Aruba.CmpService.BaremetalProvider.Api.dll"]
EXPOSE 9080/tcp
EXPOSE 8080/tcp
