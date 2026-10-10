# Part 2 WP1 Queue Producer (Joseph)

## Endpoint

POST /api/orders/queue (anonymous). Validates the order, sends it as Base64 JSON to
order-processing-queue and returns 202 Accepted straight away.

## Request body

| Field | Rules |
|---|---|
| orderId | Required, not blank. Trimmed. |
| customerName | Required, not blank. Trimmed. |
| selectedItemSKUs | Required, at least one SKU, no blank SKUs. |
| totalPrice | Required, greater than zero. |
| orderTimestamp | Optional. If missing, the server uses the current UTC time. |

## Responses

| Status | When |
|---|---|
| 202 Accepted | Order queued. Body has orderId, queue and statusUrl. |
| 400 Bad Request | Malformed JSON (error) or failed validation (errors list). |

## How it connects

The message is the shared Order model. ProcessOrderQueue (Lesego) reads it, writes it to the
Orders table and advances Received, Preparing, Ready, Collected. Status is read with
GET /api/orders/{orderId}/status.
