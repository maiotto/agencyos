# MVP Implementation Plan

**Project:** AgencyOS MVP 1.0  
**Phase:** 2 — Commercial Domain Implementation  
**Version:** 1.0  
**Status:** Approved for Execution  
**Owner:** AgencyOS Program  
**Last Updated:** July 2026

---

# Purpose

This document defines the execution plan for implementing the Commercial Domain of the AgencyOS MVP 1.0.

Its purpose is to provide a controlled implementation process aligned with the approved architecture, functional specifications, engineering standards and governance model.

This document bridges the gap between the approved documentation and the actual implementation.

No architectural or functional decisions are introduced here.

---

# Scope

## Included

The implementation includes the complete Commercial Domain composed of:

- Lead Management
- Client Management
- Client Contact Management
- Client Contract Management

The implementation covers:

- Domain Entities
- Value Objects (when applicable)
- Repository Interfaces
- Repository Implementations
- Application Services
- DTOs
- Validators
- REST Controllers
- Swagger Documentation
- HTTP Tests
- Unit Tests
- Dependency Injection
- Logging
- Exception Handling

---

## Excluded

The following capabilities are explicitly out of scope:

- Operations Domain
- Missions
- Tasks
- Execution Resources
- Assignments
- Planning Engines
- Decision Engine
- Frontend
- Decision Intelligence
- AI Factory
- New Product Features
- Architectural Changes

---

# Implementation Strategy

The Commercial Domain shall be implemented as a single integrated increment.

The four approved Functional Specifications have already been reconciled and validated together.

Therefore, implementation shall not occur independently by Functional Specification.

The implementation must preserve the integrity of the relationships between:

Lead

↓

Client

↓

Client Contact

↓

Client Contract

This approach minimizes integration inconsistencies and reduces implementation risk.

---

# Implementation Order

Implementation shall follow the business lifecycle.

```
Lead
    ↓
Client
    ↓
Client Contact
    ↓
Client Contract
```

Each stage depends on the previous one.

No downstream entity shall be implemented before its dependencies are complete.

---

# Deliverables

Each business entity shall include:

- Domain Entity
- Repository Interface
- Repository Implementation
- Application Service
- DTOs
- Validators
- REST Controller
- Swagger Documentation
- Dependency Injection Registration
- HTTP Tests
- Unit Tests

---

# Engineering Workflow

The approved implementation workflow is:

```
Commercial Documentation
        ↓
Engineering Review
        ↓
MVP Implementation Plan
        ↓
Cursor Implementation
        ↓
Implementation Review
        ↓
Corrections (if required)
        ↓
Commercial Domain Approval
        ↓
Documentation Update
        ↓
Git Commit
        ↓
Operations Domain Documentation
```

Documentation Update is mandatory before the commit, in accordance with ADR-008.

---

# Technical Standards

Implementation shall follow the official AgencyOS engineering standards.

## Architecture

- Layered Architecture
- Database First
- Clean Architecture
- Single Responsibility Principle
- Dependency Injection
- Domain-Oriented Design

---

## Database

- Existing schema only
- No architectural changes
- No unnecessary migrations
- snake_case naming
- UUID primary keys

---

## APIs

REST conventions shall be respected.

Each endpoint shall provide:

- Input Validation
- Standard HTTP Responses
- Swagger Documentation
- Consistent DTOs

---

## Validation

Business validation shall be implemented using FluentValidation.

Business rules belong to the Domain/Application layers.

Controllers shall contain no business logic.

---

# Quality Gates

Each implementation must satisfy the following criteria before approval.

## Build

- Successful build
- Zero compilation errors
- Zero warnings

---

## Tests

- Unit Tests passing
- HTTP Tests passing
- Repository Tests passing (when applicable)

---

## API

- Swagger available
- Endpoints validated
- Standard HTTP responses

---

## Code Review

The implementation shall be reviewed to verify:

- Architecture compliance
- Naming conventions
- Layer isolation
- Business rule consistency
- Clean Architecture adherence

---

# Acceptance Criteria

The Commercial Domain will be accepted only when all of the following are satisfied.

## Functional

- Lead fully operational
- Client fully operational
- Client Contact fully operational
- Client Contract fully operational

---

## Technical

- Build successful
- Tests passing
- Dependency Injection configured
- Swagger updated

---

## Documentation

Documentation updated according to ADR-008.

The following documents shall be updated when required:

- Sprint Register
- Decision Log
- Baseline
- Architecture Documentation
- Technical Documentation

---

## Review

Implementation Review approved.

---

# Merge Strategy

Development shall follow the official Git Flow.

```
feature/commercial-domain
        ↓
develop
        ↓
main
```

Direct commits to **main** are prohibited.

Merge shall occur only after successful implementation review and documentation update.

---

# Test Strategy

Validation shall occur progressively.

## Entity Validation

- Lead
- Client
- Contact
- Contract

---

## API Validation

CRUD operations

- Create
- Read
- Update
- Delete

---

## Business Flow Validation

The complete commercial lifecycle shall be validated.

```
Lead
    ↓
Client
    ↓
Client Contact
    ↓
Client Contract
```

---

# Rollback Strategy

Rollback shall be performed exclusively through Git.

Database rollback shall occur only through version-controlled SQL migrations when necessary.

Manual database modifications are prohibited.

---

# Risks

Potential implementation risks include:

- Relationship inconsistencies
- Validation gaps
- API contract deviations
- Documentation drift

Risk mitigation includes:

- Engineering Review
- Implementation Review
- Automated Tests
- Documentation Update
- Controlled Merge

---

# Definition of Done

The Commercial Domain implementation is considered complete only when all of the following conditions are met:

- All four business entities implemented
- Repositories completed
- Services completed
- DTOs completed
- Validators completed
- Controllers completed
- Swagger updated
- Unit Tests passing
- HTTP Tests passing
- Build successful
- Zero warnings
- Zero errors
- Documentation updated
- Engineering Review approved
- Commercial Domain approved
- Commit completed

---

# Next Phase

After successful completion of the Commercial Domain, the AgencyOS MVP implementation proceeds to:

**Operations Domain Documentation**

following the same governance process established for the Commercial Domain.

---

# Official Statement

This document defines the official execution plan for implementing the Commercial Domain of AgencyOS MVP 1.0.

It does not introduce new product requirements or architectural decisions.

Its purpose is to ensure that implementation is executed in a controlled, traceable and auditable manner, preserving the integrity of the approved documentation and governance model.