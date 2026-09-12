# Docker Commands - Part 1 (standalone containers, no Compose)

Part 1 uses standalone containers only. Do not add a `docker-compose.yml` file.

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

## 6. Publish the required images

```powershell
docker login
docker tag coffeenchill-functions:v1.0 lesegokhaole/coffeenchill-functions:v1.0
docker push lesegokhaole/coffeenchill-functions:v1.0

docker pull mcr.microsoft.com/azure-storage/azurite
docker tag mcr.microsoft.com/azure-storage/azurite lesegokhaole/coffeenchill-azurite:v1.0
docker push lesegokhaole/coffeenchill-azurite:v1.0
```

Only the authenticated Docker Hub owner should run the push commands.

## 7. Pulling images on a clean machine

```powershell
docker pull lesegokhaole/coffeenchill-functions:v1.0
docker pull lesegokhaole/coffeenchill-azurite:v1.0
```

Repeat steps 1, 2, and 4, substituting the pulled image names.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| Table Storage is unavailable | Azurite is stopped or not attached to `coffeenchill-net` | Check `docker ps` and `docker network inspect coffeenchill-net`. |
| Staff document operation fails against Azurite | Blob service is stopped or the upload is not a permitted file type | Check `docker ps`, retry a PDF/Word/PNG/JPEG file, and inspect the Functions logs. |
| Wrong function count | A stale image or old scaffold function was used | Rebuild with `docker build -t coffeenchill-functions:v1.0 .`. |
| `CS5001` during build | The build stage lacks `/src` as its working directory | Confirm `WORKDIR /src` is present before the first `COPY`. |
