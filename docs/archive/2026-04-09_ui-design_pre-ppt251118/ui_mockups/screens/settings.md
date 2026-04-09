# Screen Specification: Settings

## Overview

| Property | Value |
|----------|-------|
| **Screen ID** | SCR-SETTINGS-001 |
| **Priority** | High |
| **User Type** | Administrators, Service Engineers |
| **Purpose** | System configuration and preferences |

---

## Layout Specification

### Top Tab Navigation

```
+----------------------------------------------------------+
| [⚙] Settings                                             |
+----------------------------------------------------------+
| [System][Account][Detector][Generator][Network]          |
| [Display][Option][Database][DicomSet][RIS Code]          |
+----------------------------------------------------------+
| Content area changes based on active tab                 |
|                                                          |
| [Network Tab Example]:                                   |
|   PACS: [Address___] Port:[104] [Add][Edit][Delete]      |
|   Worklist: [Address___] Port:[4006] [Add][Edit][Delete] |
|   Print: [Address___] Port:[104] [Add][Edit]             |
+----------------------------------------------------------+
| [Cancel]                                    [Save]       |
+----------------------------------------------------------+
```

### Navigation Structure
- **Top Row Tabs:** System, Account, Detector, Generator, Network
- **Second Row Tabs:** Display, Option, Database, DicomSet, RIS Code
- **Content Area:** Dynamic, changes with selected tab
- **Bottom Buttons:** [Cancel] [Save]

---

## Tab Navigation Categories

### System Tab
- Session timeout configuration
- TLS/network security status
- Password policy settings
- Audit logs viewer (Admin only — FR-SA-025)
- Software version and updates
- **Note:** "Access Notice" (formerly "Login Popup") notification settings

### Account Tab
- User role management
- Access control levels
- Permission configuration
- Role ComboBox selector (updated from text field)

### Detector Tab
- Detector calibration (link to Calibration Wizard)
- Detector status monitoring
- Firmware version info

### Generator Tab
- Generator settings
- Device status monitoring
- Power configuration

### Network Tab (Consolidated)
**Combines former separate PACS, Worklist, and Print tabs**
- **PACS Settings:** AE Title, Address, Port, TLS status
- **Worklist Settings:** Address, Port configuration
- **Print Server Settings:** Address, Port configuration
- TLS status indicator (FR-DC-030)
- Test Connection button

### Display Tab
- Language selection (with preview)
- Theme (Dark/Light mode)
- Font size
- Monitor configuration (for multi-display setups)

### Option Tab
- User preferences
- Workflow settings
- View customizations

### Database Tab
- Database path
- Image storage location
- Export path
- Auto-archive settings

### DicomSet Tab
- DICOM configuration
- Standard settings

### RIS Code Tab
**Sub-tabs: Matching and Un-Matched**
- Matching: RIS code matching configuration
- Un-Matched: Handle unmatched RIS codes (formerly "Only No matching")

---

## Section Specifications

### Network Tab — Consolidated PACS/Worklist/Print Settings

```
+------------------------------------------+
| Network Configuration                    |
|                                          |
| PACS Server:                             |
| Address:  [pacs.hospital.local_____]     |
| Port:     [104________________]          |
| [Add Server] [Edit] [Delete]             |
|                                          |
| Worklist Server:                         |
| Address:  [mwl.hospital.local_____]      |
| Port:     [4006_______________]          |
| [Add Server] [Edit] [Delete]             |
|                                          |
| Print Server:                            |
| Address:  [print.hospital.local__]       |
| Port:     [104________________]          |
| [Add Server] [Edit]                      |
|                                          |
| TLS/Network Security                     |
| DICOM TLS:  [●] Connected  (TLS 1.3)    |
|             Certificate: Valid           |
|             Expires: 2027-03-15          |
|                                          |
| [Test Connection]                        |
+------------------------------------------+
```

- **TLS Status Badge:**
  - Connected: #00C853 filled circle + "Connected (TLS 1.3)"
  - Disabled: #B0BEC5 empty circle + "Disabled"
  - Error: #D50000 filled circle + "Error: {reason}"
  - Warning: #FFD600 circle + "Certificate expiring in {n} days"
- **Test Connection Button:**
  - Action: Send DICOM C-ECHO to all configured servers
  - Shows spinner during test (max 10 seconds)
  - Result: inline green/red banner "Connection successful" / "Connection failed: {reason}"
- **Access:** Admin only

### System Tab — Session Timeout Configuration

```
+------------------------------------------+
| Session Management                       |
|                                          |
| Auto-logout after inactivity:            |
|   [30 min  v]                            |
|   Options: 5, 10, 15, 30, 60, 120 min   |
|            Never (requires admin PIN)    |
|                                          |
| Warning shown before logout:             |
|   [3 min   v]                            |
|   Options: 1, 2, 3, 5 min               |
|                                          |
| Access Notice (Security Alert)           |
| [ ] Show Access Notice on login          |
+------------------------------------------+
```

- **Auto-logout Dropdown:**
  - Default: 30 minutes
  - Options: 5, 10, 15, 30, 60, 120 minutes, Never
  - "Never" requires admin PIN confirmation
  - Stored per-user or system-wide (admin configurable)
- **Warning Period:** Time before logout when countdown modal appears
- **Access Notice:** Toggle to show/hide security notice on login screen
- **Behavior:** On timeout, session expires, user redirected to login with session-expired notice

### Detector Tab — Calibration Wizard Link (FR-SA-010–015)

```
+------------------------------------------+
| Detector Calibration                     |
|                                          |
| Last Offset Cal:   2026-03-15  [OK]      |
| Last Gain Cal:     2026-03-10  [OK]      |
| Last Defect Map:   2026-02-28  [Warn]    |
|                                          |
| [Open Calibration Wizard]                |
+------------------------------------------+
```

- **Calibration Wizard Button:** Opens multi-step wizard (SCR-CAL-001, future spec)
- **Status Icons:**
  - OK (within 30 days): #00C853
  - Warn (30–90 days): #FFD600
  - Overdue (>90 days): #D50000
- **Access:** Service Engineer, Admin only

### Account Tab — User Role Configuration

```
+------------------------------------------+
| User Account Settings                    |
|                                          |
| Username:  [admin_______________]        |
| Role:      [Administrator    v]          |
|            - Radiologist                 |
|            - Radiographer                |
|            - Service Engineer            |
|            - Administrator               |
|                                          |
| [ ] Enable multi-factor auth             |
+------------------------------------------+
```

- **Role ComboBox:** Dropdown selector for user role (updated from text field)
- **Access:** Admin configurable for other users, user can view own role (read-only)

---

### System Tab — Audit Logs — Admin Only (FR-SA-025)

```
+------------------------------------------+
| Audit Trail                              |
|                                          |
| [Date Range: 2026-03-01 to 2026-04-06]  |
| [User: All v] [Event Type: All v]        |
| [Search...                         ]     |
|                                          |
| +--------+--------+----------+---------+|
| |Timestamp|User    |Event     |Details  ||
| +--------+--------+----------+---------+|
| |2026-04-06|admin  |Login     |Success  ||
| |14:32:01  |       |          |IP:10.x  ||
| |2026-04-06|rad01  |Exposure  |P001/ACC1||
| |14:15:22  |       |          |120kV/2mA||
| +--------+--------+----------+---------+|
|                                          |
| Showing 1-50 of 1,247 entries            |
| [< Prev] [1] [2] ... [25] [Next >]      |
|                                          |
| [Export CSV]  [Export PDF]               |
+------------------------------------------+
```

**Audit Log Table Columns:**

| Column | Width | Sortable | Description |
|--------|-------|----------|-------------|
| Timestamp | 160px | Yes | ISO 8601 date-time |
| User | 120px | Yes | Username who performed action |
| Event | 150px | Yes | Event type (Login, Logout, Exposure, Config Change, etc.) |
| Details | Flexible | No | Event-specific detail string |

**Filters:**
- Date range picker (default: last 30 days)
- User dropdown (All or specific user)
- Event type dropdown (All, Login/Logout, Exposure, Configuration, Export, Error)
- Free-text search across all columns

**Export:**
- CSV: All filtered results, UTF-8 with BOM
- PDF: Formatted report with filter summary header
- Filename: `HnVue_AuditLog_YYYYMMDD.csv / .pdf`

**Access:** Admin role only. Navigation item hidden for non-admin users.

### System Tab — Software Updates

```
+------------------------------------------+
| Software Version                         |
|                                          |
| Current Version:  HnVue 2.1.0            |
| Build Date:       2026-03-28             |
| Component:        Console GUI            |
|                                          |
| [Check for Updates]                      |
|                                          |
| -- After check --                        |
| Update Available: HnVue 2.1.1            |
| Release Notes: [View]                    |
| Size: 45 MB                              |
|                                          |
| [Download & Install]                     |
|                                          |
| -- During install --                     |
| Downloading: [===========    ] 72%       |
| Installing:  [Waiting...]                |
| Estimated time: 3 minutes                |
| [Cancel]                                 |
+------------------------------------------+
```

**States:**
- **Idle:** Version display + "Check for Updates" button
- **Checking:** Spinner + "Checking for updates..."
- **Up to date:** Green checkmark + "Your software is up to date."
- **Update available:** Version info + release notes link + "Download & Install" button
- **Downloading:** Progress bar (percentage), file size, cancel button
- **Installing:** Secondary progress bar, "Do not power off" warning
- **Complete:** "Update installed. Restart required." + [Restart Now] button
- **Error:** Red banner with error reason + [Retry] button

**Access:** Admin only

---

## Default Configuration Values

### Network Configuration
| Key | Value |
|-----|-------|
| AE Title | HNVUE_SCU |
| Port | 104 |
| Timeout | 30 seconds |
| Max PDU Size | 16384 bytes |

### Storage Configuration
| Setting | Default | Type |
|---------|---------|------|
| Database Path | C:\HnVue\Data\Database | Folder |
| Image Storage | D:\HnVue\Images | Folder |
| Export Path | E:\Export | Folder |
| Auto-archive | Enabled | Toggle |
| Archive After | 90 days | Number |

### Device Settings
| Device | Status | Last Calibration |
|--------|--------|------------------|
| Detector | Connected | 2026-03-15 |
| Generator | Ready | 2026-03-10 |
| Compression Plate | OK | 2026-03-20 |

### RIS/HL7 Settings
| Setting | Default | Type |
|---------|---------|------|
| HL7 Enabled | No | Toggle |
| Interface Type | TCP/IP | Dropdown |
| Remote Host | ris.hospital.local | Text |
| Remote Port | 2575 | Number |
| Logging | Enabled | Toggle |

---

## Input Controls

### Toggle Switches
- Used for binary settings (on/off)
- Visual: Pill shape with sliding circle
- Active color: #1B4F8A (CoreTokens `--primary-main`)
- Inactive color: #2E4A6E (CoreTokens `--border-default`)
- Accessibility: ARIA toggle role

### Dropdown Selects
- Used for fixed options
- Style: Match input field styling
- Border: #2E4A6E, focus: #00AEEF
- Keyboard: Arrow keys to navigate

### Number Inputs
- Min/max validation
- Step buttons
- Unit indicators

### Folder Browsers
- Button with "..." indicator
- Opens folder selection dialog
- Shows selected path in read-only input

---

## Button Actions

### Apply
- **Style:** Primary button, background #1B4F8A
- **Action:** Save all settings
- **Validation:** Required before apply
- **Feedback:** Success toast, then return to previous screen

### Cancel
- **Style:** Secondary button
- **Action:** Discard changes, close settings
- **Confirmation:** "Discard unsaved changes?"

### Reset
- **Style:** Secondary button
- **Action:** Reset current tab to defaults
- **Confirmation:** "Reset all settings to default values?"

---

## Import/Export Configuration

### Export Configuration
- **Format:** JSON
- **Content:** All non-sensitive settings
- **Exclude:** Passwords, encryption keys
- **Filename:** HnVue_Config_YYYYMMDD.json

### Import Configuration
- **Validation:** Schema validation on load
- **Merge Option:** "Replace all" or "Merge with existing"
- **Backup:** Auto-backup before import
- **Confirmation:** Full dialog showing changes

---

## Validation

### Field-Level Validation
| Field | Rule | Error Message |
|-------|------|---------------|
| AE Title | Required, max 16 chars | "AE Title is required" |
| Port | 1-65535 | "Port must be 1-65535" |
| Timeout | 1-300 | "Timeout must be 1-300 seconds" |
| Folder Path | Must exist | "Folder does not exist" |

### Cross-Field Validation
- DICOM port must be unique across all nodes
- Storage paths must be unique
- IP addresses must be valid format

---

## Access Control

### Admin-Only Settings
- Network configuration
- Storage paths
- Device calibration
- RIS interface
- Security / session timeout
- Audit logs viewer
- Software updates

### User-Accessible Settings
- Display preferences
- Language
- Theme

### Service Engineer Settings
- Device calibration
- Diagnostic tools
- Firmware updates

---

## Color Reference (CoreTokens)

| Token | Value | Usage |
|-------|-------|-------|
| `--primary-main` | #1B4F8A | Apply button, active toggle |
| `--primary-light` | #00AEEF | Focus ring, TLS Connected accent |
| `--bg-surface` | #16213E | Page background |
| `--bg-card` | #0F3460 | Panel backgrounds |
| `--border-default` | #2E4A6E | Input borders, inactive toggle |
| `--border-focus` | #00AEEF | Focused input outline |
| `--text-primary` | #FFFFFF | Primary text |
| `--text-muted` | #B0BEC5 | Secondary/hint text |
| `--error` | #D50000 | Error states, overdue calibration |
| `--warning` | #FFD600 | Warning states, expiring cert |
| `--success` | #00C853 | TLS connected, calibration OK |

---

## PPT 명칭 변경 (슬라이드 14~21)

| 기존 | 신규 | 위치 |
|-----|-----|------|
| Login Popup | **Access Notice** | System 탭 |
| Only No matching | **Un-Matched** | RIS Code 서브탭 |
| PACS + Worklist + Print (별도 탭) | **Network** (통합 탭) | 탭 메뉴 |

---

## ViewModel Reference

**ISettingsViewModel:**
- Tab navigation and state management
- Persistence of settings changes
- Validation of network addresses and ports
- Support for all tab-specific operations

---

## Related Documents

- [Component Library](../component_library.md)
- [Network Configuration Guide](../../management/network_setup.md)

---

**Version:** 1.2
**Last Updated:** 2026-04-07
**Status:** Active
**Changes v1.2:** PPT 슬라이드 14~21 — 좌측 사이드바 → 상단 탭 네비게이션 변경, Network 탭 통합 (PACS+Worklist+Print), "Access Notice" 명칭 변경, RIS Code 서브탭 추가 (Matching/Un-Matched), Account 탭 Role ComboBox 추가, 시스템 탭 통합 (Session, Audit, Updates)
