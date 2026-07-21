# REVIEW-US-003

Review Checklist

## Architecture

- Layer separation respected
- Repository pattern respected
- Dependency Injection used
- No business logic inside Controller

---

## Business Rules

- Legal Name is mandatory
- Tax Identifier must be unique
- Inactive Clients cannot receive new Contracts
- Existing Contracts remain valid after Client deactivation
- Clients with active Contracts cannot be deleted
- Physical deletion is not allowed

---

## API

- GET /clients implemented
- GET /clients/{id} implemented
- POST /clients implemented
- PUT /clients/{id} implemented
- DELETE /clients/{id} implemented
- REST conventions respected
- Correct HTTP status codes
- Swagger updated

---

## Validation

- FluentValidation implemented
- Legal Name required
- Status required
- Optional fields handled correctly
- Duplicate Tax Identifier returns 409

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
- Swagger updated
- HTTP file updated
- Validation implemented
- Logging implemented
- Build without warnings
- Build without errors
- Review approved

---

## Final Approval

Ready for Commit
