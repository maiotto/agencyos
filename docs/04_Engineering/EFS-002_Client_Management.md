# AgencyOS Engineering Specification

# EFS-002 – Client Management

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

This Engineering Functional Specification defines the complete implementation requirements for the Client Management capability of the Commercial Domain.

The Client entity represents an organization that has an established commercial relationship with the agency.

A Client exists only after successful commercial conversion of a Won Lead.

This document is a pure implementation specification derived from the reconciled Functional Documentation.

Implementation shall strictly follow this specification.

No architectural decisions may be introduced during implementation.

This specification shall not redesign the product, introduce new features, modify Business Rules or modify approved architecture.

---

# 2. References

This specification is derived from the following approved documents.

Product Vision

Product Principles

AgencyOS Baseline

Program_Architecture.md

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

These documents are frozen.

This specification shall not contradict them.

Authoritative Business Rules are defined exclusively in Business_Rules.md.

---

# 3. Business Context

A Client represents an organization authorized to establish commercial contracts with the agency.

Clients originate from Lead conversion only.

Direct Client creation without Lead conversion is not allowed in MVP 1.0.

Client Aggregate ownership:

- Contacts are child entities of Client
- Contracts reference Client by identity and are independent Aggregate Roots
- Missions belong to Operations and are not owned by Client

A Client is the commercial party associated with operational demand generated from Active Contracts.

Clients do not execute work.

Clients consume agency services.

Reference

BR-COM-003

BR-COM-012

ADR-010

---

# 4. Functional Objectives

The system shall allow users to:

Create Clients through Lead conversion

Maintain Client information

Activate Clients

Suspend Clients

Archive Clients

Search Clients

Retrieve Client details

Maintain Contacts through the Client Aggregate

Maintain complete commercial history

A Client may have multiple Contacts.

A Client may be associated with multiple Contracts.

Traceability

FR-COM-003

FR-COM-004

FR-COM-005

FR-COM-007

FR-COM-010

FR-COM-011

FR-SYS-005

US-COM-003

US-COM-004

AC-COM-003

AC-COM-004

---

# 5. Domain Ownership

Domain

Commercial

Aggregate Root

Client

Repository

IClientRepository

Application Service

ClientApplicationService

Domain Service

ClientDomainService

REST Controller

ClientController

Aggregate ownership follows ADR-010.

Client is an independent Commercial Aggregate Root.

Contact is a child entity of Client.

Contract is not part of the Client Aggregate.

Mission is not part of the Client Aggregate.

---

# 6. Aggregate Definition

Aggregate Root

Client

Child Entities

Contact

External References

OriginLeadId

Identity references only

Not owned

Contract

Mission

The Client Aggregate is responsible for Client invariants and Contact ownership.

Contracts are independent Aggregate Roots that reference Client by identity.

Missions belong to the Operations Domain.

Contacts are loaded and persisted exclusively through the Client Aggregate.

There is no Contact Aggregate Root repository.

Reference

ADR-010

BR-COM-005

BR-COM-006

BR-COM-012

---

# 7. Entity Definition

Entity Name

Client

Schema

commercial

Table

clients

Primary Key

client_id

Identifier Type

UUID

Lifecycle

Active

↓

Suspended

↓

Archived

or

Active

↓

Archived

State transitions shall be validated.

Official lifecycle statuses follow BR-COM-001 style closed enumerations for ClientStatus under BR-COM-014.

---

# 8. Entity Fields

## ClientId

Type

UUID

Required

Yes

Immutable

Yes

Generated automatically

---

## ClientCode

Type

String

Maximum Length

30

Unique

Yes

Generated automatically

Format

CLI-000001

Immutable

---

## OriginLeadId

Type

UUID

Required

Yes

Immutable

Yes

Identifies the Lead that originated the Client through conversion.

Supports historical traceability between Lead and Client.

Does not create Aggregate ownership of Lead.

Reference

BR-COM-002

BR-COM-003

---

## LegalName

Type

String

Maximum Length

200

Required

---

## TradeName

Type

String

Maximum Length

200

Optional

---

## TaxIdentifier

Type

String

Maximum Length

30

Required

Unique

Validation according to country rules

---

## Industry

Type

Enumeration

Optional

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

---

## CompanySize

Type

Enumeration

Optional

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

---

## Website

Type

String

Maximum Length

250

Optional

---

## Email

Type

String

Maximum Length

200

Required

RFC compliant

---

## Phone

Type

String

Maximum Length

30

Optional

---

## Status

Type

ClientStatus

Required

Default

Active

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

---

## AccountOwnerUserId

Type

UUID

Required

---

## Notes

Type

String

Maximum Length

4000

Optional

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

Populated when Client status becomes Archived.

Reference

ADR-011

BR-COM-013

---

## SoftDelete

Type

Boolean

Default

false

Technical field only.

Not used by Client business workflows.

Not exposed as a business command.

Reserved for exceptional maintenance according to ADR-011.

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

## ClientStatus

Active

Suspended

Archived

---

## CompanySize

Micro

Small

Medium

Large

Enterprise

---

## Industry

Technology

Manufacturing

Retail

Healthcare

Education

Finance

Services

Other

---

# 10. Value Objects

ClientCode

LegalName

TaxIdentifier

Email

PhoneNumber

Website

Each Value Object validates itself.

Primitive types shall not contain business validation.

---

# 11. Business Rules

Authoritative Business Rules are defined in Business_Rules.md.

This Engineering Specification implements the following Commercial Business Rules for Client Management.

## BR-COM-002 — Lead Conversion

Client creation is the Client-side result of Lead conversion Option B.

The originating Lead remains a Lead Aggregate Root with status Converted.

---

## BR-COM-003 — Client Creation and Mandatory Information

Clients originate from Lead conversion.

Direct Client creation without Lead conversion is not allowed in MVP 1.0.

Mandatory Client identification includes:

LegalName

TaxIdentifier

---

## BR-COM-004 — Multiple Contacts

A Client may have multiple Contacts.

Contacts do not own Contracts.

Contacts do not own Missions.

---

## BR-COM-005 — Contact Ownership

Every Contact belongs to exactly one Client.

Contact is a child entity of the Client Aggregate.

Contact is not an Aggregate Root.

Contact creation, update, activation, deactivation, archival and primary assignment occur through the Client Aggregate.

Only one Primary Contact is allowed per Client.

---

## BR-COM-006 — Contract Client Reference

Every Contract belongs to exactly one Client by identity reference.

Contract is an independent Aggregate Root.

---

## BR-COM-007 — Multiple Contracts per Client

A Client may be associated with multiple Contracts.

Only Active Clients may create new Contracts.

Suspended Clients cannot create Contracts.

Archived Clients cannot create Contracts.

---

## BR-COM-010 — Commercial Audit History

Client Aggregate Root shall generate Audit Events for successful state-changing operations.

GetClientHistoryQuery obtains history from centralized AuditEvents.

---

## BR-COM-012 — Commercial Aggregate Ownership

Client is a Commercial Aggregate Root.

Contact is a child of Client.

Contract and Mission are outside the Client Aggregate.

---

## BR-COM-013 — Archive Policy

Archive is the business inactivation model for Client.

Archived Clients have Status = Archived and ArchivedAt populated.

Archived Clients are read-only.

SoftDelete is not part of Client business workflows.

---

## BR-COM-014 — Closed Enumerations

ClientStatus, CompanySize and Industry follow ADR-012.

---

## Engineering Constraints

ClientCode is generated automatically.

ClientCode is immutable.

TaxIdentifier shall be unique.

Every Client must have one AccountOwnerUserId.

OriginLeadId is mandatory and immutable.

Clients shall never be physically deleted through business workflows.

Archived Clients cannot be restored automatically in MVP 1.0.

---

# 12. State Machine

Allowed transitions

Active

↓

Suspended

Suspended

↓

Active

Active

↓

Archived

Suspended

↓

Archived

Forbidden transitions

Archived

↓

Active

Archived

↓

Suspended

Undefined transitions shall generate BusinessRuleException.

---

# 13. Domain Invariants

A Client always has:

ClientId

ClientCode

OriginLeadId

LegalName

TaxIdentifier

Status

CreatedAt

AccountOwnerUserId

ClientId never changes.

ClientCode never changes.

OriginLeadId never changes.

TaxIdentifier is unique.

Archived Clients are immutable.

Clients cannot exist without LegalName.

Clients cannot exist without TaxIdentifier.

Clients cannot exist without OriginLeadId.

Client shall never own Contract as a child entity.

Client shall never own Mission as a child entity.

---

# 14. Domain Events

ClientCreated

ClientUpdated

ClientActivated

ClientSuspended

ClientArchived

AccountOwnerChanged

ContactCreated

ContactUpdated

PrimaryContactChanged

ContactActivated

ContactDeactivated

ContactArchived

Domain Events are immutable.

Domain Events express business facts inside the domain model.

Domain Events support application reactions and orchestration.

Domain Events are published after successful transaction commit.

Domain Events are not the system of record for long-term history.

Reference

ADR-009

---

# 14A. Audit Events

Every successful Client Aggregate state-changing operation shall produce an Audit Event.

This includes Contact changes persisted through the Client Aggregate.

Audit Events are persisted in the centralized AuditEvents model.

Audit Events are written only after successful transaction commit.

Failed transactions shall not produce Audit Events.

Audit Events are immutable.

GetClientHistoryQuery shall read exclusively from AuditEvents filtered by:

AggregateType = Client

AggregateId = ClientId

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

Administrators

Create

Sales Managers

Administrators

Create is available only as part of Lead conversion orchestration.

Update

Commercial Users

Sales Managers

Suspend

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

ClientApplicationService

Responsibilities

- Create Client from Lead conversion
- Update Client
- Activate Client
- Suspend Client
- Archive Client
- Change Account Owner
- Manage Contacts through Client Aggregate
- Search Clients
- Get Client Details
- Get Client History

The Application Service orchestrates use cases.

Business Rules remain inside the Domain Layer.

Create Client is invoked by Lead conversion orchestration defined in EFS-001.

ClientApplicationService shall not expose direct Client creation independent of OriginLeadId.

---

# 17. Domain Service

ClientDomainService

Responsibilities

Validate business invariants.

Validate lifecycle transitions.

Generate ClientCode.

Validate TaxIdentifier uniqueness.

Validate Primary Contact uniqueness.

Raise Domain Events.

The Domain Service shall never access Infrastructure.

The Domain Service shall not create Contracts.

The Domain Service shall not create Missions.

---

# 18. Repository

Interface

IClientRepository

Required Methods

Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken)

Task<Client?> GetByCodeAsync(string code, CancellationToken cancellationToken)

Task<Client?> GetByTaxIdentifierAsync(string taxIdentifier, CancellationToken cancellationToken)

Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken)

Task<Client?> GetByOriginLeadIdAsync(Guid originLeadId, CancellationToken cancellationToken)

Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)

Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken)

Task<bool> ExistsByTaxIdentifierAsync(string taxIdentifier, CancellationToken cancellationToken)

Task<bool> ExistsByOriginLeadIdAsync(Guid originLeadId, CancellationToken cancellationToken)

Task<PagedResult<Client>> SearchAsync(ClientSearchFilter filter, CancellationToken cancellationToken)

Task AddAsync(Client client, CancellationToken cancellationToken)

Task UpdateAsync(Client client, CancellationToken cancellationToken)

Task SaveChangesAsync(CancellationToken cancellationToken)

Repositories shall never contain business rules.

IClientRepository loads and persists Client including Contact child entities.

There is no IContactRepository as an Aggregate Root repository.

Contracts are not persisted through IClientRepository.

Reference

ADR-010

---

# 19. Commands

CreateClientFromLeadConversionCommand

Fields

OriginLeadId

LegalName

TaxIdentifier

TradeName

Industry

CompanySize

Website

Email

Phone

AccountOwnerUserId

Notes

Rules

OriginLeadId required

LegalName required

TaxIdentifier required

Email required

AccountOwnerUserId required

Invoked only by Lead conversion orchestration

Must not be used for direct Client creation without Lead conversion

One Client per OriginLeadId

---

UpdateClientCommand

Fields

ClientId

LegalName

TradeName

Industry

CompanySize

Website

Email

Phone

Notes

Rejected when Status is Archived

---

SuspendClientCommand

Fields

ClientId

Reason

---

ActivateClientCommand

Fields

ClientId

Requires Status = Suspended

---

ArchiveClientCommand

Fields

ClientId

Reason

Sets Status = Archived

Sets ArchivedAt

Does not use SoftDelete

---

ChangeAccountOwnerCommand

Fields

ClientId

NewOwnerUserId

Rejected when Status is Archived

---

AddContactCommand

Fields

ClientId

Contact fields defined by Contact child entity requirements

Persisted through Client Aggregate

---

UpdateContactCommand

Fields

ClientId

ContactId

Contact updatable fields

Persisted through Client Aggregate

---

SetPrimaryContactCommand

Fields

ClientId

ContactId

Enforces single Primary Contact per Client

---

ArchiveContactCommand

Fields

ClientId

ContactId

Reason

Uses Archive, not SoftDelete

---

# 20. Queries

GetClientByIdQuery

GetClientByCodeQuery

SearchClientsQuery

GetClientHistoryQuery

GetClientContactsQuery

GetClientHistoryQuery obtains history exclusively from centralized AuditEvents.

SearchClientsQuery excludes Archived records by default.

SearchClientsQuery excludes SoftDelete = true records from normal business retrieval.

Archived records may be included only when explicitly requested.

Reference

ADR-009

ADR-011

FR-COM-011

---

# 21. DTOs

CreateClientFromLeadConversionRequest

CreateClientResponse

UpdateClientRequest

ClientResponse

ClientSummaryResponse

ClientSearchResponse

ClientHistoryResponse

PagedClientResponse

SuspendClientRequest

ActivateClientRequest

ArchiveClientRequest

ContactResponse

ContactSummaryResponse

DTOs shall never expose Domain Entities.

Enumeration properties in DTOs use enum names.

ClientResponse shall include

ClientId

ClientCode

OriginLeadId

LegalName

TaxIdentifier

Status

AccountOwnerUserId

CreatedAt

ArchivedAt when applicable

Reference

ADR-012

---

# 22. Validators

CreateClientFromLeadConversionValidator

Rules

OriginLeadId required

LegalName required

TaxIdentifier required

Email required

AccountOwnerUserId required

TaxIdentifier format valid

Website valid when informed

Email RFC compliant

OriginLeadId must not already have a Client

---

UpdateClientValidator

Rules

ClientId required

LegalName required

Email valid

Website valid

Client must not be Archived

---

SuspendClientValidator

ClientId required

Reason required

Client must be Active

---

ActivateClientValidator

ClientId required

Client must be Suspended

---

ArchiveClientValidator

ClientId required

Reason required

Client must not already be Archived

---

ChangeAccountOwnerValidator

ClientId required

NewOwnerUserId required

Client must not be Archived

---

AddContactValidator

ClientId required

Contact mandatory fields required

Client must not be Archived

---

SetPrimaryContactValidator

ClientId required

ContactId required

Contact must belong to Client

---

# 23. REST Controller

Controller

/api/v1/clients

Controller Name

ClientController

Authorization

Authenticated

---

# 24. REST Endpoints

GET /api/v1/clients

Returns

Paged Client List

Default filter excludes Archived and SoftDelete records

Supports explicit includeArchived query parameter

---

GET /api/v1/clients/{id}

Returns

Client Details

Archived Clients remain retrievable by identifier for authorized users

---

GET /api/v1/clients/code/{code}

Returns

Client Details

---

GET /api/v1/clients/{id}/history

Returns

ClientHistoryResponse from centralized AuditEvents

---

GET /api/v1/clients/{id}/contacts

Returns

Contacts owned by the Client Aggregate

---

PUT /api/v1/clients/{id}

Updates Client

Returns

200 OK

Rejected for Archived Clients

---

POST /api/v1/clients/{id}/activate

Activates Client

Returns

200 OK

---

POST /api/v1/clients/{id}/suspend

Suspends Client

Returns

200 OK

---

POST /api/v1/clients/{id}/archive

Archives Client

Returns

200 OK

Sets Status = Archived and ArchivedAt

Does not set SoftDelete

---

POST /api/v1/clients/{id}/change-owner

Changes Account Owner

Returns

200 OK

---

POST /api/v1/clients/{id}/contacts

Adds Contact through Client Aggregate

Returns

201 Created

---

PUT /api/v1/clients/{id}/contacts/{contactId}

Updates Contact through Client Aggregate

Returns

200 OK

---

POST /api/v1/clients/{id}/contacts/{contactId}/primary

Sets Primary Contact

Returns

200 OK

---

POST /api/v1/clients/{id}/contacts/{contactId}/archive

Archives Contact through Client Aggregate

Returns

200 OK

---

Client creation endpoint

Direct POST /api/v1/clients is not provided.

Client creation occurs only through Lead conversion endpoint defined in EFS-001:

POST /api/v1/leads/{id}/convert

Reference

BR-COM-003

FR-COM-003

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

Administrator

---

Create from Lead conversion

SalesManager

Administrator

---

Update

CommercialUser

SalesManager

---

Suspend

SalesManager

Administrator

---

Activate

SalesManager

Administrator

---

Archive

Administrator

---

Change Account Owner

SalesManager

Administrator

---

Manage Contacts

CommercialUser

SalesManager

Administrator

---

# 27. Exception Handling

Throw BusinessRuleException

When

Duplicate ClientCode

Duplicate TaxIdentifier

Duplicate Email

Duplicate OriginLeadId

Archived Client Update

Invalid State Transition

Client Not Found

Validation Failure

More than one Primary Contact

Contact without Client

Contract creation attempted for Suspended or Archived Client

All business exceptions shall return ProblemDetails.

---

# 28. Logging

Information

Client Created

Client Updated

Client Activated

Client Suspended

Client Archived

Account Owner Changed

Contact Created

Contact Updated

Primary Contact Changed

Contact Archived

Warning

Duplicate TaxIdentifier

Duplicate Email

Invalid Transition

Error

Unexpected Exception

Persistence Failure

All log entries shall include

ClientId

ClientCode

CorrelationId

UserId

Timestamp UTC

Reference

NFR-036

NFR-066

---

# 29. Transaction Boundaries

Transactional boundaries follow ADR-010.

Create Client from Lead conversion

Client Aggregate transactional boundary

Invoked as step 1 of Lead conversion orchestration defined in EFS-001

Does not include Lead Aggregate mutation in the same Aggregate consistency boundary

---

Update Client

Single Client Aggregate transaction

---

Activate Client

Single Client Aggregate transaction

---

Suspend Client

Single Client Aggregate transaction

---

Archive Client

Single Client Aggregate transaction

---

Change Account Owner

Single Client Aggregate transaction

---

Contact commands

Single Client Aggregate transaction

Contacts are persisted with the Client Aggregate

---

Contract operations

Outside Client Aggregate transactional boundary

Contract Aggregate is independent

Client status is validated by identity reference only

---

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

IClientRepository

ClientRepository

ClientApplicationService

ClientDomainService

Validators

Controllers invoke Application Services directly.

No MediatR.

No CQRS framework.

No dependency shall be instantiated manually.

Dependency Injection shall be used exclusively.

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

clients

Primary Key

client_id

Mapping

client_id                 uuid                  PK

client_code               varchar(30)           UNIQUE

origin_lead_id            uuid                  NOT NULL

legal_name                varchar(200)

trade_name                varchar(200)

tax_identifier            varchar(30)

industry                  smallint

company_size              smallint

website                   varchar(250)

email                     varchar(200)

phone                     varchar(30)

status                    smallint

account_owner_user_id     uuid

notes                     varchar(4000)

created_at                timestamptz

updated_at                timestamptz

archived_at               timestamptz NULL

soft_delete               boolean NOT NULL DEFAULT false

Optimistic concurrency uses the PostgreSQL xmin system column mapped through Entity Framework Core.

No SQL Server types are used.

Contact child entities are persisted in commercial.client_contacts and mapped as owned/child collection of Client.

Contracts are not mapped as owned children of Client.

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

PK_Clients

ClientId

---

UQ_ClientCode

ClientCode

---

UQ_TaxIdentifier

TaxIdentifier

---

UQ_OriginLeadId

OriginLeadId

---

CK_ClientStatus

Status IN (Active, Suspended, Archived)

---

LegalName

NOT NULL

---

TaxIdentifier

NOT NULL

---

OriginLeadId

NOT NULL

---

Email

NOT NULL

---

AccountOwnerUserId

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

IX_ClientCode

Unique

---

IX_TaxIdentifier

Unique

---

IX_OriginLeadId

Unique

---

IX_Email

---

IX_Status

---

IX_AccountOwner

---

IX_LegalName

---

IX_CreatedAt

---

IX_Status_CreatedAt

Composite

(Status, CreatedAt)

---

IX_SoftDelete_Status

Supports default business search exclusion

Indexes shall support dashboard and commercial searches.

---

# 34. Migration

Migration Name

create_commercial_clients

Migration Tool

Supabase CLI

Migration Location

supabase/migrations

Migration Responsibilities

Create table commercial.clients

Create table commercial.client_contacts as child persistence of Client Aggregate

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

Conflicting updates shall return

409 Conflict

Silent overwrite is prohibited.

Reference

NFR-044

---

# 36. Search

Supported Filters

ClientCode

LegalName

TradeName

TaxIdentifier

Industry

CompanySize

Status

Email

AccountOwner

OriginLeadId

CreatedFrom

CreatedTo

Archived

Pagination

Mandatory

Sorting

LegalName

CreatedAt

Status

ClientCode

Default behavior

Exclude Status = Archived

Exclude SoftDelete = true

Explicit Archived filter may include archived Clients

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

Create Client from Lead conversion

Reject direct Client creation without OriginLeadId

Update Client

Suspend Client

Activate Client

Archive Client

Change Account Owner

Add Contact through Client Aggregate

Set Primary Contact uniqueness

Archive Contact through Client Aggregate

Duplicate ClientCode

Duplicate TaxIdentifier

Duplicate Email

Duplicate OriginLeadId

Archived Client Update

Invalid State Transition

Validators

Repository Mock

Application Service

Domain Service

Business Rule Exceptions

Audit Event creation after successful commit

No Audit Event after failed transaction

Enumeration persistence conversion

Only Active Client may be associated with new Contract creation

Every implemented Business Rule shall have at least one Unit Test.

---

# 39. Integration Tests

Create Client from Lead conversion

Update Client

Search Client

Suspend Client

Activate Client

Archive Client

Contact persistence through Client Aggregate

Get Client History from AuditEvents

Authentication

Authorization

REST Endpoints

Persistence

Migration

Transaction Rollback

Swagger

Health Check

Default search excludes Archived

No direct POST /api/v1/clients creation endpoint

Integration Tests shall execute against a real database.

---

# 40. Performance

Target

Typical CRUD operations should complete within interactive response times under expected MVP load.

Client searches shall remain responsive through proper indexing.

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

Clients Created

Clients Activated

Clients Suspended

Clients Archived

Active Clients

Suspended Clients

Archived Clients

Logs shall include

CorrelationId

RequestId

UserId

ClientId

ClientCode

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

✓ Repository implemented for Client Aggregate Root only

✓ Contact child persistence through Client Aggregate

✓ Repository Tests approved

✓ Application Service implemented

✓ Domain Service implemented

✓ Validators implemented

✓ DTOs implemented

✓ REST Controller implemented

✓ Endpoints implemented including history and contacts

✓ No direct Client creation endpoint independent of Lead conversion

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

✓ Domain Events published after successful commit

✓ Audit Events written to centralized AuditEvents after successful commit

✓ GetClientHistoryQuery reads AuditEvents only

✓ Client creation implements Lead conversion origin with OriginLeadId

✓ Contract relationship remains identity reference only

✓ Code Review approved

✓ Reconciled Business Rules respected

✓ Functional Requirements satisfied

✓ Acceptance Criteria satisfied

✓ Non-Functional Requirements respected

✓ No architecture violations

✓ Documentation Update completed according to ADR-008

---

# 43. Implementation Deliverables

The implementation shall produce

## Domain Layer

Client Entity

Contact Child Entity

ClientStatus Enum

Industry Enum

CompanySize Enum

Value Objects

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

ClientController

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

Lead Conversion Origin Tests

Contact Ownership Tests

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

EFS-001 Lead Management conversion orchestration

No implementation may violate any approved document.

---

# 45. Traceability Matrix

| Engineering Concern | Business Rule | Functional Requirement | Acceptance Criteria |
| --- | --- | --- | --- |
| Create Client from Lead conversion | BR-COM-002, BR-COM-003 | FR-COM-003 | AC-COM-003 |
| Maintain Client | BR-COM-003, BR-COM-013 | FR-COM-003 | AC-COM-003 |
| Contact ownership | BR-COM-004, BR-COM-005, BR-COM-012 | FR-COM-004 | AC-COM-004 |
| Contract association | BR-COM-006, BR-COM-007 | FR-COM-005 | AC-COM-005 |
| Client History | BR-COM-010 | FR-COM-007, FR-SYS-005 | AC-COM-003 |
| Archive Client | BR-COM-013 | FR-COM-010, FR-COM-011 | AC-COM-003 |
| Enumerations | BR-COM-014 | FR-SYS-008 | — |

---

# 46. Architecture Compliance

## ADR-009

Client produces Domain Events and Audit Events after successful commit.

GetClientHistoryQuery reads centralized AuditEvents only.

Event Sourcing is not used.

## ADR-010

Client is an Aggregate Root.

Contact is a child entity of Client.

Contract is an independent Aggregate Root referencing Client by identity.

Mission belongs to Operations.

Transactional boundaries remain Aggregate-aligned.

## ADR-011

Archive is the business inactivation model.

SoftDelete is technical only.

## ADR-012

Client enumerations are C# enums persisted as smallint.

No lookup tables.

No enumeration seed data.

API and Swagger use enum names.

---

# 47. Final Engineering Statement

This Engineering Functional Specification defines the complete implementation of the Client Management capability for AgencyOS MVP 1.0.

This specification is derived from the reconciled Functional Documentation and approved Architecture Decision Records.

Implementation shall strictly follow this specification.

No assumptions are permitted.

Any ambiguity shall stop implementation and generate a blocking report.

This specification is the engineering input required for implementing Client Management.

# End of Engineering Specification

EFS-002 – Client Management

Status

READY FOR IMPLEMENTATION
