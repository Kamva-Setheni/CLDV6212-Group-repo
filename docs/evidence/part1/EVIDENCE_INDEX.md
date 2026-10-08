# CoffeeNChill Part 1 Evidence Index

These are the strongest **final-state** screenshots from the project walkthrough. They are ordered for a short demonstration: show the standalone Docker environment first, then the Azurite activity, then the complete API test result.

| Order | File | What it proves | Part 1 requirement it supports |
| --- | --- | --- | --- |
| 1 | `01_Docker_Standalone_Containers_Running.png` | Docker Desktop shows the two separate running containers: `coffeenchill-azurite` and `coffeenchill-functions`. It also shows the Functions port `7071:80` and the Azurite port mapping. | Standalone Docker execution with no Docker Compose; Functions and Azurite are running separately. |
| 2 | `02_Azurite_and_Functions_Docker_Logs.png` | Docker logs show `coffeenchill-functions` handling `ListStaffDocuments` and `coffeenchill-azurite` receiving storage operations. | The Functions API communicates with local Azurite Blob Storage. |
| 3 | `03_Postman_All_14_Tests_Passed.png` | The Postman Collection Runner reports **All 14**, **Passed 14**, **Failed 0**, **Skipped 0**, and **Errors 0**. | The committed Postman collection tests all required Menu and Staff Document HTTP endpoints. |
| 4 | `04_Azurite_Blob_Document_List_200_OK.png` | `GET /api/documents` returns `200 OK` and lists `staff-document.pdf`, including its size and modification time. | The document was successfully stored and retrieved through the Azure Blob Storage implementation. |
| 5 | `05_Postman_Document_Workflow_Passed.png` | The runner shows the document upload error case, listing, successful download, and missing-download response all passing. | Coverage of the Staff Documents endpoints, including expected error responses. |

## What the official rubric actually requires

The Part 1 rubric and addendum do **not** require a static screenshot pack to be submitted. They require the following project artefacts instead:

- a public GitHub repository with the source and `/docs/PostmanCollection.json`;
- a README with local setup, standalone Docker commands, roles, and the unlisted YouTube video link;
- working Functions connected to Azurite Table Storage and Blob Storage;
- public Docker Hub links for the tagged Functions and Azurite images; and
- a video that visibly runs the Docker containers and the Postman collection.

These images are supporting evidence for the repository and video; they do not replace the required video or Docker Hub links.

## Screenshots that are not needed

Do not submit the earlier setup, empty-example, file-picker, failed-run, old-container, Docker account-settings, Postman onboarding, or free-disk-space screenshots. They either show an intermediate state, an earlier defect, or something outside the rubric.

There is also no need to capture a separate screenshot for every one of the 14 Postman requests: screenshot 3 is the concise proof that the complete saved suite passed, and screenshots 4 and 5 provide focused document-storage detail.

## Evidence not represented by an uploaded screenshot

- **Docker build success:** the rubric requires a working tagged image, but no final terminal/build-success screenshot was uploaded. Capture one only if your lecturer specifically asks for screenshot evidence.
- **Docker Hub publication:** the rubric requires verified public image links, not a screenshot. The README already contains the image links; a Docker Hub page screenshot is optional supporting evidence.
- **Git history:** the rubric requires five meaningful commits per group member. This must be verified from GitHub history, not inferred from these screenshots.
- **Video:** a screenshot cannot prove the required live demonstration or human voiceover. Add the unlisted YouTube link to the root README before submission.
