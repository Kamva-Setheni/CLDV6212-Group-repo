# Part 2 Completion Checklist

## Included in this project package

- Part 1 menu and staff-document functionality remains intact.
- `Order` message contract, queue producer, queue trigger, Orders-table lifecycle, and status endpoint are implemented.
- Queue retry, Base64 encoding, and poison-queue settings are in `host.json`.
- `docker-compose.yml` defines the required Azurite and Functions services, custom bridge network,
  named Azurite data volume, port mappings, and `image: lesegokhaole/coffeenchill-functions:v2.0`.
- The Postman collection contains 18 requests across Menu Items, Staff Documents, and Orders Queue.
- The README includes the Part 2 changelog, Compose instructions, Dockerfile syntax, roles, and
  an explicit video-link placeholder.
- `dotnet build --no-restore` completed with zero warnings and zero errors on 9 October 2026.
- `docker compose config`, a clean `docker compose up -d`, and the two required containers
  completed successfully on 9 October 2026.
- The live order flow was verified against the Compose stack: a valid order returned `202`, was
  stored in the Orders table, and advanced through Received, Preparing, Ready, and Collected.
- The live negative cases were verified: invalid input returned `400`, an unknown order returned
  `404`, and malformed Base64 queue content was retried three times then moved to
  `order-processing-queue-poison`.

## Required final-run steps

These are intentionally not marked complete because they require Docker Desktop to be running and,
where applicable, the authenticated Docker Hub owner or a real human video presentation:

1. Publish the locally built `lesegokhaole/coffeenchill-functions:v2.0` image from the authenticated
   Docker Hub account, then re-run Compose after a clean `docker compose pull`.
2. Run the expanded Part 2 Postman collection and retain its run-result evidence.
3. Capture the Orders-table lifecycle and a poison-queue message after three failed attempts for
   the human-video demonstration.
4. Add the unlisted human-voice YouTube demonstration link to `README.md`.
5. Verify the required contribution history and submit the project through the prescribed GitHub workflow.

The final-run commands and expected evidence are documented in `DOCKER_COMMANDS.md`,
`PART2_TEST_MATRIX.md`, and `PART2_LESEGO_WP2.md`.
