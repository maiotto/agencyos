# REVIEW-US-008

Review Checklist

## Architecture

- Layer separation respected
- Repository pattern respected
- Dependency Injection used
- No business logic inside Controller

---

## Business Rules

- Resource Code must be unique
- Capacity must be greater than zero
- Inactive Resources cannot receive new assignments
- Historical assignments remain unchanged
- Physical deletion is not allowed

---

## API

- GET /execution-resources implemented
- GET /execution-resources/{id} implemented
- POST /execution-resources implemented
- PUT /execution-resources/{id} implemented
- DELETE /execution-resources/{id} implemented
- REST conventions respected
- Correct HTTP status codes
- Swagger updated

---

## Validation

- FluentValidation implemented
- Resource Code required
- Resource Name required
- Resource Type required
- Status required
- Capacity required
- Duplicate Resource Code returns 409

---

## Logging

- Create logged
- Update logged
- Deactivate logged

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
