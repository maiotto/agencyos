# US-103 — Configure Standard Working Hours

Release: 1.1

Status: Implemented

---

## Objective

Configure standard operational Working Hours associated with a Working Calendar for Planning.

---

## API

Base route: `/working-hours`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/working-hours` | List / filter |
| GET | `/working-hours/{id}` | Get by id |
| POST | `/working-hours` | Create (Inactive) |
| PUT | `/working-hours/{id}` | Update |
| POST | `/working-hours/{id}/activate` | Activate |
| POST | `/working-hours/{id}/deactivate` | Deactivate |
| DELETE | `/working-hours/{id}` | Delete inactive |

Integration:

| Method | Path |
|--------|------|
| GET | `/working-calendars/operational-day` (includes schedule + plannedNetHours) |
| GET | `/working-calendars/{id}/working-hours` |

---

## Business Rules

BR-301..BR-309
