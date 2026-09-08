# ☕ Coffee N Chill — Docker & Containerization Setup

> **Docker setup guide for the Coffee N Chill project**  
> Prepared by **Person C** · Docker Hub: `heistherealhlogi`

---

## 📦 Docker Hub Images

The project images are available on Docker Hub:

- **Functions API:** `heistherealhlogi/coffeenchill-functions:v1.0`
- **Azurite:** `heistherealhlogi/coffeenchill-azurite:v1.0`

Both images are tagged **`v1.0`** and publicly available.

---

## 🏗️ Architecture Overview

The Docker setup uses two containers connected through a shared Docker network:

```text
┌───────────────────────────────┐
│     Coffee N Chill API       │
│  Azure Functions / .NET       │
│        Port: 7071             │
└───────────────┬───────────────┘
                │
                │ coffeenchill-net
                │
┌───────────────▼───────────────┐
│          Azurite              │
│     Azure Storage Emulator    │
│ Ports: 10000 / 10001 / 10002  │
└───────────────────────────────┘
```

---

# 🚀 Setup & Run Instructions

## 1. Create the Docker Network

Create the shared network that allows the Functions container to communicate with Azurite:

```powershell
docker network create coffeenchill-net
```

> **Note:** If the network already exists, Docker will report that it already exists. You can continue with the next step.

---

## 2. Run Azurite

Start the Azure Storage emulator:

```powershell
docker run -d `
  --name coffeenchill-azurite `
  --network coffeenchill-net `
  -p 10000:10000 `
  -p 10001:10001 `
  -p 10002:10002 `
  heistherealhlogi/coffeenchill-azurite:v1.0
```

Azurite provides the local Azure Storage services required by the Functions application.

| Service | Port |
|---|---:|
| Blob | `10000` |
| Queue | `10001` |
| Table | `10002` |

---

## 3. Build the Functions Image

From the **repository root**, where the `Dockerfile` is located:

```powershell
docker build -t coffeenchill-functions:v1.0 .
```

This creates the local Docker image used to run the Azure Functions application.

---

## 4. Run the Functions Container

Run the Functions container on the same Docker network as Azurite:

```powershell
docker run --rm `
  --name coffeenchill-functions `
  --network coffeenchill-net `
  -p 7071:80 `
  -e FUNCTIONS_WORKER_RUNTIME=dotnet-isolated `
  -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://coffeenchill-azurite:10000/devstoreaccount1;QueueEndpoint=http://coffeenchill-azurite:10001/devstoreaccount1;TableEndpoint=http://coffeenchill-azurite:10002/devstoreaccount1;" `
  coffeenchill-functions:v1.0
```

### ✅ Successful Startup

The container is working correctly when the logs show:

```text
9 functions loaded
```

The logs should also list the application's routes, including:

- Menu items CRUD
- Staff documents

---

# 🐳 Pulling the Images from Docker Hub

For **markers, demonstrations, or setting up the project on a fresh machine**, the images can be pulled directly from Docker Hub.

### Pull Azurite

```powershell
docker pull heistherealhlogi/coffeenchill-azurite:v1.0
```

### Pull the Functions API

```powershell
docker pull heistherealhlogi/coffeenchill-functions:v1.0
```

After pulling the images, run the containers using the commands described in **Steps 1 and 4**.

---

# 🛠️ Important Fixes Made

During the Docker setup, several issues were identified and resolved.

### 1. Missing `WORKDIR /src`

The Dockerfile was missing:

```dockerfile
WORKDIR /src
```

in the build stage.

This caused build files to collide with the container's root filesystem and resulted in a misleading `CS5001` error.

**Fix:** Added `WORKDIR /src` immediately after the first `FROM` instruction.

---

### 2. Missing `builder.Build().Run();`

`Program.cs` was missing:

```csharp
builder.Build().Run();
```

The application could therefore build successfully but would crash during startup and report:

```text
0 functions found
```

**Fix:** Added the missing startup statement.

---

### 3. Duplicate Project Files

Leftover duplicate project files containing `- Copy` variants were being reintroduced during a branch merge.

**Fix:** Removed the duplicate files to keep the project structure clean and prevent conflicts.

---

# 🔍 Quick Verification

Use the following checklist before demonstrating the Docker setup:

- [ ] Docker Desktop is running.
- [ ] `coffeenchill-net` exists.
- [ ] Azurite is running.
- [ ] The Functions image builds successfully.
- [ ] The Functions container starts without crashing.
- [ ] Logs show **9 functions loaded**.
- [ ] Menu item CRUD routes are available.
- [ ] Staff-docs routes are available.
- [ ] Both Docker images can be pulled from Docker Hub.

---

## 📋 Useful Commands

### View running containers

```powershell
docker ps
```

### View all containers

```powershell
docker ps -a
```

### View Functions logs

```powershell
docker logs coffeenchill-functions
```

### View Azurite logs

```powershell
docker logs coffeenchill-azurite
```

### Stop Azurite

```powershell
docker stop coffeenchill-azurite
```

### Remove Azurite

```powershell
docker rm coffeenchill-azurite
```

---

## 👥 For Group Members

If you are setting up the project on a new machine:

1. Clone the repository.
2. Make sure Docker Desktop is installed and running.
3. Create the `coffeenchill-net` network.
4. Pull the required images from Docker Hub.
5. Start Azurite.
6. Start the Functions container.
7. Check the logs for **`9 functions loaded`**.

> **Tip:** You do not need to rebuild the Functions image if you are using the published `v1.0` Docker Hub image.

---

## 📌 Project Docker Information

| Component | Image | Version |
|---|---|---|
| Azure Functions | `heistherealhlogi/coffeenchill-functions` | `v1.0` |
| Azurite | `heistherealhlogi/coffeenchill-azurite` | `v1.0` |
| Network | `coffeenchill-net` | — |
| Functions Port | `7071` | — |
| Azurite Blob Port | `10000` | — |
| Azurite Queue Port | `10001` | — |
| Azurite Table Port | `10002` | — |

---

### ☕ Coffee N Chill

**Dockerized Azure Functions + Azurite setup**

*Keep this README updated whenever Docker images, ports, container names, or startup requirements change.*
