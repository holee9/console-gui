# Screen Specification: Dose Monitoring Panel

## Overview

| Property | Value |
|----------|-------|
| **Panel ID** | PANEL-DM-001 |
| **Priority** | Critical |
| **Type** | Embedded panel (not a standalone screen) |
| **Host Screen** | SCR-ACQ-001 (Acquisition) |
| **Regulatory** | FR-DM-001, FR-DM-015, IEC 62366-1 |
| **Purpose** | Real-time radiation dose monitoring with threshold-based safety alerts |

This panel is embedded in the left column of the Acquisition screen, below the Patient Information section. It is not a separate navigation destination.

---

## Panel Layout

```
+-------------------------------+
| Dose Monitoring               |
+-------------------------------+
| DAP                           |
|   2.34 Gy·cm²                 |
|                               |
| DRL                           |
|   [||||||||--]  72%           |
|   72% of reference level      |
|                               |
| EI     320    [green dot]     |
| DI    +0.8    [green dot]     |
|                               |
| [View Dose History]           |
+-------------------------------+
```

### Panel Dimensions
- Width: Fills left panel (280px minus padding)
- Minimum Height: 160px
- Padding: 12px
- Background: `--bg-panel` (#16213E)
- Border-top: 1px solid `--border-default` (#2E4A6E)

---

## Real-Time DAP Display

DAP (Dose Area Product) reflects the cumulative radiation dose for the current study.

| Property | Value |
|----------|-------|
| Label | "DAP" |
| Value format | X.XX Gy·cm² |
| Update frequency | After each exposure completes |
| During exposure | Updates live (if hardware supports streaming) |
| Initial value | 0.00 Gy·cm² |
| Font size | 20px, bold |
| Color | White (#FFFFFF) normally; follows DRL color at thresholds |

---

## DRL Progress Visualization

The DRL (Diagnostic Reference Level) bar shows the cumulative dose as a percentage of the regional reference level for the selected procedure.

### Visual Gauge Bar

```
DRL: 72%
[||||||||--]
 ^filled    ^empty
```

- Bar width: 100% of panel content width
- Bar height: 12px
- Border radius: 6px
- Background (empty): `--bg-card` (#0F3460)
- Fill color: Determined by threshold (see table below)
- Percentage label: Right-aligned above or beside bar

### Color Thresholds

| DRL Range | Fill Color | Token | Label Color | Action Required |
|-----------|-----------|-------|-------------|-----------------|
| 0–69% | #00C853 | `--status-safe` | #00C853 | None |
| 70–89% | #FFD600 | `--status-warning` | #FFD600 | Amber warning banner |
| 90–99% | #FF6D00 | `--status-blocked` | #FF6D00 | Confirmation modal before next exposure |
| 100%+ | #D50000 | `--status-emergency` | #D50000 | Physician PIN authorization |

---

## Warning Banners

Banners appear inline below the DRL gauge and are dismissible only after the exposure constraint is resolved.

### 70–89%: Warning Banner

```
+----------------------------------------------+
|  Dose approaching DRL limit (72%).           |
|  Review exposure parameters before           |
|  continuing.                                 |
+----------------------------------------------+
```

| Property | Value |
|----------|-------|
| Background | rgba(#FFD600, 0.15) |
| Border-left | 4px solid #FFD600 |
| Icon | Warning triangle |
| Text color | #FFD600 |
| Dismissible | No — remains until DRL drops below 70% or study closes |

---

## 90–99%: Confirmation Modal

Displayed when user presses Expose and DRL is between 90% and 99%.

```
+--------------------------------------------+
| Dose Near DRL Limit                        |
|                                            |
| Current DRL: 93%                           |
| Proceeding will increase cumulative dose   |
| above the diagnostic reference level.      |
|                                            |
| Confirm to continue with this exposure.    |
|                                            |
|     [Cancel Exposure]   [Confirm Proceed]  |
+--------------------------------------------+
```

| Property | Value |
|----------|-------|
| Trigger | Expose attempted when DRL 90–99% |
| Header background | #FF6D00 at 20% opacity |
| Header text | "Dose Near DRL Limit" |
| Body text color | #FFFFFF |
| Button: Cancel Exposure | Aborts exposure, returns to IDLE |
| Button: Confirm Proceed | Proceeds with exposure; logs override with timestamp |
| Logged fields | User ID, timestamp, DRL at time of override |

---

## 100%+: Physician PIN Authorization Modal

Displayed when DRL reaches or exceeds 100%. No exposure is permitted until a physician enters their PIN.

```
+--------------------------------------------+
| DRL Limit Exceeded                         |
|                                            |
| Current DRL: 102%                          |
| Physician authorization is required        |
| before any further exposures.              |
|                                            |
| Physician PIN:                             |
| [________]                                 |
|                                            |
|  [Cancel]              [Authorize]         |
+--------------------------------------------+
```

| Property | Value |
|----------|-------|
| Trigger | DRL >= 100% on any exposure attempt |
| Header background | #D50000 at 20% opacity |
| Header text | "DRL Limit Exceeded" |
| PIN field | Masked input, numeric, 4–8 digits |
| Button: Cancel | Returns to IDLE, no exposure |
| Button: Authorize | Validates PIN; if correct, allows one exposure and logs authorization |
| Failed PIN | "Incorrect PIN. Please try again." shown in #D50000 |
| Max PIN attempts | 3 — locks exposure capability after 3 failures, requires supervisor reset |
| Logged fields | Physician ID, timestamp, DRL at authorization |

---

## EI and DI Indicators

EI (Exposure Index) and DI (Deviation Index) are calculated from the acquired image after processing.

### Display Format

```
EI:   320    [green dot]
DI:  +0.8    [green dot]
```

### Color Coding

| Range | Color | Token | Meaning |
|-------|-------|-------|---------|
| DI: -2.0 to +2.0 | #00C853 | `--status-safe` | Optimal exposure |
| DI: -3.0 to -2.0 or +2.0 to +3.0 | #FFD600 | `--status-warning` | Suboptimal, review technique |
| DI: beyond ±3.0 | #D50000 | `--status-emergency` | Significant over/underexposure |

### Indicator Dot
- Circular dot, 10px diameter
- Color matches the DI range above
- Positioned to the right of the value

### Update Timing
- EI and DI are only shown after image processing is complete (COMPLETE state)
- During IDLE, PREP, EXPOSING, PROCESSING: fields show "--"

---

## Dose History Link

At the bottom of the panel, a link opens the dose history for the current patient and study.

```
[View Dose History]
```

| Property | Value |
|----------|-------|
| Style | Text link, underlined on hover |
| Color | `--accent` (#00AEEF) |
| Action | Opens dose history dialog (modal or side panel) |
| Content | Table of all exposures in current study: timestamp, DAP, EI, DI, operator |

---

## Related Documents

- [Acquisition Screen](acquisition.md)
- [Component Library](../component_library.md)
- [Regulatory: FR-DM-001, FR-DM-015](../../regulatory/IEC62366.md)

---

**Version:** 1.0
**Last Updated:** 2026-04-06
**Status:** Draft
