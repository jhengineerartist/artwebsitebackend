# Use the official .NET 7 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project files and restore the dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the files and build the project
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET 7 runtime image as the final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the output files from the build image
COPY --from=build /app/out .

# Expose port 80 for HTTP traffic
EXPOSE 80

# Run the web API when the container starts
ENTRYPOINT ["dotnet", "artwebsitebackend.dll"]
