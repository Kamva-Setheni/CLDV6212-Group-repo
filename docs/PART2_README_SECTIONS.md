## Part 2 Docker Compose

Part 2 uses the two services declared in `docker-compose.yml`: `coffeenchill-azurite` and the
Functions host. The Compose file intentionally uses a published image reference rather than a
`build:` entry, as required by the brief:

```powershell
# Hlogi builds the current source before publishing this exact v2.0 tag.
docker build -t heistherealhlogi/coffeenchill-functions:v2.0 .
docker compose config
docker compose up -d
docker compose ps
docker compose logs -f functions
```

For a clean-machine test after the image has been published, use:

```powershell
docker compose down -v
docker compose pull
docker compose up -d
```

The Functions service receives a container-safe `AzureWebJobsStorage` connection string whose
Blob, Queue, and Table endpoints refer to `coffeenchill-azurite`, not `localhost`. The named
`azurite-data` volume preserves emulator data between restarts, and `coffeenchill-net` is a custom
bridge network. For local emulator compatibility, the Azurite command includes
`--skipApiVersionCheck`; this affects only the development emulator, not the Functions image or a
real Azure Storage account.

## API reference

| Method | Route | Description |
|---|---|---|
| POST | `/api/menu` | Creates a menu item. |
| GET | `/api/menu` | Returns all menu items. |
| GET | `/api/menu/category/{category}` | Returns menu items in a category. |
| PUT | `/api/menu/{category}/{sku}` | Updates an existing menu item. |
| DELETE | `/api/menu/{category}/{sku}` | Deletes an existing menu item. |
| POST | `/api/documents/upload` | Uploads an allowed PDF, Word, PNG, or JPEG staff document. |
| GET | `/api/documents` | Lists staff documents in the Blob container. |
| GET | `/api/documents/download/{fileName}` | Downloads a staff document. |
| POST | `/api/orders/queue` | Validates and queues an order; returns `202 Accepted`. |
| GET | `/api/orders/{orderId}/status` | Returns the latest processed order status. |

Valid order sample:

```json
{
  "orderId": "ORD-2026-8801",
  "customerName": "Jane Smith",
  "selectedItemSKUs": ["COF-001", "PAS-104"],
  "totalPrice": 65.00,
  "orderTimestamp": "2026-10-12T10:15:00Z"
}
```

## Testing

The committed Postman collection uses the `{{baseUrl}}` variable and contains the Part 1 menu and
document tests plus Part 2 order queue requests. Run the menu/document collection in its saved
order. For the queue workflow, queue a valid order and poll its status until it is `Collected`.

- [Part 1 API test matrix](docs/API_TEST_MATRIX.md)
- [Part 2 queue test matrix](docs/PART2_TEST_MATRIX.md)
- [Lesego WP2 implementation and handoff notes](docs/PART2_LESEGO_WP2.md)

## Dockerfile

The Dockerfile uses a multi-stage build. The first stage restores and publishes the .NET project;
the second stage is the smaller official Azure Functions .NET isolated runtime image that runs the
published output on port `80`.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish CoffeeAndChill.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
WORKDIR /home/site/wwwroot
EXPOSE 80
COPY --from=build /app/publish .
```

The completed image is tagged as `heistherealhlogi/coffeenchill-functions:v2.0` for Part 2 Compose
use. Docker Hub publication is performed only by the authenticated account owner.

## Part 2 changelog

- Added Base64 queue producer endpoint: `POST /api/orders/queue`.
- Added `ProcessOrderQueue` to write Orders table entities and simulate the full lifecycle.
- Added `GET /api/orders/{orderId}/status` for HTTP verification of queue processing.
- Added poison-queue configuration: three retries, five-second visibility timeout, explicit Base64 encoding.
- Added a versioned-image Docker Compose file with a custom network and Azurite volume.
- Expanded the Postman collection and test documentation for Part 2.

## Team contributions

| Member | Part 2 work package | Responsibility |
|---|---|---|
| Joseph | WP1 | Queue producer contract and order submission workflow. |
| Lesego | WP2 | Queue trigger, Orders-table lifecycle, status endpoint, poison handling, and WP2 documentation. |
| Hlogi | WP3 and WP4 | Versioned Docker Hub image and Docker Compose integration. |
| Kamva | WP5 | Full Postman collection, README finalisation, and video demonstration. |

## AI use disclosure

AI coding assistance was used for planning, code-review support, and integration debugging. Team
members review, understand, test, and can explain the submitted implementation.

