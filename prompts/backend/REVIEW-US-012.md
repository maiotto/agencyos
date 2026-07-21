# REVIEW-US-012

Review Checklist

## Architecture

- Service implemented in Application layer
- Capacity Calculator reused
- Workload Calculator reused
- No duplicated calculations
- Controller contains no business logic

---

## Algorithm

- Working Calendar respected
- Active Resources only
- Cancelled Assignments ignored
- Completed Assignments ignored
- Availability correctly calculated

---

## API

- REST conventions respected
- Correct HTTP status codes
- Swagger updated

---

## Validation

- Planning Period required
- Invalid requests handled correctly

---

## Logging

- Calculation started
- Calculation completed
- Errors logged

---

## Build

dotnet build

Expected

- 0 Warnings
- 0 Errors

---

## Final Approval

Ready for Commit