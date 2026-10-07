FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EventBooking.API/*.csproj EventBooking.API/
COPY EventBooking.Service/*.csproj EventBooking.Service/
COPY EventBooking.Data/*.csproj EventBooking.Data/
COPY EventBooking.Core/*.csproj EventBooking.Core/
RUN dotnet restore EventBooking.API/EventBooking.API.csproj

COPY EventBooking.API/ EventBooking.API/
COPY EventBooking.Service/ EventBooking.Service/
COPY EventBooking.Data/ EventBooking.Data/
COPY EventBooking.Core/ EventBooking.Core/
RUN dotnet publish EventBooking.API/EventBooking.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EventBooking.API.dll"]
