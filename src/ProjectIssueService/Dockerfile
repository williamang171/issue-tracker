FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80

# copy all .csproj files and restore as distinct layers.  Use the same COPY
# for every dockerfile in the project to take advantage of Docker caching
COPY IssueTracker.sln IssueTracker.sln
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/ProjectIssueService/ProjectIssueService.csproj src/ProjectIssueService/ProjectIssueService.csproj

# Restore package deps
RUN dotnet restore IssueTracker.sln

# Copy the app folders over
COPY src/ProjectIssueService src/ProjectIssueService
WORKDIR /app/src/ProjectIssueService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "ProjectIssueService.dll" ]