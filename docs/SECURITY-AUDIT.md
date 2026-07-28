# Security Audit & Credentials Governance Plan

## Overview
This document outlines the security posture, vulnerability findings, and credential rotation protocol for the SK Fabricator & Erector platform.

---

## 🚨 Critical Security Directive: Compromised Credentials Protocol

Any secret key, API password, JWT secret, or SMTP password previously displayed or committed in configuration files must be treated as **COMPROMISED**.

### Required Action Items:
1. **Cloudinary**:
   - Rotate Cloudinary API Secret via Cloudinary Dashboard immediately.
   - Inject `CLOUDINARY_API_SECRET` exclusively via environment variables or GitHub Secrets.

2. **Gmail SMTP**:
   - Revoke existing Google App Password and generate a new dedicated App Password.
   - Inject `SMTP_PASSWORD` via environment secrets.

3. **JWT Signing Key**:
   - Generate a brand-new cryptographically secure 256-bit (minimum 32 character) secret key.
   - Use distinct keys for QA (`JWT_SECRET_QA`) and Production (`JWT_SECRET_PROD`).

4. **Database Passwords**:
   - Change Admin and Manager database user credentials on Neon PostgreSQL.
   - Separate database roles/connection strings for QA and Production environments.

5. **Secrets Governance**:
   - Purge all secrets from source control (`appsettings.json` must contain only placeholders like `REPLACE_WITH_...`).
   - Use `.gitignore` and secret detection tools (`gitleaks`, GitHub Secret Scanning) in CI workflows.

---

## Vulnerability & Exposure Risk Matrix

| Risk Area | Threat Vector | Mitigation Strategy | Status |
|---|---|---|---|
| Hardcoded Secrets | Leaked API keys in repo | Purge git history, use environment variables & secret scanners | IN PROGRESS |
| JWT Tokens | Weak signing key or long TTL | 32+ byte random secret, 15-30 min access token TTL + refresh token rotation | IMPLEMENTED |
| Database Exposure | Direct database access over internet | Block port 5432 on Oracle VM; host database on Neon with TLS | PLANNED |
| Container Security | Running container as root user | Multi-stage Docker build with dedicated `appuser` (non-root) | PLANNED |
| TLS / SSL | Unencrypted HTTP traffic | Nginx reverse proxy enforcing HTTPS & HSTS headers | PLANNED |
| CORS | Wildcard origin reflection | Strict allowed origins per environment (`qa.yourdomain.com`, `www.yourdomain.com`) | PLANNED |

---

## Automated Security Pipeline (CI Integration)
- **Gitleaks**: Scans commit history and PRs for exposed secrets.
- **Trivy**: Scans Docker images for OS and application package vulnerabilities.
- **CodeQL**: Static Application Security Testing (SAST) for C# and TypeScript codebases.
- **Dependabot**: Automatic security patches for NuGet and npm packages.
