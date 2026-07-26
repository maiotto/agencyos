# AgencyOS Engineering Specification

# EFS-003 – Contact Management

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

This Engineering Functional Specification defines the complete implementation requirements for the Contact Management capability of the Commercial Domain.

Contacts represent the individuals associated with a Client organization.

Contacts are communication entities only.

Contacts do not own Contracts.

Contacts do not own Missions.

Contacts belong to exactly one Client.

Contact is a child entity of the Client Aggregate.

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

EFS-002 Client Management

These documents are frozen.

This specification shall not contradict them.

Authoritative Business Rules are defined exclusively in Business_Rules.md.

---

# 3. Business Context

Every Client may have one or more Contacts.

A Contact represents a person responsible for commercial communication.

Examples

Commercial Manager

Marketing Manager

Procurement

CEO

Financial Contact

Operational Contact

A Contact may be marked as the Primary Contact.

Only one Primary Contact is allowed per Client.

Contact ownership and Primary Contact rules follow BR-COM-004 and BR-COM-005.

---

# 4. Functional Objectives

The system shall allow users to:

Create Contacts through the Client Aggregate

Update Contacts through the Client Aggregate

Activate Contacts

Deactivate Contacts

Archive Contacts

Search Contacts

Retrieve Contact details

Assign Primary Contact

Maintain Contact history

Traceability

FR-COM-004

FR-COM-007

FR-COM-010

FR-COM-011

FR-SYS-005

US-COM-004

AC-COM-004

---

# 5. Domain Ownership

Domain

Commercial

Aggregate Root

Client

Child Entity

Contact

Repository

IClientRepository

Application Service

ContactApplicationService

Domain Service

ContactDomainService

REST Controller

ContactController

Contacts belong to the Client Aggregate.

There is no Contact Aggregate Root.

There is no IContactRepository as an Aggregate Root repository.

Contact persistence occurs exclusively through IClientRepository.

Reference

ADR-010

BR-COM-005

BR-COM-012

EFS-002

---

# 6. Aggregate Definition

Aggregate Root

Client

Child Entity

Contact

Relationships

One Client

↓

Many Contacts

A Contact shall never exist without a Client.

Contact creation, update, activation, deactivation, archival and primary assignment occur through the Client Aggregate consistency boundary.

Contracts are outside Contact ownership.

Missions are outside Contact ownership.

Reference

ADR-010

---

# 7. Entity Definition

Entity Name

Contact

Schema

commercial

Table

client_contacts

Primary Key

contact_id

Identifier Type

UUID

Lifecycle

Active

↓

Inactive

↓

Archived

or

Active

↓

Archived

Transitions shall be validated.

---

# 8. Entity Fields

## ContactId

UUID

Generated automatically

Immutable

---

## ClientId

UUID

Required

FK

Immutable

References Client Aggregate Root by identity ownership.

---

## ContactCode

String

Maximum Length

30

Generated automatically

Unique

Format

CNT-000001

Immutable

---

## FullName

String

Maximum Length

150

Required

---

## JobTitle

String

Maximum Length

100

Optional

---

## Department

String

Maximum Length

100

Optional

---

## Email

String

Maximum Length

200

Required

RFC compliant

---

## Phone

String

Maximum Length

30

Optional

---

## Mobile

String

Maximum Length

30

Optional

---

## PreferredCommunication

Enumeration

Optional

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

---

## IsPrimary

Boolean

Default

False

Only one Contact with IsPrimary = true is allowed per Client.

---

## Status

ContactStatus

Default

Active

Persistence

smallint

API Representation

Enum name

Reference

ADR-012

---

## Notes

String

Maximum Length

2000

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

Populated when Contact status becomes Archived.

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

Not used by Contact business workflows.

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

## ContactStatus

Active

Inactive

Archived

---

## PreferredCommunication

Email

Phone

Mobile

WhatsApp

Teams

Other

---

# 10. Value Objects

ContactCode

PersonName

Email

PhoneNumber

MobileNumber

JobTitle

Department

Every Value Object validates itself.

---

# 11. Business Rules

Authoritative Business Rules are defined in Business_Rules.md.

This Engineering Specification implements the following Commercial Business Rules for Contact Management.

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

When a Contact is designated as Primary, any previous Primary Contact for the same Client ceases to be Primary.

---

## BR-COM-010 — Commercial Audit History

Contact changes are audited as Client Aggregate state-changing operations.

GetContactHistoryQuery obtains history from centralized AuditEvents.

---

## BR-COM-012 — Commercial Aggregate Ownership

Contact is a child of Client.

Repositories exist only for Aggregate Roots.

IClientRepository is the persistence boundary for Contact.

---

## BR-COM-013 — Archive Policy

Archive is the business inactivation model for Contact.

Archived Contacts have Status = Archived and ArchivedAt populated.

Archived Contacts are read-only.

SoftDelete is not part of Contact business workflows.

---

## BR-COM-014 — Closed Enumerations

ContactStatus and PreferredCommunication follow ADR-012.

---

## Engineering Constraints

ClientId is immutable.

ContactCode is generated automatically.

ContactCode is immutable.

FullName is mandatory.

Email is mandatory.

Client must exist and must not be Archived before creating Contacts.

Contacts shall never be physically deleted through business workflows.

Archived Contacts cannot become Active or Inactive.

Email uniqueness is enforced per Client.

---

# 12. State Machine

Allowed transitions

Active

↓

Inactive

Inactive

↓

Active

Active

↓

Archived

Inactive

↓

Archived

Forbidden transitions

Archived

↓

Active

Archived

↓

Inactive

Any undefined transition shall generate BusinessRuleException.

---

# 13. Domain Invariants

Every Contact always has

ContactId

ClientId

ContactCode

FullName

Email

Status

CreatedAt

ClientId never changes.

ContactCode never changes.

Only one Primary Contact exists per Client.

Archived Contacts are immutable.

Contact shall never exist without Client.

Contact shall never own Contract.

Contact shall never own Mission.

---

# 14. Domain Events

ContactCreated

ContactUpdated

PrimaryContactChanged

ContactActivated

ContactDeactivated

ContactArchived

Domain Events are immutable.

Domain Events express business facts inside the domain model.

Domain Events support application reactions and orchestration.

Domain Events are published after successful transaction commit of the Client Aggregate.

Domain Events are not the system of record for long-term history.

Reference

ADR-009

---

# 14A. Audit Events

Every successful Contact state-changing operation shall produce an Audit Event.

Because Contact is a child entity, Audit Events are recorded against the owning Client Aggregate.

Audit Events are persisted in the centralized AuditEvents model.

Audit Events are written only after successful Client Aggregate transaction commit.

Failed transactions shall not produce Audit Events.

Audit Events are immutable.

GetContactHistoryQuery shall read exclusively from AuditEvents filtered by:

AggregateType = Client

AggregateId = ClientId

EventType related to Contact operations

and/or Payload.ContactId = ContactId

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

Commercial Users

Sales Managers

Update

Commercial Users

Sales Managers

Archive

Sales Managers

Administrators

Set Primary Contact

Sales Managers

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

ContactApplicationService

Responsibilities

- Create Contact through Client Aggregate
- Update Contact through Client Aggregate
- Activate Contact
- Deactivate Contact
- Archive Contact
- Set Primary Contact
- Search Contacts
- Get Contact Details
- Get Contact History

The Application Service coordinates use cases.

Business Rules remain exclusively in the Domain Layer.

ContactApplicationService loads the Client Aggregate through IClientRepository, mutates Contact child entities on the Aggregate, and persists through IClientRepository.

ContactApplicationService shall not persist Contact independently of Client.

---

# 17. Domain Service

ContactDomainService

Responsibilities

Validate business invariants.

Validate lifecycle transitions.

Generate ContactCode.

Guarantee only one Primary Contact per Client.

Raise Domain Events for Contact changes within the Client Aggregate.

The Domain Service shall never access Infrastructure.

The Domain Service shall never create Contracts or Missions.

Primary Contact replacement shall clear IsPrimary on the previous Primary Contact within the same Client Aggregate operation.

---

# 18. Repository

Interface

IClientRepository

Contact Management uses the Client Aggregate Root repository defined in EFS-002.

Required Contact-related access through IClientRepository

Load Client Aggregate including Contacts

Persist Client Aggregate including Contact child collection

Query support for Contact search may use read-model projections, but write operations always go through Client Aggregate persistence.

There is no IContactRepository Aggregate Root interface.

Repositories shall never implement business logic.

Reference

ADR-010

EFS-002

---

# 19. Commands

CreateContactCommand

Fields

ClientId

FullName

JobTitle

Department

Email

Phone

Mobile

PreferredCommunication

IsPrimary

Notes

Persisted through Client Aggregate

Rejected when Client is Archived

---

UpdateContactCommand

Fields

ClientId

ContactId

FullName

JobTitle

Department

Email

Phone

Mobile

PreferredCommunication

Notes

Rejected when Contact is Archived

Rejected when Client is Archived

---

ActivateContactCommand

Fields

ClientId

ContactId

Requires Contact Status = Inactive

---

DeactivateContactCommand

Fields

ClientId

ContactId

Reason

Requires Contact Status = Active

---

ArchiveContactCommand

Fields

ClientId

ContactId

Reason

Sets Status = Archived

Sets ArchivedAt

Does not use SoftDelete

---

SetPrimaryContactCommand

Fields

ClientId

ContactId

Contact must belong to Client

Previous Primary Contact for the same Client is cleared in the same Client Aggregate transaction

---

# 20. Queries

GetContactByIdQuery

GetContactByCodeQuery

GetContactsByClientQuery

SearchContactsQuery

GetContactHistoryQuery

GetContactHistoryQuery obtains history exclusively from centralized AuditEvents.

SearchContactsQuery excludes Archived records by default.

SearchContactsQuery excludes SoftDelete = true records from normal business retrieval.

Archived records may be included only when explicitly requested.

Reference

ADR-009

ADR-011

FR-COM-011

---

# 21. DTOs

CreateContactRequest

CreateContactResponse

UpdateContactRequest

ContactResponse

ContactSummaryResponse

ContactSearchResponse

ContactHistoryResponse

PagedContactResponse

SetPrimaryContactRequest

DTOs shall never expose Domain Entities.

Enumeration properties in DTOs use enum names.

ContactResponse shall include

ContactId

ContactCode

ClientId

FullName

Email

IsPrimary

Status

CreatedAt

ArchivedAt when applicable

Reference

ADR-012

---

# 22. Validators

CreateContactValidator

Rules

ClientId required

FullName required

Email required

Email RFC compliant

PreferredCommunication valid

Client must exist

Client must not be Archived

---

UpdateContactValidator

Rules

ClientId required

ContactId required

FullName required

Email valid

Contact must belong to Client

Contact must not be Archived

---

ActivateContactValidator

ClientId required

ContactId required

Contact must be Inactive

---

DeactivateContactValidator

ClientId required

ContactId required

Reason required

Contact must be Active

---

ArchiveContactValidator

ClientId required

ContactId required

Reason required

Contact must not already be Archived

---

SetPrimaryContactValidator

ClientId required

ContactId required

Contact belongs to Client

Contact must not be Archived

---

# 23. REST Controller

Controller

Contact endpoints are exposed under the Client Aggregate API surface.

Primary route prefix

/api/v1/clients/{clientId}/contacts

Controller Name

ContactController

Authorization

Authenticated

Compatibility read routes under /api/v1/contacts may exist for search and retrieval by ContactId or ContactCode, but all write operations must load and persist the Client Aggregate.

Reference

EFS-002

ADR-010

---

# 24. REST Endpoints

GET /api/v1/clients/{clientId}/contacts

Returns

Client Contact List

Default filter excludes Archived and SoftDelete records

---

GET /api/v1/contacts/{id}

Returns

Contact Details

Archived Contacts remain retrievable by identifier for authorized users

---

GET /api/v1/contacts/code/{code}

Returns

Contact Details

---

GET /api/v1/contacts/{id}/history

Returns

ContactHistoryResponse from centralized AuditEvents

---

GET /api/v1/contacts

Returns

Paged Contact List

Default filter excludes Archived and SoftDelete records

Supports explicit includeArchived query parameter

---

POST /api/v1/clients/{clientId}/contacts

Creates Contact through Client Aggregate

Returns

201 Created

---

PUT /api/v1/clients/{clientId}/contacts/{contactId}

Updates Contact through Client Aggregate

Returns

200 OK

Rejected for Archived Contacts

---

POST /api/v1/clients/{clientId}/contacts/{contactId}/activate

Activates Contact

Returns

200 OK

---

POST /api/v1/clients/{clientId}/contacts/{contactId}/deactivate

Deactivates Contact

Returns

200 OK

---

POST /api/v1/clients/{clientId}/contacts/{contactId}/archive

Archives Contact

Returns

200 OK

Sets Status = Archived and ArchivedAt

Does not set SoftDelete

---

POST /api/v1/clients/{clientId}/contacts/{contactId}/primary

Defines Primary Contact

Returns

200 OK

Clears previous Primary Contact for the same Client in the same transaction

---

Standalone POST /api/v1/contacts write endpoints that bypass Client Aggregate ownership are not provided.

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

Create

CommercialUser

SalesManager

---

Update

CommercialUser

SalesManager

---

Activate

CommercialUser

SalesManager

---

Deactivate

CommercialUser

SalesManager

---

Archive

SalesManager

Administrator

---

Set Primary Contact

SalesManager

Administrator

---

# 27. Exception Handling

Throw BusinessRuleException

When

Client Not Found

Client Archived

Contact Not Found

Duplicate ContactCode

Duplicate Email for Client

Archived Contact Update

Invalid State Transition

More Than One Primary Contact

Contact does not belong to Client

Validation Failure

Every business exception shall return ProblemDetails.

---

# 28. Logging

Information

Contact Created

Contact Updated

Primary Contact Changed

Contact Activated

Contact Deactivated

Contact Archived

Warning

Duplicate Email

Primary Contact Conflict

Invalid Transition

Error

Unexpected Exception

Persistence Failure

All logs shall include

ContactId

ContactCode

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

Create Contact

Single Client Aggregate transaction

---

Update Contact

Single Client Aggregate transaction

---

Activate Contact

Single Client Aggregate transaction

---

Deactivate Contact

Single Client Aggregate transaction

---

Archive Contact

Single Client Aggregate transaction

---

Set Primary Contact

Single Client Aggregate transaction

When a Contact becomes Primary, the previous Primary Contact shall automatically have IsPrimary set to False within the same Client Aggregate transaction.

Domain Events shall be published only after successful Client Aggregate Commit.

Audit Events shall be written only after successful Client Aggregate Commit.

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

ContactApplicationService

ContactDomainService

Validators

Controllers invoke Application Services directly.

No MediatR.

No CQRS framework.

Do not register IContactRepository as an Aggregate Root repository.

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

client_contacts

Primary Key

contact_id

Mapping

contact_id                  uuid                  PK

client_id                   uuid                  FK

contact_code                varchar(30)           UNIQUE

full_name                   varchar(150)

job_title                   varchar(100)

department                  varchar(100)

email                       varchar(200)

phone                       varchar(30)

mobile                      varchar(30)

preferred_communication     smallint

is_primary                  boolean

status                      smallint

notes                       varchar(2000)

created_at                  timestamptz

updated_at                  timestamptz

archived_at                 timestamptz NULL

soft_delete                 boolean NOT NULL DEFAULT false

Optimistic concurrency uses the PostgreSQL xmin system column mapped through Entity Framework Core.

No SQL Server types are used.

Contact is mapped as a child collection of the Client Aggregate.

Contact rows are never written outside Client Aggregate persistence.

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

PK_ClientContacts

ContactId

---

FK_ClientContacts_Clients

client_id

References

commercial.clients(client_id)

ON DELETE RESTRICT

---

UQ_ContactCode

ContactCode

---

CK_PrimaryContact

Only one Contact with

IsPrimary = True

per Client

Implemented through filtered unique index.

---

FullName

NOT NULL

---

Email

NOT NULL

---

ClientId

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

IX_ContactCode

Unique

---

IX_Email

---

IX_Status

---

IX_IsPrimary

---

IX_Client_Status

Composite

(ClientId, Status)

---

UX_Client_PrimaryContact

Filtered Unique

(ClientId)

WHERE IsPrimary = 1

---

IX_SoftDelete_Status

Supports default business search exclusion

Indexes shall support client navigation and dashboard queries.

---

# 34. Migration

Migration Name

create_commercial_client_contacts

Migration Tool

Supabase CLI

Migration Location

supabase/migrations

Migration Responsibilities

Create table commercial.client_contacts

Create Foreign Keys

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

ContactCode

ClientId

FullName

Email

JobTitle

Department

Status

PreferredCommunication

PrimaryContact

CreatedFrom

CreatedTo

Archived

Pagination

Mandatory

Sorting

FullName

CreatedAt

Status

ContactCode

Default behavior

Exclude Status = Archived

Exclude SoftDelete = true

Explicit Archived filter may include archived Contacts

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

Create Contact through Client Aggregate

Update Contact through Client Aggregate

Activate Contact

Deactivate Contact

Archive Contact

Set Primary Contact

Primary Contact Replacement clears previous Primary

Reject Contact persistence outside Client Aggregate

Duplicate ContactCode

Duplicate Email for Client

Invalid State Transition

Archived Contact Immutability

Validators

Client Aggregate persistence mock

Application Service

Domain Service

Business Rule Exceptions

Audit Event creation after successful commit

No Audit Event after failed transaction

Enumeration persistence conversion

Every implemented Business Rule shall have at least one Unit Test.

---

# 39. Integration Tests

Create Contact through Client Aggregate

Update Contact

Search Contact

List Client Contacts

Activate Contact

Deactivate Contact

Archive Contact

Set Primary Contact

Get Contact History from AuditEvents

Authentication

Authorization

REST Endpoints

Persistence through IClientRepository

Migration

Transaction Rollback

Swagger

Health Check

Default search excludes Archived

No IContactRepository Aggregate Root write path

Integration Tests shall execute against a real database.

---

# 40. Performance

Target

Typical CRUD operations should complete within interactive response times under expected MVP load.

Client contact retrieval shall remain responsive through proper indexing.

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

Contacts Created

Contacts Archived

Contacts Activated

Contacts Deactivated

Primary Contact Changes

Contacts Per Client

Logs shall include

CorrelationId

RequestId

UserId

ClientId

ContactId

ContactCode

ExecutionTime

UTC Timestamp

Reference

NFR-036

NFR-066

NFR-067

---

# 42. Definition of Done

Implementation is complete only when

✓ Contact child entity implemented

✓ Value Objects implemented

✓ Enumerations implemented as C# enums with EF Core smallint conversions

✓ Persistence implemented through IClientRepository only

✓ No IContactRepository Aggregate Root write path

✓ Repository Tests approved

✓ Application Service implemented against Client Aggregate

✓ Domain Service implemented

✓ Validators implemented

✓ DTOs implemented

✓ REST Controller implemented

✓ Endpoints implemented including history

✓ Write endpoints nested under Client Aggregate API surface

✓ Swagger generated with enum names

✓ Entity Framework Mapping implemented as Client child collection

✓ Migration created without enumeration seed data

✓ Database Constraints implemented

✓ Filtered unique Primary Contact index created

✓ Indexes created

✓ Unit Tests passing

✓ Integration Tests passing

✓ Logging implemented with CorrelationId

✓ Authorization implemented

✓ Archive implemented according to ADR-011

✓ SoftDelete reserved as technical field only and unused by business workflows

✓ PostgreSQL xmin optimistic concurrency implemented

✓ Single Primary Contact rule enforced

✓ Domain Events published after successful Client Aggregate commit

✓ Audit Events written to centralized AuditEvents after successful commit

✓ GetContactHistoryQuery reads AuditEvents only

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

Contact Child Entity

ContactStatus Enum

PreferredCommunication Enum

Value Objects

Contact Domain Service

Domain Events

---

## Application Layer

Commands

Queries

Validators

DTOs

Contact Application Service

Mappings

Audit Event emission after commit

Controllers invoke Application Services directly

No MediatR

No CQRS framework

---

## Infrastructure Layer

Client Aggregate persistence including Contact child mapping

Entity Framework Configuration

Enumeration Conversions

Migration

Dependency Injection

Persistence

Centralized AuditEvents persistence integration

---

## API Layer

ContactController

Swagger

OpenAPI

Authentication

Authorization

ProblemDetails

---

## Tests

Unit Tests

Integration Tests

Client Aggregate Persistence Tests

API Tests

Business Rule Tests

Audit History Tests

Primary Contact Tests

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

EFS-002 Client Management Aggregate ownership

No implementation may violate any approved document.

---

# 45. Traceability Matrix

| Engineering Concern | Business Rule | Functional Requirement | Acceptance Criteria |
| --- | --- | --- | --- |
| Create Contact | BR-COM-004, BR-COM-005 | FR-COM-004 | AC-COM-004 |
| Primary Contact | BR-COM-005 | FR-COM-004 | AC-COM-004 |
| Contact History | BR-COM-010 | FR-COM-007, FR-SYS-005 | AC-COM-004 |
| Archive Contact | BR-COM-013 | FR-COM-010, FR-COM-011 | AC-COM-004 |
| Aggregate Ownership | BR-COM-012 | FR-COM-004 | AC-COM-004 |
| Enumerations | BR-COM-014 | FR-SYS-008 | — |

---

# 46. Architecture Compliance

## ADR-009

Contact changes produce Domain Events and Audit Events after successful Client Aggregate commit.

GetContactHistoryQuery reads centralized AuditEvents only.

Event Sourcing is not used.

## ADR-010

Contact is a child entity of Client.

Contact is not an Aggregate Root.

Contact persistence uses IClientRepository only.

Transactional boundaries remain inside the Client Aggregate.

## ADR-011

Archive is the business inactivation model.

SoftDelete is technical only.

## ADR-012

Contact enumerations are C# enums persisted as smallint.

No lookup tables.

No enumeration seed data.

API and Swagger use enum names.

---

# 47. Final Engineering Statement

This Engineering Functional Specification defines the complete implementation of the Contact Management capability for AgencyOS MVP 1.0.

This specification is derived from the reconciled Functional Documentation and approved Architecture Decision Records.

Implementation shall strictly follow this specification.

No assumptions are permitted.

Any ambiguity shall stop implementation and generate a blocking report.

This specification is the engineering input required for implementing Contact Management.

# End of Engineering Specification

EFS-003 – Contact Management

Status

READY FOR IMPLEMENTATION
