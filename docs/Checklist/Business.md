# Checklist — Business Overview

## Vision

Checklist is a generic platform for companies to define, execute, and audit recurring operational processes. Restaurants are the initial market, but the product does not depend on that industry.

## Company Structure

An organization has hierarchical sectors, supporting anything from simple structures to trees with multiple levels. Each sector may have shifts and its own managers and operators.

The organization may define a default location. A sector may use that location, inherit the location of an ancestor sector, or define its own.

## Users and Access

- **Owner:** has full control of the organization, including administrator management.
- **Admin:** manages users, sectors, shifts, and organizational processes.
- **Manager:** manages the sectors assigned to them and monitors their executions.
- **Operator:** executes checklists assigned to their sectors and shifts.

## Processes

A checklist represents a process and contains ordered tasks. Each task defines instructions, schedule, required status, input types, and an optional location requirement.

Initial input types include boolean, text, number, selection, photo, video, date, time, and signature.

Published checklists are immutable. Any change creates a new version, preserving the meaning of historical records.

## Execution and Monitoring

A schedule determines when, in which sector, and optionally in which shift or by which operator a checklist must be executed.

`ChecklistRecord` stores an expected or completed execution. `TaskRecord` stores its task responses. These records support monitoring delays, missed executions, compliance, location, review, and evidence.

Reports are produced from existing records and are not separate entities initially.

## Principles

- Complete isolation of each organization's data.
- Permissions enforced according to role, sector, and shift.
- Immutable history of published and executed processes.
- Audit trail for relevant actions.
- Collection of only the data that is necessary.
- Files stored outside the relational database.
- Technical logs and observability separated from business data.
