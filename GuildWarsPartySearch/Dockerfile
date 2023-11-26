FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory inside the container
WORKDIR /app

EXPOSE 80 443

# Copy the build output to the working directory
COPY Publish/ .

# Command to run the executable
ENTRYPOINT ["dotnet", "GuildWarsPartySearch.Server.dll"]