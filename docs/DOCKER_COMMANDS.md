# Docker Commands - Parts 1 and 2

Part 1 uses standalone containers only. Part 2 adds `docker-compose.yml` for the required
two-container orchestration. Use the Part 2 commands after the v2.0 image has been built and
published by the authenticated Docker Hub owner.

## 1. Create a shared network

```powershell
docker network create coffeenchill-net
```

## 2. Run Azurite for Table and Blob Storage

```powershell
docker run -d --name coffeenchill-azurite --network coffeenchill-net `
  -p 10000:10000 -p 10001:10001 -p 10002:10002 `
  mcr.microsoft.com/azure-storage/azurite
```

## 3. Build the Functions image

Run this from the repository root:

```powershell
docker build -t coffeenchill-functions:v1.0 .
```

## 4. Run the Functions container

The Part 1 addendum removes the connection string from the `docker run` command. The
Dockerfile provides the standard non-secret Azurite development-storage setting and points
to the `coffeenchill-azurite` container on this shared network. Do not use Azure cloud keys.

```powershell
docker run -d --name coffeenchill-functions --network coffeenchill-net -p 7071:80 `
  -e FUNCTIONS_WORKER_RUNTIME=dotnet-isolated `
  coffeenchill-functions:v1.0
```

## 5. Confirm the Functions host

```powershell
docker logs coffeenchill-functions
```

The host should list eight functions: five Menu functions and three Staff Document
functions. Test every route against `http://localhost:7071/api`.

> Important: the Part 1 addendum requires Azure Blob Storage, which Azurite emulates.
> The `staff-docs` Blob container is created automatically on the first permitted upload.

## 6. Publish the Part 1 images

```powershell
docker login
docker tag coffeenchill-functions:v1.0 heistherealhlogi/coffeenchill-functions:v1.0
docker push heistherealhlogi/coffeenchill-functions:v1.0

docker pull mcr.microsoft.com/azure-storage/azurite
docker tag mcr.microsoft.com/azure-storage/azurite heistherealhlogi/coffeenchill-azurite:v1.0
docker push heistherealhlogi/coffeenchill-azurite:v1.0
```

Only the authenticated Docker Hub owner should run the push commands.

## Part 2 - Compose orchestration

The Part 2 Compose file must reference a versioned Docker Hub image through `image:`, not a
local `build:` entry. It has a custom bridge network, a named Azurite data volume, and a
Functions-to-Azurite connection string that uses the `coffeenchill-azurite` service name. Its
Azurite command uses `--skipApiVersionCheck` so newer Azure SDK clients can be exercised against
the local emulator; that flag is not used with a real Azure Storage service.

```powershell
# Build and tag the exact image declared by docker-compose.yml.
docker build -t heistherealhlogi/coffeenchill-functions:v2.0 .

# The account owner may then publish it. Do not push unless you own the account.
docker push heistherealhlogi/coffeenchill-functions:v2.0

# Validate the configuration, pull images, and start the full Part 2 stack.
docker compose config
docker compose pull
docker compose up -d
docker compose ps
docker compose logs -f functions
```

For the final clean-state verification, remove the Compose containers and its test data before
pulling the public image again:

```powershell
docker compose down -v
docker compose pull
docker compose up -d
```

## 7. Pulling images on a clean machine

```powershell
docker pull heistherealhlogi/coffeenchill-functions:v1.0
docker pull heistherealhlogi/coffeenchill-azurite:v1.0
```

Repeat steps 1, 2, and 4, substituting the pulled image names.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| Table Storage is unavailable | Azurite is stopped or not attached to `coffeenchill-net` | Check `docker ps` and `docker network inspect coffeenchill-net`. |
| Staff document operation fails against Azurite | Blob service is stopped or the upload is not a permitted file type | Check `docker ps`, retry a PDF/Word/PNG/JPEG file, and inspect the Functions logs. |
| Wrong function count | A stale image or old scaffold function was used | Rebuild with `docker build -t coffeenchill-functions:v1.0 .`. |
| `CS5001` during build | The build stage lacks `/src` as its working directory | Confirm `WORKDIR /src` is present before the first `COPY`. |
