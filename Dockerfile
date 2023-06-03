FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder
WORKDIR /project
COPY . .
RUN dotnet publish --nologo -c Release -o /app \
        src/Verkaufsprognose/Verkaufsprognose.csproj

FROM mcr.microsoft.com/dotnet/runtime:7.0
EXPOSE 3000
COPY vendor/data/products.csv /data/
COPY vendor/data/storage_02.csv /data/
WORKDIR /data
COPY --from=builder /app /app
CMD [ "dotnet", "/app/Verkaufsprognose.dll", "/data" ]
