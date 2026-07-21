# REVIEW-US-009

Review Checklist

## Architecture

- Layer separation respected
- Repository pattern respected
- Dependency Injection used
- No business logic inside Controller

---

## Business Rules

- Task must exist
- Execution Resource must exist
- Execution Resource must be Active
- Planned Hours must be greater than zero
- Allocation Percentage must be between 1 and 100
- Cancelled Assignments cannot be edited
- Physical deletion is not allowed

---

## API

- GET /assignments implemented
- GET /assignments/{id} implemented
- POST /assignments implemented
- PUT /assignments/{id} implemented
- DELETE /assignments/{id} implemented
- PATCH /assignments/{id}/cancel implemented
- REST conventions respected
- Correct HTTP status codes
- Swagger updated

---

## Validation

- FluentValidation implemented
- Task required
- Execution Resource required
- Planned Hours required
- Planned Start Date required
- Planned End Date required
- Status required
- Cancelled Assignment edit blocked

---

## Logging

- Create logged
- Update logged
- Cancel logged

---

## Build

dotnet build

Expected

- 0 Warnings
- 0 Errors

---

## Definition of Done

- CRUD complete
- Validation implemented
- Logging implemented
- Swagger updated
- HTTP Tests updated
- Build without warnings
- Build without errors
- Review approved

---

## Final Approval

Ready for Commit
