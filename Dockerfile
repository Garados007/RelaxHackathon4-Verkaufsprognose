FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder
WORKDIR /project
COPY src src
RUN dotnet publish --nologo -c Release -o /app \
        src/Verkaufsprognose/Verkaufsprognose.csproj

FROM mcr.microsoft.com/dotnet/sdk:7.0 as trainer
WORKDIR /project
COPY src src
COPY vendor/data vendor/data
WORKDIR /project/src
RUN dotnet run --project Tools/Trainer/Trainer.csproj -- \
        ../vendor/data

FROM mcr.microsoft.com/dotnet/runtime:7.0
EXPOSE 3000
COPY vendor/data data
WORKDIR /data
COPY --from=trainer /project/src/solution.json /app/training.json
COPY --from=builder /app /app
CMD [ "dotnet", "/app/Verkaufsprognose.dll", "/data", "/app/training.json" ]
