FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY app/bin/Release/netcoreapp3.1/publish/ app/

ENTRYPOINT ["dotnet", "app/myapp.dll"]