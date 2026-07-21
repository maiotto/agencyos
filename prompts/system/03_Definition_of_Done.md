# AgencyOS - Definition of Done

Version: 1.0

Status: Frozen for MVP

---

# Purpose

This document defines the mandatory completion criteria for every User Story, Task and Sprint of the AgencyOS project.

No implementation is considered complete unless every applicable criterion is satisfied.

---

# General Principles

Done means:

- Implemented
- Compiled
- Tested
- Reviewed
- Documented
- Committed

Work in progress is not Done.

---

# User Story Definition of Done

A User Story is complete only when:

- Acceptance Criteria are fully implemented.
- Solution builds successfully.
- No compilation errors.
- No warnings introduced.
- Architecture rules are respected.
- Coding Standards are respected.
- Logging is implemented where required.
- Validation is implemented where required.
- Dependency Injection is configured.
- Swagger documentation is available.
- Manual test executed successfully.
- Code reviewed.
- Changes committed to Git.

---

# API Checklist

Every new endpoint must:

- Follow REST conventions.
- Return correct HTTP status codes.
- Validate input.
- Return ProblemDetails for errors.
- Be visible in Swagger.
- Use DTOs.
- Never expose Domain Entities.

---

# Database Checklist

When a User Story changes the database:

- SQL migration created.
- Migration applied successfully.
- Foreign Keys validated.
- Indexes validated.
- Naming conventions respected.

Entity Framework migrations are not allowed.

---

# Code Quality Checklist

- No duplicated code.
- No dead code.
- No unused imports.
- Small methods.
- Single Responsibility Principle respected.
- Readable naming.
- Meaningful exceptions.

---

# Testing Checklist

Minimum manual validation:

- Happy path.
- Invalid input.
- Resource not found.
- Unexpected error.

Automated tests are optional during the MVP unless explicitly requested.

---

# Documentation Checklist

When applicable:

- Engineering documents updated.
- ADR updated (if architectural decision changed).
- Prompt updated (if AI process changed).

---

# Git Checklist

Before closing the User Story:

- git status reviewed
- git add executed
- git commit executed
- git push executed

Commit message must follow the project convention.

---

# Sprint Definition of Done

A Sprint is complete only when:

- All planned User Stories are Done.
- Acceptance Criteria achieved.
- Build succeeds.
- Database validated (if applicable).
- Documentation updated.
- Git repository synchronized.
- Sprint Review completed.

---

# Exit Criteria

A User Story or Sprint cannot be closed if any mandatory item remains incomplete.

Done means potentially releasable.