# AgencyOS Baseline v1.0

Version: 2.0

Status: Official Baseline

Baseline Date: July 2026

---

# Purpose

This document establishes the official baseline for AgencyOS after the completion of the program audit.

It consolidates all strategic, architectural and engineering decisions taken during:

- Phase 1
- Sprint 0
- Sprint 1
- Sprint 2
- Sprint 3
- Sprint 4
- Sprint 5
- Sprint 6
- Sprint 7
- Sprint 8
- Sprint 9
- Sprint 10

and reflects the official project progress reached through Phase 2 (Commercial Domain certification), Phase 3 (Operations Domain certification), and Phase 4 (Analytics & Decision Engine certification).

Phase 4 has been completed. Analytics & Decision Engine certification has been completed. The next planned milestone is Frontend MVP.

This document supersedes previous baseline versions and remains the official reference for future development.

---

# Program Vision

AgencyOS is an AI-First Goal-Oriented Operational Planning Platform.

Its objective is to transform business objectives into optimized execution strategies by combining deterministic operational intelligence with Artificial Intelligence.

Unlike ERP systems, CRM systems or Project Management tools, AgencyOS focuses on answering operational decision questions before execution begins.

Typical decisions supported include:

- Can this contract be accepted?
- Do we have enough capacity?
- Should additional resources be hired?
- Should work be outsourced?
- Which execution strategy produces the best business outcome?
- What operational risks exist?
- How will changing constraints affect delivery?

---

# Program Mission

Transform commercial demand into optimized operational execution.

AgencyOS continuously evaluates:

Business Goals

↓

Operational Constraints

↓

Capacity

↓

Execution Alternatives

↓

Business Recommendations

↓

Human Approval

↓

Execution

↓

Continuous Replanning

---

# Product Principles

The following principles are considered immutable during Baseline 1.0.

## Capacity First

Capacity is the primary operational asset.

Commercial commitments shall never ignore productive capacity.

---

## Goal-Oriented Planning

Planning begins from business objectives.

Resources are allocated only after goals have been defined.

---

## Explainable Decisions

Every recommendation must be explainable.

Every recommendation must be auditable.

---

## Deterministic Core

Business calculations remain deterministic.

Artificial Intelligence complements deterministic logic.

It never replaces it.

---

## Human Governance

Artificial Intelligence proposes.

Humans approve.

AgencyOS always supports human decision making.

---

# Program Structure

AgencyOS consists of two independent strategic programs.

AgencyOS

│

├── Program A

│      AgencyOS Product

│

└── Program B

       AgencyOS AI Factory

The Product delivers customer value.

The AI Factory delivers engineering productivity.

Both programs evolve independently.

---

# Product Scope

AgencyOS manages the complete operational lifecycle.

Lead

↓

Client

↓

Contract

↓

Mission

↓

Task

↓

Execution Resource

↓

Assignment

↓

Planning

↓

Decision

↓

Execution

↓

Continuous Replanning

---

# MVP Scope

The MVP includes:

Commercial Domain

Operations Domain

Planning Engines

Decision Engine

REST APIs

Backend

Database

Swagger

Validation

Documentation

Automated Tests

The MVP explicitly excludes:

Frontend

Simulation

Optimization

Decision Intelligence

Machine Learning

Autonomous Planning

These capabilities belong to future releases.

---

# Current Program Status

Commercial Domain

Status: Certified

Operations Domain

Status: Certified

Current Phase

Phase 4 Completed

Next Phase

Frontend MVP

Business Vision

Completed

Architecture

Completed

Backend MVP

Completed

Database

Completed

Planning Engines

Completed

Decision Engine

Completed

Documentation

Under Consolidation

Frontend

Not Started

Analytics

Completed

Decision Intelligence

Architecture Completed

Implementation Pending

---

# Technology Stack

Backend

ASP.NET Core 8

C# 12

Entity Framework Core 8

FluentValidation

Database

PostgreSQL

Supabase Cloud

Frontend

React

TypeScript

Engineering

Git

GitHub

Cursor

Swagger / OpenAPI

Architecture

Layered Architecture

Domain-Oriented Design

Database First

AI-Assisted Engineering

---

# Official Repository Structure

/backend

/frontend

/database

/docs

/infrastructure

/prompts

/scripts

/supabase

Every project artifact shall belong to one of these top-level directories.

No parallel structures are permitted without architectural approval.
---

# Official Product Architecture

The official architecture of AgencyOS Baseline 1.0 is defined by six logical layers.

Presentation

↓

Application

↓

Business Domains

↓

Operational Intelligence

↓

Decision

↓

Decision Intelligence (Future)

Dependencies always point downward.

Business rules never depend on infrastructure.

Artificial Intelligence never replaces deterministic business logic.

---

# Business Domains

AgencyOS is organized around independent business domains.

Each domain owns:

- Entities
- Business Rules
- Services
- Repositories
- APIs

Domains communicate through well-defined interfaces.

No domain directly modifies another domain.

---

## Commercial Domain

Purpose

Manage the commercial lifecycle from opportunity identification to signed contract.

Responsibilities

- Lead Management
- Client Management
- Client Contacts
- Client Contracts
- Commercial History

Core Entities

- Lead
- Client
- ClientContact
- ClientContract

Output

Approved commercial demand.

---

## Operations Domain

Purpose

Transform approved demand into executable operational work.

Responsibilities

- Mission Management
- Task Management
- Execution Resources
- Assignments
- Operational Organization

Core Entities

- Mission
- Task
- ExecutionResource
- Assignment

Output

Operational execution plan.

---

# Operational Intelligence

Operational Intelligence evaluates execution feasibility before work begins.

This layer is completely deterministic.

Every calculation is reproducible.

Artificial Intelligence is intentionally excluded.

---

## Capacity Engine

Calculates productive capacity.

Outputs

- Installed Capacity
- Planned Capacity
- Remaining Capacity
- Capacity Utilization

---

## Workload Engine

Calculates operational workload.

Outputs

- Planned Hours
- Assigned Hours
- Resource Load
- Mission Load
- Contract Load

---

## Availability Engine

Calculates operational availability.

Outputs

- Available Resources
- Available Time
- Availability Matrix

---

## Allocation Conflict Detection

Detects operational conflicts.

Examples

- Overallocated Resources
- Schedule Conflicts
- Invalid Assignments
- Resource Collisions

No automatic correction is performed.

---

# Decision Engine

The Decision Engine is the principal differentiator of the MVP.

Its responsibility is not to execute work.

Its responsibility is to recommend the best execution strategy.

The engine consists of four independent services.

Delivery Strategy Builder

↓

Delivery Strategy Evaluator

↓

Delivery Strategy Ranking

↓

Delivery Strategy Explanation

---

## Builder

Generates feasible execution alternatives.

---

## Evaluator

Measures operational performance.

Metrics include

- Cost
- Duration
- Capacity
- Utilization
- Risk

---

## Ranking

Orders alternatives according to Company Decision Profiles.

---

## Explanation

Produces deterministic explanations for every recommendation.

Recommendations are always auditable.

---

# Decision Intelligence

Status

Architecture Approved

Implementation Pending

Decision Intelligence extends the MVP architecture.

It does not replace the Decision Engine.

Future components include

- Constraint Engine
- Strategy Generation Engine
- Optimization Engine
- Simulation Engine
- Explainability Engine

Planning becomes Goal-Oriented instead of Resource-Oriented.

Business Goals

↓

Operational Constraints

↓

Execution Strategy

↓

Execution Plan

↓

Resources

---

# Company Decision Profiles

AgencyOS supports configurable organizational priorities.

Official profiles

- Profit Maximization
- Delivery Speed
- Operational Stability
- AI Adoption
- Human Resource Optimization
- Balanced Strategy

Future releases may allow organization-specific profiles.

---

# Artificial Intelligence Strategy

AgencyOS follows a Hybrid AI Architecture.

## Deterministic Layer

Responsible for

- Capacity
- Workload
- Availability
- Conflict Detection
- Ranking
- Business Rules

Deterministic components always produce identical results.

---

## AI Layer

Responsible for

- Strategy Generation
- Scenario Exploration
- Recommendation Refinement
- Natural Language Interaction

Artificial Intelligence always operates within deterministic business boundaries.

---

# Official Technology Stack

Backend

- ASP.NET Core 8
- C# 12
- Entity Framework Core 8
- FluentValidation

Database

- PostgreSQL
- Supabase

Frontend

- React
- TypeScript

Engineering

- Cursor
- Git
- GitHub
- Swagger/OpenAPI

---

# Engineering Standards

The following principles are mandatory.

Database First

Every business domain begins with the data model.

Layer Isolation

Dependencies always point inward.

Single Responsibility

Each service performs one business responsibility.

Documentation First

Documentation precedes implementation.

Definition of Done

Implementation is complete only when:

- Build succeeds
- Tests pass
- Documentation updated
- Review completed
- APIs documented

---

# Official Documentation

The official documentation hierarchy is:

1. Baseline
2. Architecture
3. ADRs
4. Product Documentation
5. Technical Documentation
6. Sprint Documentation
7. Audit Documentation

Historical documents remain available for traceability but do not supersede the approved Baseline.

---

# Architecture Governance

Architecture changes require:

- Architecture review
- Updated ADR
- Updated Architecture documentation
- Updated Baseline (when applicable)

Architecture shall not evolve through implementation alone.

Every structural decision must be formally documented.
---

# Engineering Governance

Engineering governance ensures that AgencyOS evolves in a controlled, predictable and auditable manner.

The governance model separates business decisions from engineering decisions.

Business decisions are owned by Product Management.

Engineering decisions are owned by Architecture.

Implementation decisions are owned by Engineering Teams.

Every decision affecting architecture, product scope or engineering standards shall be documented.

---

# Change Management

Changes are classified into four categories.

## Product Changes

Examples

- New business capabilities
- Functional requirements
- Product scope

Approval

Product Owner

Architecture Review (when required)

---

## Architecture Changes

Examples

- New architectural layers
- New business domains
- Structural modifications
- Technology replacement

Approval

Architecture Review

Architecture Decision Record (ADR)

Baseline update (if applicable)

---

## Engineering Changes

Examples

- Coding standards
- Development workflow
- Tooling
- Build process

Approval

Technical Leadership

Engineering Handbook update

---

## Operational Changes

Examples

- Documentation
- Build scripts
- CI/CD improvements
- Monitoring

Approval

Engineering Team

---

# Documentation Governance

Documentation is considered part of the product.

No implementation is considered complete until the corresponding documentation has been updated.

Documentation categories

- Product
- Architecture
- Functional
- Technical
- ADR
- AI Factory
- Sprint
- Project Management
- Audit

Each document has a single owner.

Duplicated information shall be avoided.

Historical information remains preserved through audit records rather than repeated across operational documents.

---

# Architecture Decision Records

Architecture Decision Records (ADR) document structural decisions that affect the evolution of AgencyOS.

Every ADR shall include:

- Context
- Decision
- Alternatives Considered
- Consequences
- Status

Current approved ADRs include:

- ADR-006 – AgencyOS AI Factory
- ADR-007 – Company Decision Profiles
- ADR-008 – Documentation Update Workflow

Future architectural decisions shall continue this sequence.

---

# Development Methodology

AgencyOS follows an incremental delivery model.

The project is organized into independent implementation cycles.

Each cycle shall produce a complete, testable and documented increment.

Implementation lifecycle

Business Requirement

↓

Analysis

↓

Architecture

↓

Planning

↓

Implementation

↓

Testing

↓

Documentation

↓

Review

↓

Approval

↓

Merge

No implementation may bypass this workflow.

---

# Sprint Strategy

Sprints are organized around complete business capabilities.

Objectives

- Deliver working software
- Preserve architectural integrity
- Maintain documentation
- Minimize technical debt

Sprint completion requires:

- Functional validation
- Technical validation
- Documentation update
- Review approval

---

# Repository Governance

Official repository structure

/backend

/frontend

/database

/docs

/infrastructure

/prompts

/scripts

/supabase

Repository organization shall remain stable during Baseline 1.0.

New top-level directories require architectural approval.

---

# Branch Strategy

The official branch model is:

main

Production-ready baseline.

develop

Integration branch.

feature/*

Individual implementation work.

hotfix/*

Production corrections.

release/*

Future production releases.

Direct commits to main are prohibited.

---

# Database Governance

AgencyOS adopts Database First as a permanent engineering principle.

Rules

- SQL migrations managed by Supabase
- Immutable migration history
- Version-controlled schema evolution
- Deterministic seed data
- UUID primary keys
- snake_case naming

Entity Framework Core is restricted to data access.

Entity Framework migrations are prohibited.

---

# API Governance

Every business capability shall be exposed through versioned REST APIs.

API principles

- Stateless
- Resource-oriented
- OpenAPI documented
- DTO-based
- Input validation
- Standard error handling

Every API shall include:

- Request validation
- Response documentation
- HTTP test coverage

---

# Quality Standards

Every implementation shall satisfy the Definition of Done.

Minimum quality requirements

- Successful build
- Zero compilation errors
- Zero unresolved warnings
- Automated tests passing
- Documentation updated
- Swagger updated
- HTTP tests updated
- Code review completed

---

# AI Factory Governance

AgencyOS AI Factory is an independent engineering program.

Purpose

Accelerate software engineering through specialized AI agents.

Responsibilities

- Prompt engineering
- Agent orchestration
- Documentation automation
- Code generation
- Code review
- Engineering analytics

The AI Factory shall not modify:

- Product Architecture
- Domain Model
- Product Scope
- Product Roadmap

without explicit architectural approval.

---

# AI Agent Organization

Management

- Product Owner Agent
- Product Manager Agent
- Scrum Master Agent

Architecture

- Solution Architect Agent

Engineering

- Backend Agent
- Frontend Agent
- Database Agent
- DevOps Agent

Quality

- QA Agent
- Code Review Agent
- Documentation Agent

Each agent performs a single primary responsibility.

Future agents shall extend—not replace—the existing engineering workflow.

---

# Risk Management

The principal program risks are:

- Architectural drift
- Documentation drift
- Scope expansion
- Technical debt
- AI-generated inconsistencies
- Integration complexity

Mitigation strategies include:

- ADR process
- Baseline governance
- Audit process
- Documentation reconciliation
- Incremental implementation
- Human architectural review

---

# Program Metrics

The following indicators shall be monitored throughout the program:

Engineering

- Build Success Rate
- Test Coverage
- Documentation Coverage
- Review Completion

Product

- MVP Completion
- Feature Progress
- Architecture Stability

Operations

- Sprint Completion
- Change Requests
- Open Risks
- Technical Debt

These metrics provide objective visibility into program health.
---

# Program Roadmap

The AgencyOS roadmap is organized into progressive maturity stages.

Each stage builds upon the previous one without requiring architectural redesign.

---

## Phase 1 — Product Definition

Status

Completed

Objectives

- Product Vision
- Business Model
- Operational Concepts
- Initial Roadmap
- Product Scope

Deliverables

- Product Vision
- Initial Architecture
- Conceptual Models
- Product Backlog

---

## Sprint 0 — Engineering Foundation

Status

Completed

Objectives

- Development Environment
- Repository Structure
- Git Strategy
- Supabase
- Cursor
- Engineering Standards

Deliverables

- Repository
- Local Environment
- Documentation Structure
- Engineering Workflow

---

## Sprint 1 — Commercial Foundation

Status

Completed

Objectives

Implement the Commercial Domain.

Deliverables

- Lead
- Client
- Contact
- Contract
- Database Foundation

---

## Sprint 2 — Mission Domain

Status

Completed

Objectives

Introduce operational planning through Missions.

Deliverables

- Mission
- Mission Types
- Mission Status

---

## Sprint 3 — Task Domain

Status

Completed

Objectives

Decompose operational work into executable Tasks.

Deliverables

- Task
- Task Types
- Task Status

---

## Sprint 4 — Application Foundation

Status

Completed

Objectives

Establish backend architecture and engineering workflow.

Deliverables

- Application Layer
- Services
- Repositories
- Controllers
- Engineering Playbook

---

## Sprint 5 — Commercial Module

Status

Completed

Deliverables

- Commercial APIs
- CRUD Operations
- Validation
- Swagger
- HTTP Tests

---

## Sprint 6 — Operational Module

Status

Completed

Deliverables

- Resource Management
- Assignments
- Operational APIs

---

## Sprint 7 — Planning Engines

Status

Completed

Deliverables

- Capacity Engine
- Workload Engine
- Availability Engine
- Conflict Detection

---

## Sprint 8 — Decision Engine

Status

Completed

Deliverables

- Strategy Builder
- Strategy Evaluation
- Strategy Ranking
- Strategy Explanation

The MVP backend is considered functionally complete at this stage.

---

## Sprint 9 — Stabilization

Status

Completed

Objectives

- Engineering hardening
- Documentation synchronization
- Integration validation
- Governance consolidation

Deliverables

- Stable backend
- Documentation reconciliation
- Engineering governance

---

## Sprint 10 — Decision Intelligence Architecture

Status

Completed (Architecture)

Objectives

Define the next strategic evolution of AgencyOS.

Deliverables

- Decision Intelligence Architecture
- Goal-Oriented Planning
- Constraint Engine
- Simulation Architecture
- Optimization Concepts

Implementation intentionally postponed.

---

# Current Program Status

Overall Status

Stable

Commercial Domain

Status: Certified

Operations Domain

Status: Certified

Current Phase

Phase 4 Completed

Next Phase

Frontend MVP

Business Vision

Completed

Product Architecture

Completed

Engineering Architecture

Completed

Backend MVP

Completed

Database

Completed

Planning Engines

Completed

Decision Engine

Completed

Documentation Audit

Completed

Documentation Consolidation

In Progress

Frontend

Not Started

Analytics

Completed

Decision Intelligence

Architecture Completed

Implementation Pending

Certification Status

Commercial Domain — Certified

Operations Domain — Certified

Analytics — Certified

Decision Engine — Certified

---

# MVP Scope

The MVP officially includes:

Commercial Domain

Operations Domain

Planning Engines

Decision Engine

REST APIs

Authentication Foundation

Validation

Swagger Documentation

HTTP Tests

Automated Tests

Engineering Workflow

Documentation

The MVP officially excludes:

Decision Intelligence

Simulation

Optimization

Machine Learning

Frontend

Native Mobile

Customer Portal

Autonomous Decision Making

These capabilities belong to future product releases.

---

# Official Deliverables

The following deliverables are considered complete.

Business

- Product Vision
- Business Scope
- Operational Model

Architecture

- Program Architecture
- Layered Architecture
- Domain Architecture

Engineering

- Development Workflow
- Engineering Standards
- Documentation Standards

Backend

- Commercial APIs
- Operational APIs
- Planning Engines
- Decision Engine

Database

- PostgreSQL Schema
- SQL Migrations
- Seed Data

Governance

- ADR Process
- Baseline
- Decision Log
- Sprint Register
- Audit

---

# Current Priorities

Priority 1

Complete documentation consolidation.

Priority 2

Finalize Baseline 1.0.

Priority 3

Review Engineering Handbook.

Priority 4

Frontend MVP planning.

Priority 5

Decision Intelligence implementation planning.

No new business capabilities should be introduced before documentation consolidation is completed.

---

# Release Strategy

The program follows milestone-based releases.

Current milestone

Baseline 1.0

Next milestone

Frontend MVP

Future milestones

Decision Intelligence

Simulation Platform

Optimization Platform

AI Copilot

Production Release

Each milestone requires:

- Architecture approval
- Documentation approval
- Functional validation
- Technical validation

---

# Baseline Freeze Policy

The following artifacts are frozen under Baseline 1.0.

Product Vision

Program Architecture

Business Domains

Database Architecture

Engineering Standards

Repository Structure

Technology Stack

Development Workflow

Changes to these artifacts require:

- Approved Architecture Decision Record
- Baseline update
- Documentation review

No architectural evolution shall occur outside the ADR process.

---

# Change Control

Changes are categorized as:

Minor

Documentation improvements.

Moderate

Engineering workflow improvements.

Major

Architecture, product scope or technology changes.

Major changes require formal approval before implementation.

---

# Success Criteria

Baseline 1.0 is considered complete when:

- Documentation is fully reconciled.
- Architecture is internally consistent.
- Engineering standards are consolidated.
- Product scope is frozen.
- MVP definition is approved.
- Historical decisions are preserved.
- Future roadmap is documented.

This baseline becomes the official reference for all future AgencyOS development until superseded by a new approved baseline.
---

# Permanent Architectural Principles

The following principles define the identity of AgencyOS and shall remain valid throughout the evolution of the platform.

## Business First

Technology exists to support business decisions.

Business requirements always drive architectural evolution.

---

## Capacity First

Operational capacity is the fundamental planning variable.

Commercial commitments, operational planning and execution strategies shall always consider available capacity before execution.

---

## Goal-Oriented Planning

Planning begins with business objectives.

Operational plans are generated to achieve business goals while respecting organizational constraints.

Resources are part of the solution.

They are not the starting point.

---

## Deterministic Core

Core business calculations shall always remain deterministic.

Examples include:

- Capacity
- Workload
- Availability
- Allocation Conflicts
- Business Constraints
- Financial Calculations
- Ranking

Identical inputs must always produce identical outputs.

---

## Explainable Intelligence

Every recommendation produced by AgencyOS shall be explainable.

Users must understand:

- Why a recommendation was selected.
- Which constraints influenced the decision.
- Which trade-offs were accepted.
- Which alternatives were rejected.

Decision transparency is mandatory.

---

## Human Governance

AgencyOS is a Decision Support Platform.

Final operational responsibility always belongs to authorized human users.

Artificial Intelligence assists decision making.

It never replaces organizational accountability.

---

## Incremental Evolution

The architecture evolves incrementally.

New capabilities extend existing architecture.

Architectural redesign shall be avoided unless formally approved.

---

# Documentation Governance

Documentation is an integral component of the product.

Every implementation must update its corresponding documentation.

Documentation categories include:

- Product
- Architecture
- Functional
- Technical
- Architecture Decision Records
- AI Factory
- Project Management
- Audit

Historical documentation shall never replace consolidated documentation.

The Baseline remains the official source of truth.

---

# Relationship with the Audit

The Program Audit reconstructs the evolution of AgencyOS.

The Audit documents preserve:

- Historical decisions
- Architectural evolution
- Roadmap evolution
- Engineering evolution

The Audit does not replace the Baseline.

The Baseline represents the current approved state of the program.

The Audit represents its historical evolution.

Both documents complement each other.

---

# Relationship with Architecture Decision Records

Architecture Decision Records document individual structural decisions.

The Baseline consolidates all approved decisions.

If inconsistencies arise:

Architecture Decision Record

↓

Baseline Update

↓

Architecture Update

↓

Engineering Update

The Baseline shall always reflect the current approved architecture.

---

# Relationship with the AI Factory

AgencyOS Product and AgencyOS AI Factory are independent strategic programs.

AgencyOS Product

Responsible for:

- Product Vision
- Business Domains
- Operational Intelligence
- Decision Engine
- Decision Intelligence

AgencyOS AI Factory

Responsible for:

- AI-assisted Engineering
- Agent Orchestration
- Prompt Engineering
- Documentation Automation
- Development Productivity

The AI Factory accelerates product delivery.

It does not define product architecture.

---

# Future Evolution

Future releases are expected to expand AgencyOS through:

Decision Intelligence Implementation

Simulation Platform

Optimization Platform

Advanced Explainability

Business Analytics

Predictive Capacity Planning

AI Copilot

Autonomous Planning Assistance

These capabilities shall extend the current architecture without violating the permanent principles defined by this Baseline.

---

# Baseline Ownership

Document Owner

AgencyOS Program

Architecture Authority

Solution Architecture

Business Authority

Product Management

Engineering Authority

Technical Leadership

Any modification to this document requires formal review and approval by the appropriate authority.

---

# Version History

| Version | Date | Description |
|---------|------|-------------|
| 1.0 | 2026-07-21 | Initial MVP Baseline |
| 2.0 | 2026-07 | Baseline fully reconciled after Audit 01–14 and documentation consolidation |
| 2.0 | 2026-07-26 | Phase 3 completion status update — Operations Domain certified; next phase Phase 4 – Analytics & Decision Engine |
| 2.0 | 2026-07-26 | Phase 4 completion status update — Analytics & Decision Engine certified; next milestone Frontend MVP |

---

# Baseline Approval

This document represents the official AgencyOS Baseline 1.0.

It consolidates the strategic, business, architectural and engineering decisions established throughout the program.

All future development shall use this document as the primary reference for:

- Product evolution
- Architecture
- Engineering
- Governance
- Documentation

Previous baseline versions remain archived for historical reference.

---

# End of Document