# AgencyOS Engineering Specification

# EFS-004 – Contract Management

Version: 1.0

Status: Approved for Engineering

Baseline: MVP 1.0

Owner: Product Management

Engineering Owner: Backend Team

Implementation Status: Pending

Documentation Gate

MVP_1.0_Documentation_Readiness_Review.md

---

# 1. Purpose

This Engineering Functional Specification defines the complete implementation requirements for the Contract Management capability of the Commercial Domain.

A Contract represents the commercial agreement established between the agency and a Client.

The Contract is the commercial Aggregate Root that generates operational demand when Approved and authorizes Mission creation only when Active.

This document is a pure implementation specification derived exclusively from the reconciled Functional Documentation.

Implementation shall strictly follow this specification.

No architectural decisions may be introduced during implementation.

This specification shall not redesign the product, introduce new features, modify Business Rules or modify approved architecture.

---

# 2. References

This specification is derived from the following approved documents.

Product Vision

Product Principles

Product Roadmap

AgencyOS Baseline

Program_Architecture.md

Decision_Log.md

ADR-006 AgencyOS AI Factory

ADR-007 Company Decision Profiles

ADR-008 Documentation Update Workflow

ADR-009 Audit and History Strategy

ADR-010 Aggregate Boundary Strategy

ADR-011 Archive and Soft Delete Policy

ADR-012 Enumeration Persistence Strategy

Business_Domains.md

Commercial_Domain.md

Business_Rules.md

Functional_Requirements.md

Non_Functional_Requirements.md

Use_Cases.md

User_Stories.md

Acceptance_Criteria.md

MVP_Feature_Matrix.md

MVP_1.0_Documentation_Readiness_Review.md

EFS-001 Lead Management

EFS-002 Client Management

EFS-003 Contact Management

These documents are frozen.

This specification shall not contradict them.

Authoritative Business Rules are defined exclusively in Business_Rules.md.

---

# 3. Business Context

A Contract formalizes the commercial relationship between the agency and a Client.

Business flow

Lead

↓

Client

↓

Contract

↓

Approval

↓

Activation

↓

Operations Domain

A Client may be associated with multiple Contracts.

A Contract belongs to exactly one Client by identity reference.

Approved Contracts generate operational demand.

Mission creation is authorized only when Contract Status = Active.

Contracts shall maintain complete commercial history through centralized Audit Events.

Reference

BR-COM-006

BR-COM-007

BR-COM-008

BR-COM-009

BR-COM-011

---

# 4. Functional Objectives

The system shall allow users to

Create Contracts for Active Clients

Update Contracts

Submit Contracts for Approval

Approve Contracts

Cancel Contracts

Activate Contracts

Complete Contracts

Archive Contracts

Search Contracts

Retrieve Contract Details

Maintain complete commercial history

Traceability

FR-COM-005

FR-COM-006

FR-COM-007

FR-COM-008

FR-COM-009

FR-COM-010

FR-COM-011

FR-OPS-001

FR-SYS-005

US-COM-005

US-COM-006

AC-COM-005

AC-COM-006

---

# 5. Domain Ownership

Domain

Commercial

Aggregate Root

Contract

Repository

IContractRepository

Application Service

ContractApplicationService

Domain Service

ContractDomainService

REST Controller

ContractController

Aggregate ownership follows ADR-010.

Contract is an independent Commercial Aggregate Root.

Contract references Client by identity.

Contract does not own Contact.

Contract does not own Mission as a child entity.

Mission belongs to Operations and references Contract by identity.

---

# 6. Aggregate Definition

Aggregate Root

Contract

Child Objects

None

External References

ClientId

Identity reference only

Mission

Identity reference from Operations

Not owned by Contract Aggregate

Relationships

One Client

←—— references by identity ——

Many Contracts

One Contract

←—— referenced by identity ——

Many Missions

The Contract Aggregate owns commercial Contract information only.

Client Contacts are outside the Contract Aggregate.

Missions are outside the Contract Aggregate.

Reference

ADR-010

BR-COM-006

BR-COM-012

---

# 7. Entity Definition

Entity Name

Contract

Schema

commercial

Table

contracts

Primary Key

contract_id

Identifier Type

UUID

Official Lifecycle

Draft

↓

Under Review

↓

Approved

↓

Active

↓

Completed

or

Cancelled

or

Archived

All transitions shall be validated.

Official statuses follow BR-COM-011.

---

# 8. Entity Fields

## ContractId

UUID

Generated automatically

Immutable

---

## ClientId

UUID

Required

Foreign Key

Immutable

References Client Aggregate Root by identity

---

## ContractCode

String

Maximum Length

30

Generated automatically

Unique

Format

CTR-000001

Immutable

---

## ContractName

String

Maximum Length

200

Required

---

## ContractType

Enumeration

Required

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

---

## Description

String

Maximum Length

4000

Optional

---

## StartDate

Date

Required

---

## EndDate

Date

Optional

Must be greater than StartDate when informed

---

## ContractValue

Decimal(18,2)

Required

Minimum

Zero

---

## Currency

String

Length

3

ISO-4217

Required

---

## BillingModel

Enumeration

Required

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

---

## Status

ContractStatus

Required

Default

Draft

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

BR-COM-011

---

## AccountManagerUserId

UUID

Required

---

## ApprovedByUserId

UUID

Nullable

Populated on approval

---

## ApprovedAt

UTC DateTime

Nullable

Populated on approval

---

## CreatedAt

UTC DateTime

Generated automatically

---

## UpdatedAt

UTC DateTime

Generated automatically

---

## ArchivedAt

UTC DateTime

Nullable

Populated when Contract status becomes Archived

Reference

ADR-011

BR-COM-013

---

## SoftDelete

Type

Boolean

Default

false

Technical field only

Not used by Contract business workflows

Not exposed as a business command

Reserved for exceptional maintenance according to ADR-011

---

## RowVersion

Optimistic concurrency uses the PostgreSQL xmin system column mapped through Entity Framework Core.

No SQL Server rowversion type is used.

Reference

NFR-044

---

# 9. Enumerations

Enumerations are implemented as C# enums.

Enumerations are persisted as smallint.

Lookup tables are not used.

Seed data is not created for enumerations.

Swagger uses enum names.

APIs return enum names.

Database stores integer values.

Reference

ADR-012

BR-COM-014

NFR-061

NFR-062

NFR-063

NFR-064

---

## ContractStatus

Draft

UnderReview

Approved

Active

Completed

Cancelled

Archived

---

## ContractType

Retainer

FixedPrice

TimeAndMaterial

Subscription

Project

Campaign

Other

---

## BillingModel

Monthly

Hourly

Daily

Milestone

Fixed

Consumption

---

# 10. Value Objects

ContractCode

Money

DateRange

ContractName

Currency

Each Value Object validates itself.

Primitive types shall not implement business validation.

---

# 11. Business Rules

Authoritative Business Rules are defined in Business_Rules.md.

This Engineering Specification implements the following Commercial Business Rules for Contract Management.

## BR-COM-006 — Contract Client Reference

Every Contract belongs to exactly one Client.

ClientId is immutable.

Contract cannot exist without a Client.

---

## BR-COM-007 — Multiple Contracts per Client

A Client may be associated with multiple Contracts.

Only Active Clients may create new Contracts.

Suspended Clients cannot create Contracts.

Archived Clients cannot create Contracts.

---

## BR-COM-008 — Operational Authorization for Missions

This is the single business rule that authorizes Mission creation.

Missions may be created only when the related Contract status is Active.

Approved means commercial approval completed and operational demand generated.

Approved alone does not authorize Mission creation.

Active is the only Contract status that authorizes Mission creation.

---

## BR-COM-009 — Contracts That Never Authorize Operational Work

Cancelled Contracts shall never authorize Mission creation or operational work.

Archived Contracts shall never authorize Mission creation or operational work.

Draft, Under Review, Approved and Completed Contracts shall never authorize Mission creation.

Completed Contracts are read-only for business mutation.

---

## BR-COM-010 — Commercial Audit History

Contract Aggregate Root shall generate Audit Events for successful state-changing operations.

GetContractHistoryQuery obtains history from centralized AuditEvents.

---

## BR-COM-011 — Official Contract Lifecycle

Every Contract shall have exactly one valid lifecycle status at all times.

Official statuses

Draft

Under Review

Approved

Active

Completed

Cancelled

Archived

---

## BR-COM-012 — Commercial Aggregate Ownership

Contract is a Commercial Aggregate Root.

Contact and Mission are outside the Contract Aggregate.

---

## BR-COM-013 — Archive Policy

Archive is the business inactivation model for Contract.

Archived Contracts have Status = Archived and ArchivedAt populated.

Archived Contracts are immutable.

SoftDelete is not part of Contract business workflows.

---

## BR-COM-014 — Closed Enumerations

ContractStatus, ContractType and BillingModel follow ADR-012.

---

## Engineering Constraints

ContractCode is generated automatically.

ContractCode is immutable.

ContractName is mandatory.

ContractValue shall be greater than or equal to zero.

EndDate shall be greater than StartDate when informed.

Only Approved Contracts may become Active.

Cancelled Contracts cannot become Active.

Contracts shall never be physically deleted through business workflows.

Approval requires an authorized user.

Every approval shall be auditable.

---

# 12. State Machine

Allowed transitions

Draft

↓

Under Review

Under Review

↓

Approved

Under Review

↓

Cancelled

Approved

↓

Active

Active

↓

Completed

Active

↓

Cancelled

Completed

↓

Archived

Cancelled

↓

Archived

Forbidden transitions

Cancelled

↓

Approved

Cancelled

↓

Active

Archived

↓

Any

Completed

↓

Active

Draft

↓

Active

Approved

↓

Draft

Any undefined transition shall throw BusinessRuleException.

---

# 13. Domain Invariants

Every Contract always has

ContractId

ClientId

ContractCode

ContractName

ContractValue

BillingModel

Status

CreatedAt

ContractId never changes.

ClientId never changes.

ContractCode never changes.

Approved Contracts always contain

ApprovedByUserId

ApprovedAt

Archived Contracts are immutable.

Completed Contracts are immutable for business mutation.

Only Active Contracts may authorize Mission creation.

Contract shall never own Mission as a child entity.

Contract shall never own Contact as a child entity.

---

# 14. Domain Events

ContractCreated

ContractUpdated

ContractSubmittedForApproval

ContractApproved

ContractCancelled

ContractActivated

ContractCompleted

ContractArchived

Domain Events are immutable.

Domain Events express business facts inside the domain model.

Domain Events support application reactions and orchestration.

Domain Events are published after successful transaction commit.

Domain Events are not the system of record for long-term history.

Reference

ADR-009

---

# 14A. Audit Events

Every successful Contract state-changing operation shall produce an Audit Event.

Audit Events are persisted in the centralized AuditEvents model.

Audit Events are written only after successful transaction commit.

Failed transactions shall not produce Audit Events.

Audit Events are immutable.

GetContractHistoryQuery shall read exclusively from AuditEvents filtered by:

AggregateType = Contract

AggregateId = ContractId

Audit Events shall capture at least:

AuditEventId

OccurredAt

AggregateType

AggregateId

EventType

ActorUserId

CorrelationId

Payload

Domain Events and Audit Events are separate.

One successful operation may produce both.

One shall not substitute for the other.

Reference

ADR-009

BR-COM-010

NFR-050

NFR-051

NFR-052

NFR-053

---

# 15. Security

Read

Commercial Users

Sales Managers

Operations Managers

Administrators

Create

Commercial Users

Sales Managers

Update

Commercial Users

Sales Managers

Approve

Sales Managers

Administrators

Cancel

Sales Managers

Administrators

Activate

Sales Managers

Administrators

Complete

Sales Managers

Administrators

Archive

Administrators

Delete

Not Allowed

Physical deletion is prohibited.

Business inactivation uses Archive only.

SoftDelete is not exposed as a business operation.

Reference

ADR-011

BR-COM-013

NFR-018

NFR-019

NFR-046

---

# End of Part 1

# 16. Application Layer

Application Service

ContractApplicationService

Responsibilities

- Create Contract
- Update Contract
- Submit Contract for Approval
- Approve Contract
- Cancel Contract
- Activate Contract
- Complete Contract
- Archive Contract
- Search Contracts
- Get Contract Details
- Get Contract History

The Application Service coordinates use cases.

Business Rules remain exclusively inside the Domain Layer.

ContractApplicationService validates that the referenced Client is Active before Contract creation.

ContractApplicationService shall not create Missions.

Mission creation belongs to Operations and must enforce BR-COM-008.

---

# 17. Domain Service

ContractDomainService

Responsibilities

Validate business invariants.

Validate lifecycle transitions.

Generate ContractCode.

Validate approval rules.

Validate activation rules.

Validate completion rules.

Validate cancellation rules.

Raise Domain Events.

The Domain Service shall never access Infrastructure.

The Domain Service shall never create Client, Contact or Mission Aggregates.

---

# 18. Repository

Interface

IContractRepository

Required Methods

Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken)

Task<Contract?> GetByCodeAsync(string code, CancellationToken cancellationToken)

Task<IEnumerable<Contract>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken)

Task<PagedResult<Contract>> SearchAsync(ContractSearchFilter filter, CancellationToken cancellationToken)

Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)

Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken)

Task AddAsync(Contract contract, CancellationToken cancellationToken)

Task UpdateAsync(Contract contract, CancellationToken cancellationToken)

Task SaveChangesAsync(CancellationToken cancellationToken)

Repositories shall never contain business logic.

IContractRepository exists because Contract is an Aggregate Root.

Contacts are not persisted through IContractRepository.

Missions are not persisted through IContractRepository.

Reference

ADR-010

---

# 19. Commands

CreateContractCommand

Fields

ClientId

ContractName

ContractType

Description

StartDate

EndDate

ContractValue

Currency

BillingModel

AccountManagerUserId

Rules

Client must exist

Client Status must be Active

---

UpdateContractCommand

Fields

ContractId

ContractName

Description

StartDate

EndDate

ContractValue

Currency

BillingModel

Rejected when Status is Completed, Cancelled or Archived

Allowed primarily for Draft and Under Review according to domain service rules

---

SubmitContractForApprovalCommand

Fields

ContractId

Requires Status = Draft

Sets Status = Under Review

---

ApproveContractCommand

Fields

ContractId

ApprovedByUserId

Requires Status = Under Review

Sets Status = Approved

Sets ApprovedByUserId

Sets ApprovedAt

Generates operational demand signal

Does not authorize Mission creation by itself

---

CancelContractCommand

Fields

ContractId

Reason

Allowed from Under Review or Active according to the state machine

Sets Status = Cancelled

Never authorizes Mission creation

---

ActivateContractCommand

Fields

ContractId

Requires Status = Approved

Sets Status = Active

Active is the sole status authorizing Mission creation

---

CompleteContractCommand

Fields

ContractId

Requires Status = Active

Sets Status = Completed

---

ArchiveContractCommand

Fields

ContractId

Reason

Requires Status = Completed or Cancelled

Sets Status = Archived

Sets ArchivedAt

Does not use SoftDelete

---

# 20. Queries

GetContractByIdQuery

GetContractByCodeQuery

GetContractsByClientQuery

SearchContractsQuery

GetContractHistoryQuery

GetContractHistoryQuery obtains history exclusively from centralized AuditEvents.

SearchContractsQuery excludes Archived records by default.

SearchContractsQuery excludes SoftDelete = true records from normal business retrieval.

Archived records may be included only when explicitly requested.

Reference

ADR-009

ADR-011

FR-COM-011

---

# 21. DTOs

CreateContractRequest

CreateContractResponse

UpdateContractRequest

ContractResponse

ContractSummaryResponse

ContractSearchResponse

ContractHistoryResponse

PagedContractResponse

ApproveContractRequest

CancelContractRequest

ArchiveContractRequest

DTOs shall never expose Domain Entities.

Enumeration properties in DTOs use enum names.

ContractResponse shall include

ContractId

ContractCode

ClientId

ContractName

Status

ApprovedByUserId when applicable

ApprovedAt when applicable

CreatedAt

ArchivedAt when applicable

Reference

ADR-012

---

# 22. Validators

CreateContractValidator

Rules

ClientId required

ContractName required

ContractType required

BillingModel required

StartDate required

ContractValue >= 0

Currency required

Client must be Active

---

UpdateContractValidator

Rules

ContractId required

ContractName required

EndDate > StartDate when EndDate informed

ContractValue >= 0

Contract must not be Completed, Cancelled or Archived

---

SubmitContractForApprovalValidator

Rules

ContractId required

Contract must be Draft

---

ApproveContractValidator

Rules

ContractId required

ApprovedByUserId required

Contract must be Under Review

---

CancelContractValidator

Rules

ContractId required

Reason required

Contract must be Under Review or Active

---

ActivateContractValidator

Rules

ContractId required

Contract must be Approved

---

CompleteContractValidator

Rules

ContractId required

Contract must be Active

---

ArchiveContractValidator

Rules

ContractId required

Reason required

Contract must be Completed or Cancelled

---

# 23. REST Controller

Controller

/api/v1/contracts

Controller Name

ContractController

Authorization

Authenticated

---

# 24. REST Endpoints

GET /api/v1/contracts

Returns

Paged Contract List

Default filter excludes Archived and SoftDelete records

Supports explicit includeArchived query parameter

---

GET /api/v1/contracts/{id}

Returns

Contract Details

Archived Contracts remain retrievable by identifier for authorized users

---

GET /api/v1/contracts/client/{clientId}

Returns

Contracts by Client

Identity association only

---

GET /api/v1/contracts/code/{code}

Returns

Contract Details

---

GET /api/v1/contracts/{id}/history

Returns

ContractHistoryResponse from centralized AuditEvents

---

POST /api/v1/contracts

Creates Contract

Returns

201 Created

Requires Active Client

---

PUT /api/v1/contracts/{id}

Updates Contract

Returns

200 OK

---

POST /api/v1/contracts/{id}/submit

Submits Contract for Approval

Returns

200 OK

Sets Status = Under Review

---

POST /api/v1/contracts/{id}/approve

Approves Contract

Returns

200 OK

Sets Status = Approved

---

POST /api/v1/contracts/{id}/cancel

Cancels Contract

Returns

200 OK

Sets Status = Cancelled

---

POST /api/v1/contracts/{id}/activate

Activates Contract

Returns

200 OK

Sets Status = Active

---

POST /api/v1/contracts/{id}/complete

Completes Contract

Returns

200 OK

---

POST /api/v1/contracts/{id}/archive

Archives Contract

Returns

200 OK

Sets Status = Archived and ArchivedAt

Does not set SoftDelete

---

Mission creation endpoints are not part of this specification.

Operations shall enforce Contract Status = Active before Mission creation according to BR-COM-008 and FR-OPS-001.

---

# 25. HTTP Status Codes

200 OK

201 Created

400 Validation Error

401 Unauthorized

403 Forbidden

404 Not Found

409 Business Rule Violation

409 Conflict for optimistic concurrency

422 Invalid State Transition

500 Internal Server Error

---

# 26. Authorization

Read

CommercialUser

SalesManager

OperationsManager

Administrator

---

Create

CommercialUser

SalesManager

---

Update

CommercialUser

SalesManager

---

Approve

SalesManager

Administrator

---

Cancel

SalesManager

Administrator

---

Activate

SalesManager

Administrator

---

Complete

SalesManager

Administrator

---

Archive

Administrator

---

# 27. Exception Handling

Throw BusinessRuleException

When

Contract Not Found

Client Not Found

Client Not Active

Duplicate ContractCode

Invalid State Transition

Invalid Approval

Archived Contract Update

Completed Contract Update

Cancelled Contract Update

Validation Failure

Attempt to authorize Mission creation for non-Active Contract

Every business exception shall return ProblemDetails.

---

# 28. Logging

Information

Contract Created

Contract Updated

Submitted For Approval

Contract Approved

Contract Cancelled

Contract Activated

Contract Completed

Contract Archived

Warning

Invalid Transition

Duplicate ContractCode

Approval Failure

Client Not Active

Error

Unexpected Exception

Persistence Failure

All logs shall include

ContractId

ContractCode

ClientId

CorrelationId

UserId

Timestamp UTC

Reference

NFR-036

NFR-066

---

# 29. Transaction Boundaries

Transactional boundaries follow ADR-010.

Create Contract

Single Contract Aggregate transaction

Client is validated by identity only

---

Update Contract

Single Contract Aggregate transaction

---

Submit Contract for Approval

Single Contract Aggregate transaction

---

Approve Contract

Single Contract Aggregate transaction

---

Cancel Contract

Single Contract Aggregate transaction

---

Activate Contract

Single Contract Aggregate transaction

---

Complete Contract

Single Contract Aggregate transaction

---

Archive Contract

Single Contract Aggregate transaction

Client Aggregate is never mutated by Contract operations.

Mission Aggregate is never mutated by Contract operations.

Domain Events shall be published only after successful Commit.

Audit Events shall be written only after successful Commit.

Failed transactions shall not produce Domain Events or Audit Events.

Reference

ADR-009

ADR-010

NFR-011

NFR-012

NFR-056

---

# 30. Dependency Injection

Register

IContractRepository

ContractRepository

ContractApplicationService

ContractDomainService

Validators

Controllers invoke Application Services directly.

No MediatR.

No CQRS framework.

Dependency Injection shall be used exclusively.

No service shall instantiate dependencies directly.

---

# End of Part 2

# 31. Persistence Mapping

ORM

Entity Framework Core 8

Database

PostgreSQL on Supabase

Schema

commercial

Table

contracts

Primary Key

contract_id

Mapping

contract_id                uuid                  PK

client_id                  uuid                  FK

contract_code              varchar(30)           UNIQUE

contract_name              varchar(200)

contract_type              smallint

description                varchar(4000)

start_date                 date

end_date                   date NULL

contract_value             numeric(18,2)

currency                   char(3)

billing_model              smallint

status                     smallint

account_manager_user_id    uuid

approved_by_user_id        uuid NULL

approved_at                timestamptz NULL

created_at                 timestamptz

updated_at                 timestamptz

archived_at                timestamptz NULL

soft_delete                boolean NOT NULL DEFAULT false

Optimistic concurrency uses the PostgreSQL xmin system column mapped through Entity Framework Core.

No SQL Server types are used.

client_id is an identity foreign key only.

Missions are not mapped as owned children of Contract.

Enumeration columns use EF Core value conversions.

API and Swagger expose enumeration names.

Database stores integer values.

Database object names follow snake_case.

Reference

ADR-010

ADR-012

Program_Architecture.md

Tech Stack

---

# 32. Database Constraints

PK_Contracts

ContractId

---

FK_Contracts_Clients

client_id

References

commercial.clients(client_id)

ON DELETE RESTRICT

---

UQ_ContractCode

ContractCode

---

CK_ContractValue

ContractValue >= 0

---

CK_EndDate

EndDate IS NULL

OR

EndDate > StartDate

---

CK_ContractStatus

Status IN

Draft

UnderReview

Approved

Active

Completed

Cancelled

Archived

---

ContractName

NOT NULL

---

ContractType

NOT NULL

---

BillingModel

NOT NULL

---

Currency

NOT NULL

---

Status

NOT NULL

---

CreatedAt

NOT NULL

---

SoftDelete

NOT NULL

DEFAULT false

---

# 33. Database Indexes

IX_ClientId

---

IX_ContractCode

Unique

---

IX_Status

---

IX_AccountManager

---

IX_StartDate

---

IX_EndDate

---

IX_Client_Status

Composite

(ClientId, Status)

---

IX_Status_StartDate

Composite

(Status, StartDate)

---

IX_SoftDelete_Status

Supports default business search exclusion

Indexes shall support operational planning and commercial reporting.

---

# 34. Migration

Migration Name

create_commercial_contracts

Migration Tool

Supabase CLI

Migration Location

supabase/migrations

Migration Responsibilities

Create table commercial.contracts

Create Foreign Key

Create Constraints

Create Indexes

No Seed Enumerations

Enumeration values are defined in code according to ADR-012

Entity Framework Core migrations are not permitted.

SQL migrations are version-controlled and immutable after execution.

Rollback Supported

YES

Migration shall be idempotent.

---

# 35. Concurrency

Optimistic Concurrency

Using

PostgreSQL xmin system column

Concurrent updates shall return

409 Conflict

Silent overwrite is prohibited.

Reference

NFR-044

---

# 36. Search

Supported Filters

ContractCode

ClientId

ContractName

ContractType

BillingModel

Status

AccountManager

StartDate

EndDate

CreatedFrom

CreatedTo

Archived

Pagination

Mandatory

Sorting

ContractName

StartDate

Status

ContractCode

Default behavior

Exclude Status = Archived

Exclude SoftDelete = true

Explicit Archived filter may include archived Contracts

Search shall support multiple combined filters.

Reference

ADR-011

FR-COM-011

NFR-042

NFR-060

---

# 37. Swagger

Every endpoint shall include

Summary

Description

Request Example

Response Example

HTTP Status Codes

Validation Errors

Business Rule Errors

Authorization Requirements

Enumeration schemas using enum names

Swagger shall be generated automatically.

Reference

ADR-012

NFR-039

NFR-064

---

# 38. Unit Tests

Minimum Coverage

90%

Required Tests

Create Contract for Active Client

Reject Create Contract for Suspended Client

Reject Create Contract for Archived Client

Update Contract

Submit For Approval

Approve Contract

Cancel Contract

Activate Contract

Complete Contract

Archive Contract

Only Active Contract authorizes Mission creation

Approved Contract does not authorize Mission creation

Cancelled Contract never authorizes Mission creation

Archived Contract never authorizes Mission creation

Duplicate ContractCode

Invalid State Transition

Invalid Approval

Validators

Repository Mock

Application Service

Domain Service

Business Rule Exceptions

Audit Event creation after successful commit

No Audit Event after failed transaction

Enumeration persistence conversion

Every implemented Business Rule shall have at least one Unit Test.

---

# 39. Integration Tests

Create Contract

Update Contract

Search Contract

List Client Contracts

Submit For Approval

Approve Contract

Cancel Contract

Activate Contract

Complete Contract

Archive Contract

Get Contract History from AuditEvents

Authentication

Authorization

REST Endpoints

Persistence

Migration

Transaction Rollback

Swagger

Health Check

Default search excludes Archived

Integration Tests shall execute against a real database.

---

# 40. Performance

Target

Typical CRUD operations should complete within interactive response times under expected MVP load.

Contract queries shall remain responsive through proper indexing.

Avoid N+1 queries.

Use projections whenever appropriate.

All operations shall support asynchronous execution.

CancellationToken shall be supported.

Reference

NFR-007

NFR-042

NFR-043

---

# 41. Observability

Metrics

Contracts Created

Contracts Approved

Contracts Cancelled

Contracts Activated

Contracts Completed

Contracts Archived

Active Contracts

Approval Time

Logs shall include

CorrelationId

RequestId

UserId

ClientId

ContractId

ContractCode

ExecutionTime

UTC Timestamp

Reference

NFR-036

NFR-066

NFR-067

---

# 42. Definition of Done

Implementation is complete only when

✓ Entity implemented

✓ Value Objects implemented

✓ Enumerations implemented as C# enums with EF Core smallint conversions

✓ Repository implemented for Contract Aggregate Root only

✓ Repository Tests approved

✓ Application Service implemented

✓ Domain Service implemented

✓ Validators implemented

✓ DTOs implemented

✓ REST Controller implemented

✓ Endpoints implemented including history

✓ Swagger generated with enum names

✓ Entity Framework Mapping implemented

✓ Migration created without enumeration seed data

✓ Database Constraints implemented

✓ Indexes created

✓ Unit Tests passing

✓ Integration Tests passing

✓ Logging implemented with CorrelationId

✓ Authorization implemented

✓ Archive implemented according to ADR-011

✓ SoftDelete reserved as technical field only and unused by business workflows

✓ PostgreSQL xmin optimistic concurrency implemented

✓ Approval workflow implemented

✓ Lifecycle state machine implemented according to BR-COM-011

✓ Operational authorization implemented according to BR-COM-008

✓ Domain Events published after successful commit

✓ Audit Events written to centralized AuditEvents after successful commit

✓ GetContractHistoryQuery reads AuditEvents only

✓ Code Review approved

✓ Reconciled Business Rules respected

✓ Functional Requirements satisfied

✓ Acceptance Criteria satisfied

✓ Non-Functional Requirements respected

✓ Consistency with EFS-001, EFS-002 and EFS-003 verified

✓ No architecture violations

✓ Documentation Update completed according to ADR-008

---

# 43. Implementation Deliverables

The implementation shall produce

## Domain Layer

Contract Entity

ContractStatus Enum

ContractType Enum

BillingModel Enum

Money Value Object

DateRange Value Object

Repository Interface

Domain Service

Domain Events

---

## Application Layer

Commands

Queries

Validators

DTOs

Application Service

Mappings

Audit Event emission after commit

Controllers invoke Application Services directly

No MediatR

No CQRS framework

---

## Infrastructure Layer

Repository

Entity Framework Configuration

Enumeration Conversions

Migration

Dependency Injection

Persistence

Centralized AuditEvents persistence integration

---

## API Layer

ContractController

Swagger

OpenAPI

Authentication

Authorization

ProblemDetails

---

## Tests

Unit Tests

Integration Tests

Repository Tests

API Tests

Business Rule Tests

Audit History Tests

Operational Authorization Tests

---

# 44. Acceptance Validation

The implementation shall satisfy

Business_Domains.md

Commercial_Domain.md

Business_Rules.md

Use_Cases.md

Functional_Requirements.md

Non_Functional_Requirements.md

User_Stories.md

Acceptance_Criteria.md

MVP_Feature_Matrix.md

MVP_1.0_Documentation_Readiness_Review.md

ADR-009

ADR-010

ADR-011

ADR-012

EFS-001 Lead Management

EFS-002 Client Management

EFS-003 Contact Management

No implementation may violate any approved document.

---

# 45. Traceability Matrix

| Engineering Concern | Business Rule | Functional Requirement | Acceptance Criteria |
| --- | --- | --- | --- |
| Create Contract | BR-COM-006, BR-COM-007 | FR-COM-005 | AC-COM-005 |
| Approve Contract | BR-COM-011 | FR-COM-006, FR-COM-008 | AC-COM-006 |
| Activate Contract | BR-COM-008, BR-COM-011 | FR-COM-008, FR-COM-009 | AC-COM-006 |
| Operational Authorization | BR-COM-008, BR-COM-009 | FR-COM-008, FR-OPS-001 | AC-COM-006 |
| Contract History | BR-COM-010 | FR-COM-007, FR-SYS-005 | AC-COM-005 |
| Archive Contract | BR-COM-013 | FR-COM-010, FR-COM-011 | AC-COM-005 |
| Aggregate Ownership | BR-COM-012 | FR-COM-005 | AC-COM-005 |
| Enumerations | BR-COM-014 | FR-SYS-008 | — |

---

# 46. Architecture Compliance

## ADR-009

Contract produces Domain Events and Audit Events after successful commit.

GetContractHistoryQuery reads centralized AuditEvents only.

Event Sourcing is not used.

## ADR-010

Contract is an Aggregate Root.

Contract references Client by identity.

Mission belongs to Operations and references Contract by identity.

Transactional boundaries remain Aggregate-aligned.

## ADR-011

Archive is the business inactivation model.

SoftDelete is technical only.

## ADR-012

Contract enumerations are C# enums persisted as smallint.

No lookup tables.

No enumeration seed data.

API and Swagger use enum names.

## Consistency with EFS-001 / EFS-002 / EFS-003

Lead conversion creates Client.

Client is Active before Contract creation.

Contact remains outside Contract Aggregate.

Mission creation is outside this specification and requires Active Contract.

---

# 47. Final Engineering Statement

This Engineering Functional Specification defines the complete implementation of the Contract Management capability for AgencyOS MVP 1.0.

This specification is derived from the reconciled Functional Documentation and approved Architecture Decision Records.

Implementation shall strictly follow this specification.

No assumptions are permitted.

Any ambiguity shall stop implementation and generate a blocking report.

This specification is the engineering input required for implementing Contract Management.

# End of Engineering Specification

EFS-004 – Contract Management

Status

READY FOR IMPLEMENTATION
