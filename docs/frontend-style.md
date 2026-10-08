# Frontend Style Guide

## 1. Design Direction

The website is an internal news portal for the 370 Air Division.

The visual identity should be inspired by the reference image supplied for this project:

- Air Force / aviation feeling
- Blue sky and blue-gray tones
- Professional and institutional
- Clean newspaper/news-portal layout
- Strong readability
- Minimal decoration
- No flashy animations

The primary visual combination is:

**Black / very dark navy + Air Force sky blue**

The design should feel modern but restrained rather than looking like a corporate landing page.

---

## 2. Color Palette

Use CSS variables so the palette can be changed centrally.

```css
:root {
    /* Primary */
    --color-primary: #3F80A7;
    --color-primary-dark: #2F6688;
    --color-primary-light: #6FA9C9;

    /* Main dark color */
    --color-dark: #111827;
    --color-dark-soft: #1F2937;
    --color-black: #0B1220;

    /* Light surfaces */
    --color-background: #F5F8FA;
    --color-surface: #FFFFFF;
    --color-surface-blue: #EAF4FA;

    /* Text */
    --color-text: #17202A;
    --color-text-muted: #667085;
    --color-text-on-primary: #FFFFFF;

    /* Borders */
    --color-border: #D9E2E8;

    /* Status */
    --color-success: #2E7D5B;
    --color-warning: #C58A24;
    --color-danger: #B42318;
}
```

### Color usage

`--color-primary: #3F80A7`

Use for:

- Main brand color
- Primary buttons
- Links where appropriate
- Active navigation
- Section headings
- Important UI accents

`--color-dark: #111827`

Use for:

- Header
- Main navigation
- Footer
- Important text
- Admin sidebar

`--color-background: #F5F8FA`

Use as the general page background.

`--color-surface: #FFFFFF`

Use for:

- News cards
- Content areas
- Tables
- Forms
- Panels

Avoid using pure black everywhere.

Avoid excessive use of blue. Blue should identify the brand and important actions, not cover the entire page.

---

## 3. Overall Layout

Prefer a traditional news website structure.

```text
┌──────────────────────────────────────────────┐
│ HEADER / BRAND                               │
├──────────────────────────────────────────────┤
│ NAVIGATION                                   │
├──────────────────────────────────────────────┤
│                                              │
│ MAIN CONTENT                                 │
│                                              │
│  Featured News                               │
│                                              │
│  Latest News                                 │
│                                              │
│  Categories / Documents                      │
│                                              │
├──────────────────────────────────────────────┤
│ FOOTER                                       │
└──────────────────────────────────────────────┘
```

Use a centered content container with a reasonable maximum width.

Recommended:

```css
max-width: 1200px;
```

Do not make the content excessively wide.

---

## 4. Header

The header should use the dark color.

Recommended structure:

- Organization/logo area
- Site name
- Navigation
- User/account area

Example visual direction:

```text
[LOGO]  CỔNG THÔNG TIN SƯ ĐOÀN 370
        SƯ ĐOÀN KHÔNG QUÂN 370

Trang chủ | Tin tức | Danh mục | Tài liệu       [Tài khoản]
```

The header should be compact.

Do not create a huge hero header.

---

## 5. Navigation

Navigation should be simple.

Use:

- Dark background
- White text
- Sky-blue hover/active state

Avoid excessive dropdown menus.

For mobile, use a simple responsive navigation pattern.

---

## 6. News Cards

News cards should resemble normal newspaper/news websites.

Recommended structure:

```text
┌─────────────────────────────┐
│                             │
│          IMAGE              │
│                             │
├─────────────────────────────┤
│ CATEGORY                    │
│ News title                  │
│ Short summary...            │
│ 08/10/2026                  │
└─────────────────────────────┘
```

Use subtle borders and spacing.

Avoid:

- Excessive shadows
- Large rounded corners
- Glassmorphism
- Neon effects
- Excessive gradients

A small border radius such as `4px` to `8px` is sufficient.

---

## 7. Typography

Prioritize Vietnamese readability.

Recommended font stack:

```css
font-family:
    Inter,
    "Segoe UI",
    Roboto,
    Arial,
    sans-serif;
```

Headlines should be clearly larger and heavier than body text.

Body text should be comfortable to read.

Do not use decorative fonts for the main UI.

---

## 8. Buttons

Buttons should be simple.

Primary button:

- Air Force blue background
- White text

Secondary button:

- White/light background
- Dark text
- Border

Danger button:

- Red only for destructive actions

Avoid excessive button styles.

---

## 9. Forms

Forms should be clean and administrative.

Use:

- Clear labels
- Full-width inputs where appropriate
- Visible validation messages
- Consistent spacing
- Simple borders

Do not use highly animated inputs.

---

## 10. Admin Dashboard

The Admin dashboard should be functional rather than visually flashy.

Recommended structure:

```text
Dashboard

┌───────────┐ ┌───────────┐ ┌───────────┐
│ Users     │ │ News      │ │ Documents │
│    120    │ │     86    │ │     42    │
└───────────┘ └───────────┘ └───────────┘

Recent activity
────────────────────────────────────

Recent news
────────────────────────────────────
```

Use cards with subtle borders.

Do not use large charts unless they provide meaningful information.

---

## 11. Admin Layout

Admin pages may use a sidebar.

```text
┌────────────┬──────────────────────────────┐
│            │                              │
│ Dashboard  │       PAGE CONTENT           │
│ News       │                              │
│ Users      │                              │
│ Documents  │                              │
│ Categories │                              │
│            │                              │
└────────────┴──────────────────────────────┘
```

Sidebar:

- Dark navy/black
- White text
- Sky-blue active state

---

## 12. Responsive Design

The website must work on:

- Desktop
- Laptop
- Tablet
- Mobile

Desktop can use multi-column news layouts.

Mobile should collapse to a single-column layout.

Avoid fixed widths that break on small screens.

---

## 13. Images

News images should use consistent aspect ratios.

Prefer:

```css
aspect-ratio: 16 / 9;
object-fit: cover;
```

Do not stretch images.

Use lazy loading for non-critical images where appropriate.

---

## 14. Animation

Keep animation minimal.

Allowed:

- Small hover transitions
- Button hover
- Navigation hover
- Subtle card transitions

Avoid:

- Page transition animations
- Large entrance animations
- Parallax
- Auto-moving content
- Excessive transforms

The website should feel like a professional internal news portal, not a marketing landing page.

---

## 15. General Rule for AI

When creating a new UI component, first check whether an existing component can be reused.

Do not create a new visual style for every page.

The following should remain visually consistent throughout the system:

- Header
- Navigation
- Buttons
- Forms
- Cards
- Tables
- Pagination
- Alerts
- Badges
- Admin sidebar

When uncertain about visual decisions, prefer:

**simple + readable + professional**

over:

**complex + decorative + animated**
