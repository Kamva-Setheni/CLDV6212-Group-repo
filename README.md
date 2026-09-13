# CoffeeNChill Canteen Management System

CoffeeNChill is a cloud-enabled canteen-management microservices project for CLDV6212
Cloud Development B. This Part 1 deliverable provides a .NET 8 Azure Functions API for
managing menu items in Azure Table Storage and staff documents in Azure Blob Storage. The
system will be extended incrementally in Parts 2 and 3.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Local Setup and Run](#local-setup-and-run)
- [API Reference](#api-reference)
- [Docker](#docker)
- [Testing](#testing)
- [Evidence Screenshots](#evidence-screenshots)
- [Repository Structure](#repository-structure)
- [Team and Contributions](#team-and-contributions)
- [AI Use Disclosure](#ai-use-disclosure)
- [Demo Video](#demo-video)
- [Known Limitations and Part 2 Preview](#known-limitations-and-part-2-preview)

## Architecture Overview

Clients use HTTP endpoints exposed by Azure Functions. Menu records are stored in the
`MenuItems` Azure Table, while staff-document metadata and content are managed through
the `staff-docs` Azure Blob container. Docker packages the Functions host; Azurite supplies
local Table and Blob Storage emulation for the complete Part 1 workflow.

```text
Postman / Web Client
        |
        v
Azure Functions HTTP API (.NET 8)
   |                         |
   v                         v
MenuItems Azure Table      staff-docs Blob container
   |                         |
   +------ Azurite (local Table and Blob emulator) ------+
                              |
                           Docker
```

## Prerequisites

- .NET SDK 8.0
- Azure Functions Core Tools v4
- Docker Desktop
- Azurite, through Docker or npm, for local Table and Blob Storage emulation
- Postman

## Local Setup and Run

1. Clone the repository and open the `CoffeeAndChill` project folder.
2. Copy `local.settings.template.json` to `local.settings.json`. The real local settings
   file is ignored by Git and must not be committed.
3. Restore and build:

   ```powershell
   dotnet restore
   dotnet build
   ```

4. Run Azurite for Table and Blob Storage:

   ```powershell
   docker run --rm --name coffeenchill-azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
   ```

5. Start the Functions app:

   ```powershell
   func start
   ```

6. Import `docs/PostmanCollection.json` into Postman and run it against
   `http://localhost:7071/api`.

`UseDevelopmentStorage=true` lets every Part 1 endpoint use local Azurite. The official
Part 1 addendum requires Azure Blob Storage rather than Azure File Shares because Azurite
emulates Blob Storage locally. No Azure subscription or real cloud connection string is
required for the Part 1 test workflow.

## API Reference

| Method | Route | Description | Sample body |
|---|---|---|---|
| POST | `/api/menu` | Creates a menu item. | `{ "category": "Hot Drinks", "sku": "COF-001", "name": "Espresso", "description": "Single shot espresso", "price": 25.00, "isAvailable": true }` |
| GET | `/api/menu` | Returns all menu items as an array. | - |
| GET | `/api/menu/category/{category}` | Returns items in a category as an array. | - |
| PUT | `/api/menu/{category}/{sku}` | Updates an existing menu item. | `{ "name": "Double Espresso", "description": "Two shots", "price": 35.00, "isAvailable": true }` |
| DELETE | `/api/menu/{category}/{sku}` | Deletes a menu item. | - |
| POST | `/api/documents/upload` | Uploads an allowed PDF, Word, PNG, or JPEG staff document. | `multipart/form-data`, field `file` |
| GET | `/api/documents` | Lists staff documents. | - |
| GET | `/api/documents/download/{fileName}` | Downloads a staff document. | - |

## Docker

Part 1 uses two standalone containers connected through `coffeenchill-net`; Docker
Compose is intentionally not used. See [docs/DOCKER_COMMANDS.md](docs/DOCKER_COMMANDS.md)
for the complete build, run, publish, pull, and troubleshooting commands.

The final public image repositories will be [Functions](https://hub.docker.com/r/lesegokhaole/coffeenchill-functions)
and [Azurite](https://hub.docker.com/r/lesegokhaole/coffeenchill-azurite).

## Testing

Import [docs/PostmanCollection.json](docs/PostmanCollection.json) into Postman. Its
collection-level `baseUrl` variable defaults to `http://localhost:7071/api`; change it
only if your host and port differ. The collection exercises all eight endpoints with the
specified success and failure scenarios. Attach a small permitted document to the upload
success request, then set the matching file name in the download-success request.

Use [docs/API_TEST_MATRIX.md](docs/API_TEST_MATRIX.md) for the ordered live-test plan and
[docs/PART1_QA_EVIDENCE.md](docs/PART1_QA_EVIDENCE.md) to record the proof needed before
submission.

## Evidence Screenshots

The screenshots below are final-state proof that the standalone Docker environment, the
Azurite integration, and the Postman test suite all work end to end. They are supporting
evidence for the repository and the demo video; they do not replace the video or the
Docker Hub links, which remain the primary required proof for submission. The image files
live in [docs/evidence/](docs/evidence/).

### 1. Standalone Docker containers running

![Docker Desktop showing coffeenchill-azurite and coffeenchill-functions running as separate containers](docs/evidence/01_Docker_Standalone_Containers_Running.png)

Docker Desktop showing the two containers running independently, `coffeenchill-azurite`
and `coffeenchill-functions`, with no Docker Compose involved. The Functions `7071:80`
port mapping and the Azurite port bindings are both visible.

### 2. Azurite and Functions Docker logs

![Docker logs showing the Functions container calling ListStaffDocuments and Azurite receiving the storage operation](docs/evidence/02_Azurite_and_Functions_Docker_Logs.png)

Docker logs showing `coffeenchill-functions` handling a `ListStaffDocuments` request while
`coffeenchill-azurite` receives the matching storage operations, confirming that the
Functions API is actually talking to local Azurite Blob Storage rather than mocking it.

### 3. Full Postman collection passing

![Postman Collection Runner reporting all 14 tests passed](docs/evidence/03_Postman_All_14_Tests_Passed.png)

The Postman Collection Runner reporting **All 14, Passed 14, Failed 0, Skipped 0, Errors
0**, confirming that the committed collection covers every required Menu and Staff
Document endpoint and that all saved tests pass together in one run.

### 4. Document list returned from Azurite Blob Storage

![GET /api/documents returning 200 OK and listing staff-document.pdf](docs/evidence/04_Azurite_Blob_Document_List_200_OK.png)

`GET /api/documents` returning `200 OK` and listing `staff-document.pdf` with its size and
last-modified time, confirming the document was stored and retrieved correctly through the
Azure Blob Storage implementation.

### 5. Staff document workflow passing

![Postman runner showing the upload error case, listing, download, and missing-file case all passing](docs/evidence/05_Postman_Document_Workflow_Passed.png)

The runner covering the Staff Documents workflow end to end: the no-file upload error
case, the document listing, a successful download, and the missing-file `404` response —
all passing.

## Repository Structure

```text
CoffeeAndChill/
|- Functions/
|  |- Menu/
|  |  |- CreateMenuItemsFunction.cs
|  |  |- GetAllMenuItemsFunction.cs
|  |  |- GetMenuItemsByCategoryFunction.cs
|  |  |- UpdateMenuItemsFunction.cs
|  |  `- DeleteMenuItemsFunction.cs
|  `- Documents/
|     |- UploadStaffDocument.cs
|     |- ListStaffDocuments.cs
|     `- DownloadStaffDocument.cs
|- Models/
|- DTOs/
|- Interfaces/
|- Services/
|- docs/
|  |- PostmanCollection.json
|  |- DOCKER_COMMANDS.md
|  `- evidence/
|     |- 01_Docker_Standalone_Containers_Running.png
|     |- 02_Azurite_and_Functions_Docker_Logs.png
|     |- 03_Postman_All_14_Tests_Passed.png
|     |- 04_Azurite_Blob_Document_List_200_OK.png
|     `- 05_Postman_Document_Workflow_Passed.png
|- Dockerfile
|- .dockerignore
|- .gitignore
|- host.json
|- local.settings.template.json
|- CoffeeAndChill.csproj
`- README.md
```

## Team and Contributions

| Name | Student Number | Part 1 Role | What they owned |
|---|---|---|---|
| Lesego Khaole | ST10455441 | Project Manager, QA, Documentation & Delivery | Postman collection and test coverage, `/docs`, README, final integration testing, final corrections to complete code/project structure, and evidence coordination. |
| Happy Van Der Merwe (Joseph) | ST10482408 | Data & Menu | `Functions/Menu/`, the `MenuItems` model, Azure Table Storage service, and all five Menu HTTP Functions. |
| Kamva Setheni | ST10447340 | Documents | `Functions/Documents/`, `staff-docs` Azure Blob Storage integration, and all three Document HTTP Functions. |
| Lehlogonolo Ntshabeleng (Hlogi) | ST10474988 | Containers & Infrastructure | Dockerfile, `.dockerignore`, Docker networking, and Docker Hub publishing. |

Lesego Khaole leads the Part 1 QA, documentation, Postman coverage, and integration
evidence. Happy Van Der Merwe (Joseph) owns the Menu data model, storage layer, and Menu
functions. Kamva Setheni owns staff-document storage and its API endpoints. Lehlogonolo
Ntshabeleng (Hlogi) owns the container workflow and Docker Hub images.




## Demo Video

**Video:** [Unlisted YouTube link - to be added before submission]

## Known Limitations and Part 2 Preview

Part 1 intentionally excludes Docker Compose, queues, and authentication. The official
Part 1 addendum replaces Azure File Shares with Azure Blob Storage, allowing all Part 1
document tests to run locally through Azurite. Part 2 and Part 3 will introduce the
remaining distributed workflows and security capabilities.


# Harvard Reference List — CoffeeNChill CLDV6212 POE Part 1

This document lists every external source, tool, package, and assistance used during the
design, implementation, testing, and documentation of CoffeeNChill Canteen Management
System — Part 1 (Azure Functions, Tables, Blob Storage and Docker Hub).

---

## Book

Mrzygłód, K. (2022) *Azure for Developers: Implement rich Azure PaaS ecosystems using
containers, serverless services, and storage solutions*. 2nd edn. Birmingham: Packt
Publishing.

---

## Module and Assessment Documents (CLDV6212)

The Independent Institute of Education (IIE) (2026a) *CLDV6212 Cloud Development B —
Portfolio of Evidence (PoE): Part 1, Part 2 and Part 3*. Module brief. South Africa: The
Independent Institute of Education.

The Independent Institute of Education (IIE) (2026b) *CLDV6212 PoE Addendum — Part 1
update: use Azure Blob Storage instead of Azure File Storage*. Module addendum. South
Africa: The Independent Institute of Education.

The Independent Institute of Education (IIE) (2026c) *CLDV6212 PoE Group Structure —
ownership, repository map and workflow*. Module document. South Africa: The Independent
Institute of Education.

---

## Group Planning Documents

CoffeeNChill Group (2026a) *CoffeeNChill Group Project Plan — operating manual,
responsibilities and integration workflow for Parts 1–3*. Unpublished internal planning
document. CLDV6212 Group Project.

CoffeeNChill Group (2026b) *CoffeeAndChill Part 1 —  status and technical handover*.
Unpublished internal review document. CLDV6212 Group Project.

---

## Microsoft Official Documentation

Microsoft (2026a) *Azure Functions documentation*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/ (Accessed: 10 September 2026).

Microsoft (2026b) *Guide for running C# Azure Functions in the isolated worker process*.
Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide
(Accessed: 10 September 2026).

Microsoft (2026c) *Azure Functions triggers and bindings concepts*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings
(Accessed: 10 September 2026).

Microsoft (2026d) *Azure Functions Core Tools reference*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local
(Accessed: 10 September 2026).

Microsoft (2026e) *Azure Table Storage documentation*. Available at:
https://learn.microsoft.com/en-us/azure/storage/tables/ (Accessed: 10 September 2026).

Microsoft (2026f) *Understanding the Table service data model — PartitionKey, RowKey and
properties*. Available at:
https://learn.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model
(Accessed: 10 September 2026).

Microsoft (2026g) *Azure Blob Storage documentation*. Available at:
https://learn.microsoft.com/en-us/azure/storage/blobs/ (Accessed: 10 September 2026).

Microsoft (2026h) *BlobContainerClient class — Azure SDK for .NET*. Available at:
https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient
(Accessed: 10 September 2026).

Microsoft (2026i) *BlobClient.UploadAsync method — Azure SDK for .NET*. Available at:
https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.uploadasync
(Accessed: 10 September 2026).

Microsoft (2026j) *Azurite open-source emulator for local Azure Storage development*.
Available at: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite
(Accessed: 10 September 2026).

Microsoft (2026k) *Azurite on Docker Hub — mcr.microsoft.com/azure-storage/azurite*.
Available at: https://mcr.microsoft.com/en-us/artifact/mar/azure-storage/azurite
(Accessed: 10 September 2026).

Microsoft (2026l) *Linux container support in Azure Functions*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/functions-container-overview
(Accessed: 10 September 2026).

Microsoft (2026m) *.NET 8 SDK downloads and release notes*. Available at:
https://dotnet.microsoft.com/en-us/download/dotnet/8.0 (Accessed: 10 September 2026).

Microsoft (2026n) *Docker Desktop for Windows — install and configure*. Available at:
https://docs.docker.com/desktop/install/windows-install/ (Accessed: 10 September 2026).

Microsoft (2026o) *host.json reference for Azure Functions 2.x and later*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/functions-host-json
(Accessed: 10 September 2026).

Microsoft (2026p) *Application Insights for Azure Functions monitoring*. Available at:
https://learn.microsoft.com/en-us/azure/azure-functions/functions-monitoring
(Accessed: 10 September 2026).

Microsoft (2026q) *Azure Storage connection strings — configure and use*. Available at:
https://learn.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string
(Accessed: 10 September 2026).

Microsoft (2026r) *Use the Azurite emulator for local Azure Storage development*.
Available at:
https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite
(Accessed: 10 September 2026).

---

## NuGet Packages

NuGet (2026a) *Azure.Data.Tables 12.11.0*. Available at:
https://www.nuget.org/packages/Azure.Data.Tables/12.11.0 (Accessed: 10 September 2026).

NuGet (2026b) *Azure.Storage.Blobs 12.29.2*. Available at:
https://www.nuget.org/packages/Azure.Storage.Blobs/12.29.2 (Accessed: 10 September 2026).

NuGet (2026c) *Azure.Storage.Files.Shares 12.27.1 (retained for reference — replaced by
Azure.Storage.Blobs after the Part 1 addendum)*. Available at:
https://www.nuget.org/packages/Azure.Storage.Files.Shares/12.27.1
(Accessed: 10 September 2026).

NuGet (2026d) *Microsoft.Azure.Functions.Worker 2.52.0*. Available at:
https://www.nuget.org/packages/Microsoft.Azure.Functions.Worker/2.52.0
(Accessed: 10 September 2026).

NuGet (2026e) *Microsoft.Azure.Functions.Worker.Sdk 2.0.7*. Available at:
https://www.nuget.org/packages/Microsoft.Azure.Functions.Worker.Sdk/2.0.7
(Accessed: 10 September 2026).

NuGet (2026f) *Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore 2.1.0*.
Available at:
https://www.nuget.org/packages/Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore/2.1.0
(Accessed: 10 September 2026).

NuGet (2026g) *Microsoft.Azure.Functions.Worker.OpenTelemetry 1.2.0*. Available at:
https://www.nuget.org/packages/Microsoft.Azure.Functions.Worker.OpenTelemetry/1.2.0
(Accessed: 10 September 2026).

NuGet (2026h) *Azure.Monitor.OpenTelemetry.Exporter 1.7.0*. Available at:
https://www.nuget.org/packages/Azure.Monitor.OpenTelemetry.Exporter/1.7.0
(Accessed: 10 September 2026).

NuGet (2026i) *OpenTelemetry.Extensions.Hosting 1.15.3*. Available at:
https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting/1.15.3
(Accessed: 10 September 2026).

---

## Mapping of Each Reference to the Code It Supported

| Reference | What It Supported in the Code |
|---|---|
| Mrzygłód (2022, pp. 107–130) | Azure service selection rationale: Functions, Table Storage, Blob Storage and Docker |
| Mrzygłód (2022, pp. 245–253) | Azure SQL/Table comparison and connection-string format as background for Menu storage |
| Mrzygłód (2022, pp. 488–494) | Azure Blob Storage concepts applied to `DocumentStorageService` after the addendum |
| Mrzygłód (2022, p. 475) | Azurite connection-string format used for local development |
| IIE (2026a) | Part 1 scope, endpoint contract, rubric and video/commit requirements |
| IIE (2026b) | Addendum decision to use Azure Blob Storage instead of Azure File Share for `staff-docs` |
| IIE (2026c) | Ownership map, branch workflow and integration sequence |
| Microsoft (2026a) | Azure Functions overview and version model |
| Microsoft (2026b) | .NET isolated worker model and `FunctionsApplication.CreateBuilder` in `Program.cs` |
| Microsoft (2026c) | HTTP trigger routing for the eight endpoints |
| Microsoft (2026d) | `func start` / `func --version` local runtime and Core Tools installation |
| Microsoft (2026e) | `TableClient`, table creation and query operations in `TableStorageService` |
| Microsoft (2026f) | `MenuItems` entity with `PartitionKey = Category`, `RowKey = SKU` |
| Microsoft (2026g) | Azure Blob Storage concepts and container metadata |
| Microsoft (2026h) | `BlobContainerClient` used in the updated `DocumentStorageService` |
| Microsoft (2026i) | `UploadAsync`, `DownloadAsync` and `GetPropertiesAsync` calls in the document service |
| Microsoft (2026j) | Azurite setup, storage accounts and default ports |
| Microsoft (2026k) | `mcr.microsoft.com/azure-storage/azurite` container image used in `DOCKER_COMMANDS.md` |
| Microsoft (2026l) | Multi-stage Dockerfile using the isolated-worker base image |
| Microsoft (2026m) | .NET 8 SDK installation and TargetFramework `net8.0` in the `.csproj` |
| Microsoft (2026n) | Docker Desktop installation and WSL2 backend requirements |
| Microsoft (2026o) | `host.json` configuration with `routePrefix: "api"` |
| Microsoft (2026p) | Optional Application Insights / OpenTelemetry wiring in `Program.cs` |
| Microsoft (2026q) | `AzureWebJobsStorage` connection-string format for Azurite and Azure |
| Microsoft (2026r) | Azurite port mapping (Blob 10000, Queue 10001, Table 10002) |
| NuGet (2026a) | `Azure.Data.Tables` 12.11.0 package reference in `CoffeeAndChill.csproj` |
| NuGet (2026b) | `Azure.Storage.Blobs` 12.29.2 added to support the post-addendum Blob implementation |
| NuGet (2026c) | Original `Azure.Storage.Files.Shares` implementation before the addendum |
| NuGet (2026d–e) | Isolated-worker hosting and SDK used to compile and run the project |
| NuGet (2026f) | HTTP routing middleware for Azure Functions |
| NuGet (2026g–i) | Optional telemetry exporter and hosting extensions in `Program.cs` |

---

## Tools Used

**Microsoft Visual Studio Code**
Primary IDE used for editing C#, JSON, Markdown and Docker files. The integrated
PowerShell terminal was used to run `dotnet build`, `dotnet run`, `docker ps`,
`docker start`, and Core Tools commands.

**Microsoft Visual Studio 2026 Developer PowerShell**
Used to run the Azure Functions host locally, restore NuGet packages, and manage the
development environment.

**Microsoft PowerShell 5.x / PowerShell 7**
Used for `docker` commands, `netstat`, `taskkill`, `winget` and environment diagnostics
during development and troubleshooting.

**Docker Desktop for Windows**
Used to build and run the multi-stage Functions container image, run the Azurite
emulator container, manage the user-defined bridge network and publish images to Docker
Hub. Version used: 29.6.2.

**Docker Hub**
Used to publish versioned container images for the Functions host and for the Azurite
emulator, satisfying the Part 1 image-tagging requirement.

**Microsoft Azurite**
Official Azure Storage emulator used for local development of Table Storage and Blob
Storage. Runs in a standalone Docker container on ports 10000 (Blob), 10001 (Queue) and
10002 (Table). Image: `mcr.microsoft.com/azure-storage/azurite`.

**Microsoft Azure Functions Core Tools v4**
Used to start the Azure Functions host locally (`dotnet run` and `func start`) and to
discover/route the eight HTTP-triggered functions. Version used: 4.14.0.

**.NET 8 SDK**
Target framework for the entire solution. Installed alongside .NET 10 SDK so that the
project could be built and run with the correct runtime.

**Postman**
Used for API testing — the exported Postman collection (`docs/PostmanCollection.json`)
contains the Menu and Staff Documents folders, collection variables (`{{baseUrl}}`),
success assertions and failure scenarios required for Part 1 QA evidence.



**Markdown and GitHub README**
Used to document setup instructions, API reference, Docker commands, team
contributions, evidence register and the unlisted video link.

**Microsoft Azure Portal (referenced for verification)**
Used as the intended production target for Azure Table Storage, Azure Blob Storage,
Azure Functions and Azure File Share — the same services emulated locally by Azurite.

---

## AI Use Disclosure

AI coding assistants, were used for understanding error messages and debugging support, code
review suggestions, integration fixes, individual role assignment, responsibility clarifiction, and design well-structured Markdown content for the README.md file. 

---

## Summary of Where Each Source Was Most Useful

| Category | Primary Use in Project |
|---|---|
| Book (Mrzygłód, 2022) | Conceptual grounding in Azure Functions, Tables, Blobs, Azurite and Docker |
| Module documents (IIE, 2026a–c) | Locked the Part 1 scope, routes, addendum and ownership boundaries |
| Group plan (CoffeeNChill Group, 2026a–b) | Defined the workflow, branch strategy, hand-offs and evidence plan |
| Microsoft Learn (2026a–r) | Every implementation detail: SDK classes, ports, Dockerfile, routing, tables, blobs |
| NuGet (2026a–i) | The exact package versions used in `CoffeeAndChill.csproj` |
| Tools section | All local and cloud tooling actually used to build, run, test and submit Part 1 |


---

