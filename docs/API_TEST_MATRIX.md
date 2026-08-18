\# Part 1 API Test Matrix



| ID | Area | Method | Route | Success check | Failure check | Owner |

|---|---|---|---|---|---|---|

| M01 | Menu | POST | `/api/menu` | 201 + JSON menu item | invalid JSON / duplicate id | Joseph |

| M02 | Menu | GET | `/api/menu` | 200 + JSON array | storage failure | Joseph |

| M03 | Menu | GET | `/api/menu/category/{category}` | 200 + filtered array | invalid category | Joseph |

| M04 | Menu | PUT | `/api/menu/{category}/{id}` | 200 + updated object | invalid body / missing item | Joseph |

| M05 | Menu | DELETE | `/api/menu/{category}/{id}` | 204 | missing item | Joseph |

| D01 | Documents | POST | `/api/documents/upload` | successful 2xx upload response | invalid upload | Kamva |

| D02 | Documents | GET | `/api/documents` | 200 + metadata array | storage failure | Kamva |

| D03 | Documents | GET | `/api/documents/download/{fileName}` | 200 + binary content | missing file 404 | Kamva |



\## Execution order



1\. Start the storage environment.

2\. Start the Functions API/container.

3\. Run M01 → M02 → M03 → M04 → M05.

4\. Run D01 → D02 → D03.

5\. Run the negative scenarios after the happy-path sequence.

6\. Export the final collection to `docs/PostmanCollection.json`.

