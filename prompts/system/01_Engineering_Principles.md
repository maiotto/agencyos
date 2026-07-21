# AgencyOS - Engineering Principles

Version: 1.0

Status: Frozen for MVP

---

# Purpose

This document defines the mandatory engineering principles for every software artifact generated for the AgencyOS project.

These principles are mandatory for every implementation.

No code shall violate these rules without explicit approval.

---

# Engineering Philosophy

The project follows these principles:

- Keep the MVP simple.
- Prefer readability over clever code.
- Prefer maintainability over premature optimization.
- Implement only what is required by the current User Story.
- Avoid unnecessary abstractions.
- Every implementation must be production-ready.

---

# Architecture

AgencyOS uses a layered architecture.

AgencyOS.Api

↓

AgencyOS.Application

↓

AgencyOS.Domain

AgencyOS.Infrastructure

AgencyOS.Shared

Dependencies always point inward.

The Domain layer must never depend on Infrastructure.

---

# Domain

The Domain layer contains:

- Entities
- Value Objects (when needed)
- Domain Rules

The Domain must not contain:

- SQL
- HTTP
- EF Core
- Controllers
- DTOs
- Logging
- External dependencies

---

# Application

Contains:

- Use Cases
- Services
- Interfaces
- DTOs
- Validators

Business orchestration belongs here.

---

# Infrastructure

Contains:

- Entity Framework Core
- DbContext
- Repository implementations
- External integrations
- Supabase access

---

# API

Contains only:

- Controllers
- Dependency Injection
- Middleware
- Swagger
- Authentication

Controllers must remain thin.

Business rules never belong inside Controllers.

---

# Database

Database schema is managed only by Supabase SQL migrations.

Entity Framework must never generate migrations.

---

# REST Standards

Endpoints must be plural.

Examples:

GET /missions

GET /missions/{id}

POST /missions

PUT /missions/{id}

DELETE /missions/{id}

---

# DTO Rules

Never expose Domain Entities.

Always use DTOs.

Separate Request DTOs from Response DTOs whenever appropriate.

---

# Validation

Use FluentValidation.

Controllers must not perform validation.

---

# Dependency Injection

Always use constructor injection.

Never instantiate dependencies manually.

---

# Logging

Use Microsoft.Extensions.Logging.

Log:

- Errors
- Warnings
- Important business events

Avoid noisy logs.

---

# Async

Prefer async/await.

Avoid synchronous I/O.

---

# Configuration

Secrets must never be committed.

Use:

- appsettings.json
- User Secrets
- Environment Variables

---

# Error Handling

Use ProblemDetails.

Never expose stack traces.

Return meaningful HTTP status codes.

---

# Naming

Classes

PascalCase

Interfaces

IMissionRepository

Methods

PascalCase

Variables

camelCase

Database

snake_case

Tables

singular

Columns

snake_case

---

# Code Quality

Methods should be small.

Classes should have a single responsibility.

Prefer composition.

Avoid duplicated code.

Follow SOLID.

---

# AI Rules

Generate complete implementations.

Never invent requirements.

Never redesign the architecture.

Never introduce new frameworks without explicit approval.

Always follow the existing project structure.

---

# Definition of Done

A User Story is complete only when:

- Code compiles.
- Build succeeds.
- No warnings.
- Dependency Injection configured.
- Swagger updated.
- Logging implemented.
- Validation implemented.
- Architecture respected.
- Code reviewed.

---

# MVP Restrictions

Do NOT introduce:

- Microservices
- CQRS
- MediatR
- Event Sourcing
- Message Brokers
- Distributed Transactions

The MVP must remain simple, maintainable and production-oriented.