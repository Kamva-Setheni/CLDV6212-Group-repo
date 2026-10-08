# CoffeeNChill API Test Matrix

Run these tests against the final running Functions host. Keep the Collection Runner
result and the listed evidence only after the response has genuinely passed.

| ID | Owner | Request | Expected result | Evidence to provide |
|---|---|---|---|---|
| M01 | Joseph | `POST /api/menu` with a valid `category`, `sku`, name, description, price, and availability | `201 Created`; returned entity contains the submitted SKU and category | Request and response screenshot or Collection Runner pass |
| M02 | Joseph | Duplicate `POST /api/menu` for the same category and SKU | `409 Conflict` | Request and response screenshot or Collection Runner pass |
| M03 | Joseph | Invalid `POST /api/menu` | `400 Bad Request` with an `errors` array | Request and response screenshot or Collection Runner pass |
| M04 | Joseph | `GET /api/menu` | `200 OK` and a JSON array | Collection Runner pass |
| M05 | Joseph | `GET /api/menu/category/{category}` | `200 OK` and a filtered JSON array | Collection Runner pass |
| M06 | Joseph | `PUT /api/menu/{category}/{sku}` for an existing item | `200 OK` and updated item | Collection Runner pass |
| M07 | Joseph | `PUT /api/menu/{category}/DOES-NOT-EXIST` | `404 Not Found` | Collection Runner pass |
| M08 | Joseph | `DELETE /api/menu/{category}/{sku}`, then repeat it | First request `204 No Content`; second request `404 Not Found` | Collection Runner pass |
| D01 | Kamva | `POST /api/documents/upload` with an allowed file | `200 OK` and an uploaded `fileName` | Request/response screenshot and Blob container listing |
| D02 | Kamva | `POST /api/documents/upload` with no file or unsupported type | `400 Bad Request` | Request and response screenshot |
| D03 | Kamva | `GET /api/documents` | `200 OK`, metadata array with filename, size, and last-modified values | Collection Runner pass and Blob container listing |
| D04 | Kamva | `GET /api/documents/download/{fileName}` for uploaded file | `200 OK` and downloaded content | Collection Runner pass and downloaded file |
| D05 | Kamva | `GET /api/documents/download/does-not-exist.pdf` | `404 Not Found` | Collection Runner pass |

## Execution order

1. Start the required storage environment and Functions host.
2. Run M01, M02, M03, M04, M05, M06, M07, and M08 in that order.
3. Upload one permitted staff document, then run D02, D03, D04, and D05.
4. Save one full successful Postman Collection Runner result for final evidence.

## Part 2 order queue tests

| ID | Owner | Request | Expected result | Evidence to provide |
|---|---|---|---|---|
| Q01 | Lesego | `POST /api/orders/queue` with the documented valid order JSON | `202 Accepted`; response contains the order ID, queue name, and status URL | Postman response and Functions log showing queue receipt |
| Q02 | Lesego | Poll `GET /api/orders/{orderId}/status` after Q01 | `200 OK`; status moves through `Received`, `Preparing`, `Ready`, and `Collected` | Postman responses or Orders-table view |
| Q03 | Lesego | `GET /api/orders/ORD-DOES-NOT-EXIST/status` | `404 Not Found` with a JSON `error` | Postman response |
| Q04 | Lesego | Invalid `POST /api/orders/queue` with missing required fields | `400 Bad Request` with an `errors` array | Postman response |
| Q05 | Lesego | Place malformed JSON directly on `order-processing-queue` | Three logged delivery failures; message moves to `order-processing-queue-poison` | Functions log and Storage Explorer poison-queue view |

### Part 2 execution order

1. Build the v2.0 Functions image, then start the services using `docker compose up -d`.
2. Run Q01 and poll Q02 until the returned `status` is `Collected`.
3. Run Q03 and Q04.
4. Run Q05 last so the poison message remains visible for evidence.
5. Save the final Postman result, Functions logs, Orders-table entity, and poison-queue screenshot.
