# Security Audit Report — Phase 1

**Date:** July 26, 2026  
**Target:** `SkFabricatorAndErector` ASP.NET Core Web API & Angular Frontend

---

## 1. Controller Endpoint Authorization Audit Matrix

Every API controller was audited to verify authentication and role-based authorization attributes:

| Controller | Route | HTTP Method | Auth Attribute | Allowed Roles | Access Scope |
|---|---|---|---|---|---|
| `AccountController` | `api/account/login` | `POST` | `[AllowAnonymous]` | Anyone | Public Auth |
| `AccountController` | `api/account/refresh-token` | `POST` | `[AllowAnonymous]` | Anyone | Public Auth |
| `ClientDetailsController` | `api/clientdetails` | `GET` | None | Anyone | Public Catalog |
| `ClientDetailsController` | `api/clientdetails/{id}` | `GET` | None | Anyone | Public Catalog |
| `ClientDetailsController` | `api/clientdetails` | `POST` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `ClientDetailsController` | `api/clientdetails/{id}` | `PUT` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `ClientDetailsController` | `api/clientdetails/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `HomeSliderController` | `api/homeslider` | `GET` | None | Anyone | Public Catalog |
| `HomeSliderController` | `api/homeslider` | `POST` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `HomeSliderController` | `api/homeslider/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `InquiryController` | `api/inquiry` | `POST` | `[AllowAnonymous]` | Anyone | Public Form |
| `InquiryController` | `api/inquiry` | `GET` | `[Authorize]` | `Admin,Manager` | Admin Inquiry Read |
| `InquiryController` | `api/inquiry/{id}` | `GET` | `[Authorize]` | `Admin,Manager` | Admin Inquiry Read |
| `InquiryController` | `api/inquiry/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Inquiry Delete |
| `OurServicesController` | `api/ourservices` | `GET` | None | Anyone | Public Catalog |
| `OurServicesController` | `api/ourservices/{id}` | `GET` | None | Anyone | Public Catalog |
| `OurServicesController` | `api/ourservices` | `POST` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `OurServicesController` | `api/ourservices/{id}` | `PUT` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `OurServicesController` | `api/ourservices/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `PhotoController` | `api/photos` | `GET` | None | Anyone | Public Media |
| `PhotoController` | `api/photos/about-slider` | `GET` | None | Anyone | Public Media |
| `PhotoController` | `api/photos` | `POST` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `PhotoController` | `api/photos/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `PhotoController` | `api/photos/about-slider/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `ProjectController` | `api/project` | `GET` | None | Anyone | Public Catalog |
| `ProjectController` | `api/project/{id}` | `GET` | None | Anyone | Public Catalog |
| `ProjectController` | `api/project/category/{category}` | `GET` | None | Anyone | Public Catalog |
| `ProjectController` | `api/project` | `POST` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `ProjectController` | `api/project/{id}` | `PUT` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `ProjectController` | `api/project/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `TeamMembersController` | `api/teammembers` | `GET` | None | Anyone | Public Catalog |
| `TeamMembersController` | `api/teammembers/{id}` | `GET` | None | Anyone | Public Catalog |
| `TeamMembersController` | `api/teammembers` | `POST` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `TeamMembersController` | `api/teammembers/{id}` | `PUT` | `[Authorize]` | `Admin,Manager` | Admin Write |
| `TeamMembersController` | `api/teammembers/{id}` | `DELETE` | `[Authorize]` | `Admin,Manager` | Admin Write |

---

## 2. IDOR & Access Control Audit Findings

- **Endpoint Protection Coverage**: 100% of mutation operations (`POST`, `PUT`, `DELETE`) and sensitive data reads (Inquiries list/by-id) carry `[Authorize(Roles = "Admin,Manager")]`.
- **IDOR Analysis**: Resources managed by the application (Projects, Services, Team Members, Gallery Photos, Clients) are corporate-wide catalog items rather than user-owned entities. Access control relies on role-level gatekeeping rather than tenant/owner checks.
- **Identified Gap (To be remediated in Phase 3)**: Role checks rely on `UserManager.GetRolesAsync()` in token generation, but `ApplicationUser.Role` exists as a duplicate string property on the entity which could drift or be confused with ASP.NET Identity roles.
