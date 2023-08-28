# Use the official .NET 7 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project files and restore the dependencies
COPY ArtWebsiteAPI/*.csproj ./ArtWebsiteAPI/
COPY ArtWebsiteDataAccess/. ./ArtWebsiteDataAccess/
RUN dotnet restore ./ArtWebsiteAPI/

# Copy the rest of the files and build the project
COPY ArtWebsiteAPI/. ./ArtWebsiteAPI/
RUN dotnet publish ./ArtWebsiteAPI/ -c Release -o out

# Use the official .NET 7 runtime image as the final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Configure that we want development mode by setting the ASPNETCORE_ENVIRONMENT
# this needs to be done in the runtime stage.
ENV ASPNETCORE_ENVIRONMENT Development

# Set the working directory
WORKDIR /app

# Copy the .env files so they can be referenced by the projects in the app directory
COPY ./ArtWebsiteAPI/.env /app/.env

# Copy the output files from the build image
COPY --from=build /app/out .

# Expose port 80 for HTTP traffic
EXPOSE 80

# Run the web API when the container starts
ENTRYPOINT ["dotnet", "ArtWebsiteAPI.dll"]
