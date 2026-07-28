# Technical Debt Register & Remediation Strategy

## Overview
This document tracks technical debt items identified during the architecture audit, along with priority levels and remediation timelines.

---

## Technical Debt Inventory

| ID | Category | Technical Debt Description | Severity | Impact | Remediation Plan |
|---|---|---|---|---|---|
| TD-01 | Security | Hardcoded credentials in legacy config files | CRITICAL | Security Compromise | Purge secrets from repo, enforce env secrets via GitHub Environments |
| TD-02 | DevOps | Manual VM configuration & lack of IaC | HIGH | Inability to quickly recover from VM loss | Implement Terraform + Ansible automated playbooks |
| TD-03 | Database | Absence of dedicated QA vs Production database isolation | HIGH | Data corruption risk during QA tests | Separate Neon PostgreSQL database instances for QA and Production |
| TD-04 | File Storage | Risk of saving uploaded files to local disk on VM | HIGH | VM disk bloat & lack of ephemeral container persistence | Enforce `CloudinaryFileStorageService` for all file operations |
| TD-05 | CI/CD | Missing security scanning (SAST/Trivy/Gitleaks) in CI | MEDIUM | Undetected vulnerabilities in production images | Integrate CodeQL, Gitleaks, Trivy into GitHub Actions |
| TD-06 | Monitoring | Lack of standardized health endpoints (`/health/live`, `/health/ready`) | MEDIUM | Delayed fault detection | Add ASP.NET Core Health Checks middleware |

---

## Technical Debt Remediation Schedule
- **Phase 1-3**: Resolve TD-01, TD-04, TD-06 (Immediate Codebase & Security Fixes).
- **Phase 4-7**: Resolve TD-03 (Environment & Database Isolation, Docker Containerization).
- **Phase 8-15**: Resolve TD-02, TD-05 (IaC, Ansible, CI/CD Pipeline Automation & Security Scanning).
