# Use the official .NET Core SDK image as a base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Set the working directory inside the container
WORKDIR /app

# Copy the project files to the container
COPY . .

# Build the project
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Set the working directory inside the container
WORKDIR /app

# Copy the published output from the build image
COPY --from=build-env /app/out .

# Expose the port your application will run on
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV DB_CONN_STRING=Server=pi41.home.lan,1433;Database=ClimateTrackr;User Id=sa;Password=qwer1234!;TrustServerCertificate=true
ENV RABBITMQ_CONN_STRING=amqp://guest:guest@pi41.home.lan:5672/
ENV RABBITMQ_EXCHANGE_NAME=climateTrackr_ex
ENV RABBITMQ_ROUTING_KEY=climateTrackrKey

# Run Entity Framework migrations and start the application
ENTRYPOINT ["dotnet", "ClimateTrackr-Server.dll"]
