---
description: Project-wide instructions for the CongThongTin frontend
applyTo: "**"
---

# Project Instructions

## Scope and Technology

- This repository contains only the frontend for the internal news portal of the 370 Air Division.
- Backend source code is not in this workspace. Do not search this workspace, sibling folders, or other repositories for backend projects, controllers, models, or API source files.
- All backend documentation the frontend needs is kept continuously up to date in `docs/backend-for-frontend.md`. That file is the only backend reference and is sufficient on its own.
- When you need anything about the backend (endpoints, request and response shapes, roles, authentication, or business rules), read `docs/backend-for-frontend.md` only. Do not search for a backend file.
- Use ASP.NET Core 10, C#, and ASP.NET Core MVC.
- Render pages as Razor Views (`.cshtml`) under `Views/`, returned by controller actions. Routing stays `{controller}/{action}/{id?}`.
- This is MVC, not Razor Pages. Do not add `@page`, a `Pages/` folder, `PageModel`, `AddRazorPages`, or `MapRazorPages`.
- Keep the MVP simple, maintainable, responsive, and easy to extend.
- Do not add backend services, databases, or infrastructure.
- Do not add a frontend framework or dependency unless explicitly requested and clearly necessary.

## Implementation

- Read `docs/frontend-style.md` before making UI changes.
- Add UI through a controller action and a matching view. Inspect and reuse existing layouts, views, components, and CSS.
- Follow existing naming and project conventions.
- Prefer small, focused changes; do not modify unrelated files.
- Keep JavaScript minimal and avoid unnecessary abstractions.
- Do not invent business rules. State any UI-affecting assumption before implementing it.
- Never expose secrets, passwords, tokens, or internal exception details.

## Visual Design

- Use a traditional, professional news-portal design with dark navy and Air Force sky blue.
- Prioritize Vietnamese readability, restrained decoration, subtle borders, and minimal animation.
- Ensure desktop, tablet, and mobile layouts work without fixed-width breakage.
- Keep shared elements such as navigation, buttons, forms, cards, tables, alerts, and pagination visually consistent.

## Verification

- By default, verify changes with relevant static checks, compilation, and automated tests only.
- Do not launch or control a browser, use browser automation, open an IDE browser, take screenshots, or perform manual UI testing unless the user explicitly requests browser/UI verification.
- Do not start the application solely for visual inspection unless explicitly requested.
- If visual verification would be useful, mention it as an optional next step instead of running it automatically.