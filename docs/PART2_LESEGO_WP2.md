# Part 2 WP2 Order Queue Processing

## Scope completed by Lesego

This work package implements the Part 2 background-processing path. It includes the shared
`Order` contract and queue-producer endpoint only so the complete WP2 workflow can be run and
tested independently while the team integrates the final branches.

| Component | Route or trigger | Behaviour |
|---|---|---|
| Queue producer | `POST /api/orders/queue` | Validates an order and sends Base64 JSON to `order-processing-queue`, returning `202 Accepted`. |
| Queue processor | `ProcessOrderQueue` | Reads the queue message, writes it to the `Orders` table, and advances `Received -> Preparing -> Ready -> Collected`. |
| Status endpoint | `GET /api/orders/{orderId}/status` | Reads the latest order status by order ID; returns `200` or a JSON `404` error. |
| Poison handling | Functions queue settings | Retries malformed/incomplete messages three times, logs every failure, then lets the host move them to `order-processing-queue-poison`. |

## Message contract

Both the producer and consumer use Base64 queue encoding and the following JSON contract:

```json
{
  "orderId": "ORD-2026-8801",
  "customerName": "Jane Smith",
  "selectedItemSKUs": ["COF-001", "PAS-104"],
  "totalPrice": 65.00,
  "orderTimestamp": "2026-10-12T10:15:00Z"
}
```

The Orders table uses the UTC order date (`yyyy-MM-dd`) as `PartitionKey` and `OrderId` as
`RowKey`. Re-delivery is safe because `UpsertEntityAsync` overwrites the same entity instead
of creating duplicate rows.

## Run and verify locally

1. Start Azurite, then start the Functions host. The existing Part 1 setup still applies.
2. Send `POST /api/orders/queue` using the valid JSON above. It returns `202` immediately.
3. Poll `GET /api/orders/ORD-2026-8801/status` while processing occurs. The status begins at
   `Received` and advances through `Preparing`, `Ready`, and `Collected` at five-second intervals.
4. Query a made-up ID to verify the `404` JSON response.
5. To demonstrate poison handling, send malformed JSON directly to the queue. The Functions
   host logs three failed attempts and moves the message to `order-processing-queue-poison`.

## Handoff contract

- Hlogi: build this merged source as `lesegokhaole/coffeenchill-functions:v2.0`; the Compose
  file already supplies the service-name Azurite connection string.
- Kamva: use `POST /api/orders/queue` followed by status polling in Postman. Use the response
  shape documented above and retain the negative `400` and `404` cases.
- Joseph: preserve the Base64 encoding and the `Order` JSON property names exactly.

## Why malformed messages are rethrown

The queue trigger deliberately rethrows malformed or incomplete messages. Swallowing an error
would acknowledge and delete the message. Throwing lets the Functions host retry it according to
`host.json` and eventually move it to the poison queue, which is the required failure handling.
