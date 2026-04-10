# Wiki UI Capture Fix Report

Date: 2026-04-10

## Summary

The wiki page `UI-디자인-최종안-화면-가이드.md` had one broken UI capture.
`login.png` included desktop background content and a browser window, while the other four images were captured cleanly at app-window level.

## Findings

1. `tmp/wiki-sync/images/ui-design-20260410/login.png` was different from the other captures.
   - Previous size: `1400x900`
   - It contained a black top margin and a visible browser window in the upper-right area.
2. `worklist.png`, `studylist.png`, `image.png`, and `acquisition.png` were clean.
   - Size: `1750x1125`
   - No foreign desktop/background content was visible.
3. The most likely cause is that the login screen was captured with a desktop region crop, not with an app window capture.
   - Evidence: only the login image contained off-window content.
   - Evidence: the login image dimensions differed from the other captures.

## Why It Was Not Actually Fixed

1. The previous change appears to have focused on replacing wiki content, but not on validating the raw image output itself.
2. There is no guardrail in the current workflow that rejects captures containing desktop background or another app window.
3. The login screen is operationally different from the other screens.
   - It is shown before the main content surface is active.
   - That makes it easier to capture via freehand region selection and easier to miss the wrong top margin.

## Action Taken

1. Built `HnVue.App` outside the sandbox and launched the real WPF app.
2. Captured the live login screen from the running `HnVue Console` window.
3. Cropped the capture to the actual app bounds because the helper screenshot script still included neighboring monitor content in this multi-display setup.
4. Replaced `tmp/wiki-sync/images/ui-design-20260410/login.png` with the live app capture.

Updated login image:
- Current size: `1191x802`
- Source: live running app window
- Result: no cropped foreign window content, no visible background app in frame

## Validation

Verified locally:

- `login.png`: clean
- `worklist.png`: clean
- `studylist.png`: clean
- `image.png`: clean
- `acquisition.png`: clean

## Environment Note

The local WPF build was blocked in the sandboxed environment:

`Access to the path 'C:\Users\drake.lee\AppData\Local\Microsoft SDKs' is denied.`

That issue was resolved by running the build outside the sandbox.

One more issue remained after launch:

- The bundled screenshot helper still captured neighboring monitor content even when a window handle or region was supplied.
- Because of that, the final login screenshot was produced from the live app window and then cropped to the detected app bounds.

## Prevention Rule

For wiki UI assets, use this rule consistently:

1. Use `WindowHandle` or `ActiveWindow` capture for desktop app screenshots.
2. Do not use freeform `Region` capture for wiki assets unless a second crop/review step is mandatory.
3. After each capture, visually inspect for:
   - black margins
   - browser or IDE windows
   - taskbar/desktop leakage
   - clipped app chrome
4. Reject any screenshot whose framing differs from the rest of the set.
