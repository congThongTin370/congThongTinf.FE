# CongThongTin Frontend Requirements

## 1. Product Scope

- Provide an internal news and information portal for the 370 Air Division.
- Keep this repository frontend-only.
- Deliver an MVP that is simple, maintainable, responsive, and easy to extend.
- Backend APIs, persistence, authentication infrastructure, deployment, and other infrastructure are out of scope.

## 2. Technology

- ASP.NET Core 10
- C#
- ASP.NET Core MVC with controller actions and server-rendered Razor Views under `Views/`
- Conventional routing: `{controller=Home}/{action=Index}/{id?}`
- Minimal client-side JavaScript

Razor Pages are out of scope. Do not introduce `@page`, a `Pages/` folder, `PageModel`, `AddRazorPages`, or `MapRazorPages`.

React, Vue, Angular, and other frontend frameworks must not be introduced unless explicitly requested.

## 3. User Interface

- Follow `docs/frontend-style.md` and `docs/backend-for-frontend`.
- Use a traditional news-portal structure with a compact header, navigation, main content, and footer.
- Use the shared dark navy and Air Force sky-blue palette through CSS variables.
- Reuse shared layouts, components, and styles instead of introducing page-specific visual systems.
- Favor readable typography, restrained decoration, subtle borders, small corner radii, and minimal animation.
- Avoid oversized hero areas, excessive shadows, glassmorphism, neon effects, parallax, and decorative motion.

## 4. Responsive and Accessible Behavior

- Support desktop, laptop, tablet, and mobile screens.
- Collapse multi-column content into a usable single-column layout on small screens.
- Avoid fixed widths that cause horizontal overflow.
- Preserve clear labels, keyboard-usable controls, visible focus states, validation messages, and sufficient color contrast.
- Display news images consistently without distortion, preferably using a 16:9 aspect ratio and `object-fit: cover`.

## 5. Engineering Constraints

- Follow existing project naming and structure.
- Prefer focused changes and simple MVP-level solutions.
- Do not modify unrelated files.
- Do not add dependencies without a clear requirement.
- Do not duplicate an existing component or style when it can be reused.
- Do not invent business rules when requirements are unclear.
- Do not expose secrets, credentials, tokens, or internal exception details.

## 6. Acceptance and Verification

A change is acceptable when:

- It builds successfully and relevant automated checks pass.
- It follows the existing layout and `docs/frontend-style.md`.
- It remains responsive and does not introduce obvious accessibility regressions.
- It does not add out-of-scope backend, database, or infrastructure work.
- It does not introduce unrelated changes or unnecessary dependencies.

Browser-based verification, IDE browser control, screenshots, and manual UI testing are opt-in. They must only be performed when the user explicitly requests them.
