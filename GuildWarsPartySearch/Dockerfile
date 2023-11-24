FROM mcr.microsoft.com/dotnet/runtime:8.0

# Set the working directory inside the container
WORKDIR /app

EXPOSE 8080

# Copy the build output to the working directory
COPY ./bin/Release/net8.0/publish/ .

# Command to run the executable
ENTRYPOINT ["dotnet", "GuildWarsPartySearch.Server.dll"]