FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

# Example: 
# docker run -it -e AZSIMULATOR_MAPSKEY -e AZSIMULATOR_IDSCOPE -e AZSIMULATOR_SASTOKEN -e AZSIMULATOR_REGISTRATIONID tacowan/simulator:latest --from 47.608963,-122.340538 --to 47.586639,-122.376389
ENTRYPOINT ["dotnet", "simexercise.dll"]