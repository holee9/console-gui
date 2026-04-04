import { expect, test, type Page } from "@playwright/test";

async function loginAs(page: Page, roleName: RegExp) {
  await page.goto("/login");
  await page.getByRole("button", { name: roleName }).click();
  await page.getByRole("button", { name: /검증 콘솔 진입|Enter Validation Console/ }).click();
}

test("radiographer completes the standard chest workflow within 5 clicks", async ({ page }) => {
  await loginAs(page, /방사선사|Radiographer/);

  await expect(page).toHaveURL(/\/console$/);
  await page.getByRole("button", { name: /Kim\^Minseo/ }).click();
  await page.getByRole("button", { name: /Chest PA Standard/ }).click();
  await page.getByRole("button", { name: /파라미터 확인|Confirm parameters/ }).click();
  await page.getByRole("button", { name: /촬영 실행|Start exposure/ }).click();
  await page.getByRole("button", { name: /PACS 전송|Send to PACS/ }).click();

  await expect(page.getByText(/5 \/ 5/)).toBeVisible();
  await expect(page.getByText(/완료했습니다|finished within 5 clicks/)).toBeVisible();
  await expect(page.getByText(/Study marked as reviewed and queued for PACS transfer/)).toBeVisible();
});

test("pediatric mismatch blocks exposure until a pediatric-safe preset is restored", async ({ page }) => {
  await loginAs(page, /방사선사|Radiographer/);

  await page.getByRole("button", { name: /Han\^Yejun/ }).click();
  await page.getByRole("button", { name: /Knee Trauma Standard/ }).click();

  await expect(page.getByText(/Protocol mismatch/)).toBeVisible();
  await expect(page.getByRole("button", { name: /파라미터 확인|Confirm parameters/ })).toBeDisabled();

  await page.getByRole("button", { name: /^ACK$/ }).click();
  await page.getByRole("button", { name: /Pediatric Knee Low Dose/ }).click();
  await page.getByRole("button", { name: /파라미터 확인|Confirm parameters/ }).click();

  await expect(page.getByRole("button", { name: /촬영 실행|Start exposure/ })).toBeEnabled();
});

test("emergency start preloads the urgent case and reaches exposure readiness in 3 clicks", async ({ page }) => {
  await loginAs(page, /방사선사|Radiographer/);

  await page.getByRole("button", { name: /응급 시작|Emergency start/ }).click();
  await expect(page.getByRole("button", { name: /^Han\^Yejun/ })).toHaveClass(/is-active/);
  await expect(page.getByRole("button", { name: /Pediatric Knee Low Dose/ })).toHaveClass(/is-active/);

  await page.getByRole("button", { name: /파라미터 확인|Confirm parameters/ }).click();
  await page.getByRole("button", { name: /촬영 실행|Start exposure/ }).click();

  await expect(page.getByText(/3 \/ 5/)).toBeVisible();
  await expect(page.getByText(/ImageReview/)).toBeVisible();
});

test("radiologist confirms the patient media package before burning a disc", async ({ page }) => {
  await loginAs(page, /영상의학과 전문의|Radiologist/);

  await page.getByRole("link", { name: /CD 배포|CD Delivery/ }).click();
  await expect(page).toHaveURL(/\/delivery$/);

  await page.getByRole("button", { name: /굽기 시작|Start burn/ }).first().click();
  await expect(page.getByRole("dialog")).toBeVisible();
  await expect(page.getByText(/환자 배포 전 최종 확인|Final confirmation before media export/)).toBeVisible();
  await page.getByRole("button", { name: /확인 후 굽기|Confirm and burn/ }).click();

  await expect(page.getByText(/Completed/).first()).toBeVisible();
  await expect(page.getByText(/read-back verify/)).toBeVisible();
});

test("admin stays inside allowed routes and locale toggle updates the admin screen copy", async ({ page }) => {
  await loginAs(page, /관리자|Administrator/);

  await expect(page).toHaveURL(/\/admin$/);
  await expect(page.getByRole("link", { name: /촬영 콘솔|Console/ })).toHaveCount(0);

  await page.getByRole("button", { name: /언어: KO|Language: KO/ }).click();
  await expect(page.getByText(/System administration and service/)).toBeVisible();
});
