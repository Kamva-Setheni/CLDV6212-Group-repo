# Part 2 Queue and Order Test Matrix

Run these tests after the existing Part 1 collection. Use the Compose stack or a local Functions
host connected to the same Azurite instance.

| ID | Request or action | Expected result |
|---|---|---|
| Q01 | `POST /api/orders/queue` with a valid sample order | `202 Accepted`, order ID and status URL returned. |
| Q02 | Poll `GET /api/orders/ORD-2026-8801/status` | `200 OK`; status progresses from Received to Collected and `lastUpdated` changes. |
| Q03 | `GET /api/orders/ORD-DOES-NOT-EXIST/status` | `404 Not Found` with a JSON `error`. |
| Q04 | `POST /api/orders/queue` with a missing customer, items, price, or date | `400 Bad Request` with an `errors` array. |
| Q05 | Send malformed JSON to `order-processing-queue` | Three logged failures, then one message in `order-processing-queue-poison`. |
| Q06 | Submit the same valid order ID again | No duplicate Orders-table entity; the same row is safely reset and processed again. |

For video evidence, show the `Orders` table with the final `Collected` entity and the poison queue
with one retained message. Do not add a poison-queue trigger because it would consume the proof.
