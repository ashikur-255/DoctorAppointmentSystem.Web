# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DoctorAppointmentSystem.Web/DoctorAppointmentSystem.Web.csproj", "DoctorAppointmentSystem.Web/"]
RUN dotnet restore "DoctorAppointmentSystem.Web/DoctorAppointmentSystem.Web.csproj"

COPY DoctorAppointmentSystem.Web/ DoctorAppointmentSystem.Web/
WORKDIR "/src/DoctorAppointmentSystem.Web"
RUN dotnet publish "DoctorAppointmentSystem.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DoctorAppointmentSystem.Web.dll"]
