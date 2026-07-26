# Cursor Commercial Domain Implementation Prompt

**Project:** AgencyOS MVP 1.0  
**Target:** Cursor AI  
**Version:** 1.0  
**Status:** Official Execution Prompt

---

# Objective

Implement the complete Commercial Domain of AgencyOS MVP 1.0.

This implementation must strictly follow the approved project documentation.

The objective is to produce production-quality code without introducing architectural changes or new business rules.

---

# Primary Reference

The implementation SHALL use the following documents as authoritative references.

Priority order:

1. AgencyOS Baseline
2. Approved ADRs
3. Program Architecture
4. Commercial Functional Specifications
5. MVP_Implementation_Plan.md
6. Commercial_Domain_Implementation_Package.md

If any conflict exists, follow the highest-priority document.

Do not make assumptions.

---

# Scope

Implement ONLY:

- Lead Management
- Client Management
- Client Contact Management
- Client Contract Management

Do not implement:

- Missions
- Tasks
- Planning
- Decision Engine
- Frontend
- Decision Intelligence
- AI Factory

---

# Engineering Principles

The implementation SHALL respect:

- Clean Architecture
- Layered Architecture
- SOLID Principles
- Single Responsibility Principle
- Dependency Injection
- Database First
- Domain-Oriented Design

Business logic belongs to the Domain/Application layers.

Controllers must remain thin.

Repositories must contain persistence only.

---

# Required Deliverables

For each business entity implement:

- Domain Entity
- Repository Interface
- Repository Implementation
- Application Service
- DTOs
- Validators
- REST Controller
- Dependency Injection
- Swagger Documentation
- HTTP Tests
- Unit Tests

No deliverable may be omitted.

---

# Implementation Order

The implementation SHALL follow this sequence:

```
Lead
    ↓
Client
    ↓
Client Contact
    ↓
Client Contract
```

Do not implement downstream entities before completing upstream dependencies.

---

# Coding Standards

Generate clean, maintainable production-ready code.

Use meaningful names.

Avoid duplicated logic.

Avoid unnecessary abstractions.

Avoid dead code.

Avoid commented code.

Avoid TODOs.

---

# Validation

Use FluentValidation.

Business validation belongs in validators and services.

Do not place business rules inside controllers.

---

# API Standards

Every controller shall expose:

GET

GET BY ID

POST

PUT

DELETE

Return standardized HTTP responses.

Swagger documentation is mandatory.

---

# Error Handling

Use the project's standard exception handling strategy.

Do not swallow exceptions.

Return standardized responses.

---

# Logging

Generate structured logging for:

Create

Update

Delete

Validation failures

Unexpected exceptions

---

# Dependency Injection

Register every:

Repository

Service

Validator

through Dependency Injection.

---

# Tests

Generate:

Unit Tests

Repository Tests

HTTP Tests

The solution shall compile successfully before considering implementation complete.

---

# Forbidden Changes

Do NOT:

Modify architecture.

Create new entities.

Rename existing entities.

Change database schema.

Introduce new business rules.

Implement roadmap features.

Refactor unrelated modules.

Modify documentation.

Change naming conventions.

---

# Expected Quality

The generated code shall be production quality.

Requirements:

- Clean code
- Readable code
- Consistent naming
- Layer isolation
- Zero duplicated business logic
- Proper dependency injection
- Proper validation
- Proper error handling

---

# Stop Conditions

If implementation requires:

Architecture changes

Database redesign

New business rules

Missing functional requirements

Conflicting documentation

STOP.

Do not invent a solution.

Report the issue instead.

---

# Definition of Done

The implementation is complete only when:

✓ Build succeeds

✓ Zero compilation errors

✓ Zero warnings

✓ Unit Tests passing

✓ HTTP Tests passing

✓ Swagger complete

✓ CRUD complete

✓ Dependency Injection configured

✓ Logging implemented

✓ Validation implemented

✓ Architecture respected

✓ Ready for Engineering Review

---

# Expected Output

Produce only implementation artifacts.

Do not rewrite documentation.

Do not explain architecture.

Do not propose improvements.

Do not introduce alternative implementations.

Focus exclusively on implementing the approved Commercial Domain.

---

# Execution Rule

Treat the approved documentation as immutable.

Implementation is an execution activity, not a design activity.

Any uncertainty must be reported instead of resolved through assumptions.

End of Prompt.