# Part 1 QA Evidence Register

This register prevents the group from relying on unsupported claims. Mark an item
complete only after the stated evidence exists and identifies the current final build.

| Evidence required | Owner | What must be supplied to Lesego | Status |
|---|---|---|---|
| Menu endpoints against Azurite | Joseph | One full passing Menu Collection Runner result, plus any defect-fix pull request/commit links | Pending |
| `staff-docs` Blob container | Kamva | Azurite evidence plus successful upload/list/download results from the final API build | Pending |
| Docker build | Hlogi | Terminal screenshot showing the final image build succeeds and its tag | Pending |
| Standalone containers | Hlogi | Screenshots of `docker ps`, the shared network, Azurite, Functions container, and Functions logs showing eight Functions | Pending |
| Docker Hub publication | Hlogi | Public Docker Hub links/screenshots for the Functions and Azurite `v1.0` images | Pending |
| Complete Postman run | Lesego | Collection Runner result showing all saved tests pass | Pending |
| Final README | Lesego | README commit containing the real unlisted YouTube link | Pending |
| Team contribution history | All members | GitHub commit-history screenshot showing five or more meaningful commits for each member | To verify |
| Video demonstration | All members | Unlisted YouTube URL; Docker commands, live API tests, architecture explanation, and human voices | Pending |
| Final delivery branch | Lesego | Pull request/merge evidence showing the final reviewed project is on `master` before submission | Pending |

## Evidence rules

- Capture evidence from the final branch/image, not an earlier ZIP or stale image.
- Keep screenshots readable: include the command, date/time if visible, and outcome.
- Do not commit Docker credentials or private account details.
- The official Part 1 addendum requires `staff-docs` Azure Blob Storage. Capture document
  endpoint evidence from the local Azurite Blob service, not an Azure File Share.

## Current technical verification — 12 September 2026

The following checks were performed against the final Part 1 implementation on
the `fix/part1-integration` branch. They are technical verification records;
the team should still capture the readable application screenshots listed above
for the submission/video evidence.

- `dotnet build` completed with zero warnings and zero errors.
- The eight Function routes loaded in the Docker Functions container at
  `http://localhost:7071/api`.
- All 14 API scenarios passed against Docker-networked Azurite: create,
  duplicate, validation, list, category filter, update, update-not-found,
  delete, delete-not-found, upload-without-file, upload, document list,
  document download, and document-not-found.
- The document scenarios use the required local Blob container, `staff-docs`.
- Public Docker Hub images have been published as
  `lesegokhaole/coffeenchill-functions:v1.0` and
  `lesegokhaole/coffeenchill-azurite:v1.0`.
- `PostmanCollection.json` is valid JSON and contains all 14 saved requests,
  each with a Postman test. The upload request still needs a real permitted
  file selected in the Postman desktop app before the Collection Runner proof
  is captured.
