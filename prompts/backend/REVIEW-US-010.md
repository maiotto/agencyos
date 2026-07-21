# REVIEW-US-010

Review Checklist

## Architecture

- Service implemented in Application layer
- Repository responsibilities respected
- Controller contains no business logic
- No duplicated calculations

---

## Business Rules

- Only Active Resources are calculated
- Cancelled Assignments are ignored
- Completed Assignments are ignored
- Planning period is mandatory
- Capacity cannot become negative

---

## API

- GET /capacity implemented
- GET /capacity/{resourceId} implemented
- GET /capacity/summary implemented
- REST conventions respected
- Correct HTTP status codes
- Swagger updated

---

## Validation

- Planning Period required
- Invalid requests handled correctly

---

## Logging

- Capacity calculation logged
- Calculation duration logged
- Calculation errors logged

---

## Build

dotnet build

Expected

- 0 Warnings
- 0 Errors

---

## Definition of Done

- Capacity calculated
- Unit tests
- Build OK
- Swagger updated

---

## Final Approval

Ready for Commit
