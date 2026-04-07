# Screen Specification: Login

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-LOGIN-001 |
| **Priority** | High |
| **User Type** | All users |
| **Purpose** | User authentication and system access control |

---

## Layout Specification

### Container
- **Width:** 400px
- **Position:** Centered horizontally and vertically
- **Background:** Card style (`--bg-card` #0F3460)
- **Radius:** 12px
- **Padding:** 48px

### Structure

```
+----------------------------------+
|           HnVue Logo             |
|      Medical Imaging Console      |
|                                  |
|  [User ID Input           ]      |
|  [Password Input          ]      |
|                                  |
|        [  Login Button  ]        |
|                                  |
|  Attempt 1 of 5                  |
|                                  |
|              [Language v]        |
+----------------------------------+
```

---

## Components

### Logo Area
- **Logo:** HnVue text logo
- **Subtitle:** "Medical Imaging Console"
- **Alignment:** Center
- **Margin Bottom:** 32px

### Input Fields

| Field | Label | Type | Required | Validation |
|-------|-------|------|----------|------------|
| User ID | "User ID" | **dropdown (ComboBox)** | Yes | Select from registered users |
| Password | "Password" | password | Yes | Non-empty |

**Input Styling:**
- Height: 36px
- Padding: 8px 12px
- Border: 1px solid `--border-default` (#2E4A6E)
- Focus: `--border-focus` (#00AEEF) outline

> **PPT 슬라이드 1 변경사항**: User ID 입력 방식을 텍스트 입력에서 **드롭다운 목록 선택**으로 변경.  
> 등록된 사용자 ID 목록에서 선택. `IsEditable="True"` 로 직접 입력도 가능.  
> ViewModel: `ILoginViewModel.AvailableUserIds` (IReadOnlyList<string>)

### Login Button
- **Text:** "Login"
- **Style:** Primary button
- **Background:** #1B4F8A (CoreTokens `--primary-main`)
- **Size:** Large (44px height)
- **Width:** Full width
- **Margin Top:** 24px

### Login Attempt Counter (FR-CS-002)
- **Position:** Below login button, left-aligned
- **Display:** "Attempt {n} of 5"
- **Visibility:** Shown after first failed attempt
- **Color:** #FFD600 (warning) after attempt 3; #D50000 (error) at attempt 5
- **ARIA:** `aria-live="polite"` for screen reader announcement
- **Example:** "Attempt 3 of 5"

### Account Lockout Countdown Timer (FR-CS-003)
- **Trigger:** After 5 consecutive failed login attempts
- **Display:** "Account locked. Unlocks in: 28:45"
- **Format:** MM:SS countdown
- **Position:** Replaces login button area during lockout
- **Color:** #D50000 background banner, white text
- **Behavior:** Button disabled; countdown updates every second
- **ARIA:** `aria-live="assertive"` role="alert"
- **On Expiry:** Timer disappears, login re-enabled, attempt counter resets

### Session Timeout Return Notice
- **Trigger:** User returned to login after session expiry (FR-CS-010)
- **Display:** "Your session expired. Please login again."
- **Position:** Below subtitle, above input fields
- **Style:** Info banner, background #00AEEF at 15% opacity, border-left 4px solid #00AEEF
- **Dismissible:** Yes (x button)
- **ARIA:** `role="status"` aria-live="polite"

### Language Selector
- **Position:** Bottom right
- **Style:** Compact select dropdown
- **Options:** English, Korean, Japanese
- **Default:** Korean

---

## Password Management

### Password Strength Indicator (Change-Password Flow)
- **Location:** Below password input in change-password modal
- **States:**

| Strength | Color | Label | Criteria |
|----------|-------|-------|----------|
| Weak | #D50000 | "Weak" | < 8 chars or only one char type |
| Fair | #FFD600 | "Fair" | 8+ chars, 2 char types |
| Good | #00AEEF | "Good" | 8+ chars, 3 char types |
| Strong | #00C853 | "Strong" | 12+ chars, 4 char types |

- **Visual:** Segmented progress bar (4 segments), each segment fills with strength color
- **Criteria display:** Checklist shown below bar:
  - [x] At least 8 characters
  - [x] Uppercase letter
  - [x] Lowercase letter
  - [x] Number
  - [ ] Special character (!@#$...)
- **ARIA:** `aria-label="Password strength: {Strong|Good|Fair|Weak}"`

---

## Interaction Flow

```
User enters credentials
         |
         v
   Click Login / Press Enter
         |
         v
   Validate credentials
         |
    +----+----+
    |         |
Valid    Invalid
    |         |
    v         v
Success    Increment attempt counter
    |         |
    v         v
Navigate  Attempt >= 5?
to Worklist   |
         +----+----+
         |         |
        No        Yes
         |         |
         v         v
   Show error  Lock account
   "Attempt n  Show countdown
   of 5"       timer
```

---

## Error Handling

| Error | Display | Action |
|-------|---------|--------|
| Empty ID | Inline under input | "Enter your user ID" |
| Empty Password | Inline under input | "Enter your password" |
| Invalid Credentials | Inline banner | "Invalid user ID or password. Attempt {n} of 5." |
| Account Locked | Lockout banner with countdown | "Account locked. Unlocks in: MM:SS" |
| Server Error | Toast notification | "Login service unavailable. Try again later." |
| Session Expired | Info banner above inputs | "Your session expired. Please login again." |

---

## Accessibility

### Tab Order
1. User ID input
2. Password input
3. Login button
4. Language selector

### Keyboard Shortcuts
- **Enter:** Submit form when any input has focus
- **Tab:** Navigate between fields
- **Shift+Tab:** Navigate backwards

### ARIA Labels
- `aria-label="User ID input field"`
- `aria-label="Password input field"`
- `aria-label="Login button"`
- `aria-label="Login attempts: {n} of 5"` on attempt counter
- `role="alert"` on lockout banner

### Screen Reader Support
- Form has proper `<label>` associations
- Error messages are announced via `aria-live`
- Attempt counter announced on change
- Lockout timer announced on activation

---

## State Specifications

### Initial State
- All fields empty
- Login button enabled
- No error messages
- No attempt counter visible

### Loading State
- Login button disabled
- Button shows loading spinner
- Text changes to "Logging in..."

### Error State (Attempts 1-4)
- Error message visible with attempt count
- Attempt counter shown below button
- Focus on User ID field
- Login button re-enabled

### Warning State (Attempt 3)
- Attempt counter color: #FFD600
- Warning tone in error message

### Pre-Lockout State (Attempt 5)
- Attempt counter color: #D50000
- One final error shown

### Locked State
- Login button replaced by lockout banner
- Countdown timer running MM:SS
- Attempt counter hidden
- User ID and password fields disabled (gray)

### Session-Expired Return State
- Session expired notice shown (info banner)
- All fields empty, button enabled
- No attempt counter shown (fresh start)

### Success State
- Success toast shown briefly
- Navigate to Worklist screen

---

## Edge Cases

1. **Caps Lock On:** Show indicator in password field
2. **Multiple Failed Attempts:** Attempt counter shown after first failure; escalating color at attempt 3+
3. **Password Expiry:** Force password change on next login (triggers change-password modal with strength indicator)
4. **Session Timeout:** Return to login with session-expired notice banner
5. **Lockout during active countdown:** Refresh page preserves lockout state via server-side check

---

## Validation Rules

| Rule | Condition |
|------|-----------|
| ID Required | User ID must not be empty |
| Password Required | Password must not be empty |
| ID Format | 3-20 alphanumeric characters |
| Password Format | 8+ characters, if enforced |
| Account Active | User account must not be disabled |
| Attempt Limit | Max 5 attempts before lockout (FR-CS-002) |
| Lockout Duration | Configurable; default 30 minutes (FR-CS-003) |

---

## Color Reference (CoreTokens)

| Token | Value | Usage |
|-------|-------|-------|
| `--primary-main` | #1B4F8A | Login button background |
| `--primary-light` | #00AEEF | Focus ring, info banner accent |
| `--border-default` | #2E4A6E | Input borders |
| `--border-focus` | #00AEEF | Focused input outline |
| `--error` | #D50000 | Lockout banner, attempt 5 counter |
| `--warning` | #FFD600 | Attempt 3+ counter |
| `--success` | #00C853 | Strong password indicator |
| `--bg-surface (BackgroundPanel)` | #2A2A2A (변경: #16213E→#2A2A2A) | Page background |
| `--bg-card (BackgroundCard)` | #3B3B3B (변경: #0F3460→#3B3B3B) | Login card background |
| BackgroundPage | #242424 | 주 배경 (변경: #1A1A2E→#242424) |

---

## Related Documents

- [Component Library](../component_library.md)
- [Design System](../design_system.pen)
- [Accessibility Guidelines](../component_library.md#accessibility-guidelines)

---

**Version:** 1.2
**Last Updated:** 2026-04-07
**Status:** Active
**Changes v1.2:** PPT 슬라이드 1 — User ID ComboBox 드롭다운 변경, 색상 토큰 업데이트 (#242424, #3B3B3B, #2A2A2A)
