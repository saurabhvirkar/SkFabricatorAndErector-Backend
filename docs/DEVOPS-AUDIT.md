# DevOps, IaC & Server Hardening Audit

## Executive Summary
This document defines the DevOps pipeline, Infrastructure as Code (Terraform), Server Configuration Automation (Ansible), and Oracle Cloud Always Free VM hardening.

---

## Infrastructure Blueprint ($0-Cost Stack)

```
                            Oracle Cloud Infrastructure (OCI)
                             Always Free Ampere/AMD Compute
                                           │
                                           v
                             Ubuntu Linux Host OS
                                           │
                   +-----------------------+-----------------------+
                   |                                               |
                   v                                               v
           Docker Engine                                  Nginx Reverse Proxy
                   │                                     (Port 80 / 443 TLS)
                   v                                               │
     ASP.NET Core Web API Container                                │
       (Non-root `appuser`) <──────────────────────────────────────┘
```

---

## IaC & Automation Components

1. **Terraform (`deploy/terraform/oracle`)**:
   - Manages OCI compute instance provisioning, Virtual Cloud Network (VCN), Subnets, and Ingress Security Lists.
   - Configures ingress rules for Port 80 (HTTP) and Port 443 (HTTPS). Strictly blocks exposed database ports (5432).

2. **Ansible (`deploy/ansible`)**:
   - Provisioning playbook for new Oracle VM instances.
   - Installs Docker, Docker Compose, Nginx, UFW firewall, Fail2ban.
   - Disables SSH password authentication, creates dedicated deployment user (`deployer`).

3. **Docker Multi-Stage Build (`Dockerfile`)**:
   - Stage 1: SDK build & publish (.NET 10).
   - Stage 2: Minimal ASP.NET 10 runtime image (`mcr.microsoft.com/dotnet/aspnet:10.0`).
   - Runs under non-root system user.

4. **CI/CD Pipelines (GitHub Actions)**:
   - **CI Workflow (`.github/workflows/ci.yml`)**: CodeQL, Gitleaks secret scan, unit/integration tests, Trivy container vulnerability scan.
   - **Deploy QA Workflow (`.github/workflows/deploy-qa.yml`)**: Automated deployment to QA container on Oracle VM upon merge to `develop`.
   - **Deploy Production Workflow (`.github/workflows/deploy-production.yml`)**: Manual approval gate, DB backup verification, zero-downtime container update on `main`.
