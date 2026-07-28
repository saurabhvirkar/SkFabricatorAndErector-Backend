# Frontend Architecture Audit & Cloudflare Pages Strategy

## Executive Summary
This document reviews the Angular frontend codebase (`SkFabricatorAndErector-Frontend`), its environment configuration, build optimization, and deployment strategy on **Cloudflare Pages**.

---

## Technical Baseline

- **Framework**: Angular 21 (`@angular/core` ^21.2.0)
- **UI Libraries**: Angular Material 21, Bootstrap 5.3, TailwindCSS 4
- **State Management / Communication**: RxJS 7.8, HttpClient
- **Hosting Target**: Cloudflare Pages ($0 Unlimited Bandwidth Free Tier)

---

## Environment & Build Architecture

```
                                Cloudflare Pages
                                       │
                +----------------------+----------------------+
                |                                             |
                v                                             v
     QA Deployment Branch                          Production Branch
       (`develop`)                                     (`main`)
            │                                             │
            v                                             v
  `environment.qa.ts`                            `environment.prod.ts`
  apiUrl: https://api-qa.yourdomain.com          apiUrl: https://api.yourdomain.com
```

### Key Refactoring Recommendations

1. **Environment File Configuration**:
   - `src/environments/environment.ts` (Development -> Local proxy)
   - `src/environments/environment.qa.ts` (QA -> `https://api-qa.yourdomain.com/api`)
   - `src/environments/environment.prod.ts` (Production -> `https://api.yourdomain.com/api`)

2. **Cloudflare Pages Integration**:
   - Production Build Command: `ng build --configuration production`
   - Output Directory: `dist/sk-fabricator-ui/browser`
   - Header & Redirect configuration (`_headers`, `_redirects`) for single-page application (SPA) routing fallback (`/* /index.html 200`).

3. **SEO & Performance Optimization**:
   - HTML Title & Meta tags configured per route.
   - Image optimization using Cloudinary dynamic URL parameters (webP/avif format, responsive sizing).
