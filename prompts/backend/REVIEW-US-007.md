# REVIEW-US-007

Review Checklist

## Architecture

- Layer separation respected
- Repository pattern respected
- Dependency Injection used
- No business logic inside Controller

---

## Business Rules

- Every Task belongs to one Mission
- Estimated Hours must be greater than zero
- Completed Tasks cannot be edited
- Task Code must be unique inside the Mission

---

## API

- GET /tasks implemented
- GET /tasks/{id} implemented
- POST /tasks implemented
- PUT /tasks/{id} implemented
- DELETE /tasks/{id} implemented
- PATCH /tasks/{id}/complete implemented
- REST conventions respected
- Correct HTTP status codes
- Swagger updated

---

## Validation

- FluentValidation implemented
- Required fields validated
- Mission relationship validated
- Estimated Hours validated
- Task Code uniqueness validated
- Completed Task edit blocked

---

## Logging

- Create logged
- Update logged
- Complete logged

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
- Build OK

---

## Final Approval

Ready for Commit
