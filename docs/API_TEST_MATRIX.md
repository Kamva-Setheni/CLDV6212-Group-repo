# Part 1 API Test Matrix

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
