#!/bin/bash
# HnVue UI Design Gitea Issue Creator
BASE="http://10.11.1.40:7001/api/v1/repos/DR_RnD/Console-GUI/issues"
TOKEN="a4cb79626194b34a2d52835de05fb770162af014"
AUTH="Authorization: token $TOKEN"
CT="Content-Type: application/json"

create_issue() {
  local file="$1"
  curl -s -X POST "$BASE" -H "$AUTH" -H "$CT" -d @"$file" | grep -o '"number":[0-9]*' | head -1
}

# Issue 44: PatientListView DataGrid
cat > /tmp/issue44.json << 'EOF'
{
  "title": "[UI-Design] PatientListView DataGrid + icon button redesign",
  "body": "## Goal\nUpgrade PatientListView from plain ListBox to styled DataGrid matching CoreTokens.\n\n## Tasks\n- Replace ListBox with DataGrid (columns: Status|PatientID|Name|DOB|Procedure)\n- Row height 44px (IEC 62366 touch target)\n- Search input: accent border focus\n- Buttons with Segoe MDL2 icons: Search(E721), Add(E710)\n- EMRG badge: #D50000 background\n- Row hover: bg-card (#0F3460)\n- Match screenshot layout exactly\n\n## Reference\n- docs/ui_mockups/02-worklist.html\n- docs/ui_mockups/screens/worklist.md\n- CoreTokens: src/HnVue.UI/Themes/tokens/CoreTokens.xaml"
}
EOF
echo -n "Issue 44: "
create_issue /tmp/issue44.json

# Issue 45: ImageViewerView
cat > /tmp/issue45.json << 'EOF'
{
  "title": "[UI-Design] ImageViewerView toolbar icon buttons + viewer styling",
  "body": "## Goal\nImprove ImageViewerView with icon buttons and proper dark theme viewer.\n\n## Tasks\n- ZOOM IN/OUT/RESET W/L: add Segoe MDL2 icons (E8A3/E71F/E72C)\n- Keep ALL CAPS button text style matching current UI\n- Button style: outline white border (match screenshot exactly)\n- Image area: true black #090909 for X-ray viewing\n- Placeholder: center icon + text\n- Zoom indicator styled with CoreTokens\n- Measurement/annotation toolbar (future placeholder)\n\n## Reference\n- screenshot_logged_in.png (current UI)\n- docs/ui_mockups/screens/acquisition.md"
}
EOF
echo -n "Issue 45: "
create_issue /tmp/issue45.json

# Issue 46: DoseDisplayView
cat > /tmp/issue46.json << 'EOF'
{
  "title": "[UI-Design] DoseDisplayView real-time DAP panel implementation",
  "body": "## Goal\nImplement dose monitoring panel that shows in right panel below WorkflowView.\n\n## Tasks\n- DAP real-time display (large number, unit label)\n- DRL gauge bar: 0-69% green, 70-89% amber, 90%+ red\n- EI/DI indicator with color coding\n- Compact design to fit in 260px right panel\n- Use ComponentTokens: HnVue.Component.DosePanel.*\n\n## Reference\n- docs/ui_mockups/screens/dose-monitoring.md\n- SPEC-UI-001 §FR-DM-001"
}
EOF
echo -n "Issue 46: "
create_issue /tmp/issue46.json

# Issue 47: MainWindow header
cat > /tmp/issue47.json << 'EOF'
{
  "title": "[UI-Design] MainWindow header EMERGENCY button + status bar enhancement",
  "body": "## Goal\nApply EmergencyStopButton style and improve header/status bar.\n\n## Tasks\n- EMERGENCY button: use HnVue.EmergencyStopButton style (#D50000)\n- Header height 48px: add Segoe MDL2 logo icon\n- Logout button: outline style\n- Status bar: DICOM status indicator, session info\n- TLS banner: yellow warning with icon\n\n## Reference\n- src/HnVue.App/MainWindow.xaml\n- HnVue.EmergencyStopButton style in HnVueTheme.xaml"
}
EOF
echo -n "Issue 47: "
create_issue /tmp/issue47.json

# Issue 48: LoginView complete
cat > /tmp/issue48.json << 'EOF'
{
  "title": "[UI-Design] LoginView complete redesign with CoreTokens",
  "body": "## Goal\nComplete LoginView redesign using CoreTokens and MahApps.Metro styles.\n\n## Tasks\n- Background: HnVue.Semantic.Surface.Page (#1A1A2E)\n- Login card: HnVue.Semantic.Surface.Panel (#16213E), CornerRadius 12px\n- Logo: gradient rectangle HnVueBrush.Primary→PrimaryLight\n- Inputs: dark bg, Accent focus border (#00AEEF)\n- LOGIN button: HnVue.PrimaryButton style (filled, 44px height)\n- Error: HnVue.Semantic.Status.Emergency brush\n- Loading progress bar: accent color\n\n## Closes #43 (existing issue)"
}
EOF
echo -n "Issue 48: "
create_issue /tmp/issue48.json

echo ""
echo "Done"
