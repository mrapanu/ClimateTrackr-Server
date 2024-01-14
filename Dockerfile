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
ENV DB_CONN_STRING=<your_db_conn_string>
ENV RABBITMQ_CONN_STRING=<Example:amqp://guest:guest@localhost:5672/>
ENV RABBITMQ_EXCHANGE_NAME=<exchange_name>
ENV RABBITMQ_ROUTING_KEY=<your_routing_key>
ENV JWT_SECRET_TOKEN=<secret_token_min_16_chars>

# Run Entity Framework migrations and start the application
ENTRYPOINT ["dotnet", "ClimateTrackr-Server.dll"]
