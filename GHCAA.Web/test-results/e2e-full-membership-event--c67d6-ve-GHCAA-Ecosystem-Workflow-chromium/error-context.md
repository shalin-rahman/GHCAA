# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: e2e\full-membership-event-workflow.spec.ts >> Comprehensive GHCAA Ecosystem Workflow
- Location: tests\e2e\full-membership-event-workflow.spec.ts:79:5

# Error details

```
Test timeout of 90000ms exceeded.
```

```
Error: locator.selectOption: Test timeout of 90000ms exceeded.
Call log:
  - waiting for locator('select[name="deg_0"]')

```

# Page snapshot

```yaml
- generic [ref=e3]:
  - navigation [ref=e4]:
    - generic [ref=e5]:
      - generic [ref=e6] [cursor=pointer]:
        - img "Logo" [ref=e7]
        - generic [ref=e8]:
          - generic [ref=e9]: GHCAA
          - generic [ref=e10]: Govt. Haraganga College Alumni Association
      - generic [ref=e11]:
        - link "Home" [ref=e12] [cursor=pointer]:
          - /url: /
        - link "About" [ref=e13] [cursor=pointer]:
          - /url: /about
        - link "News" [ref=e14] [cursor=pointer]:
          - /url: /news
        - link "Contact" [ref=e15] [cursor=pointer]:
          - /url: /contact
        - link "Login" [ref=e16] [cursor=pointer]:
          - /url: /login
  - main [ref=e17]:
    - generic [ref=e21]:
      - generic [ref=e22]:
        - generic [ref=e23]:
          - img "GHCAA" [ref=e24]
          - heading "Member Registry" [level=3] [ref=e25]:
            - text: Member
            - text: Registry
        - generic [ref=e26]:
          - generic [ref=e27] [cursor=pointer]:
            - generic [ref=e28]: ✓
            - generic [ref=e29]:
              - generic [ref=e30]: Induction
              - generic [ref=e31]: Identity & Reachability
          - generic [ref=e32]:
            - generic [ref=e33]: "02"
            - generic [ref=e34]:
              - generic [ref=e35]: Background
              - generic [ref=e36]: Academic & Career Milestones
          - generic [ref=e37]:
            - generic [ref=e38]: "03"
            - generic [ref=e39]:
              - generic [ref=e40]: Finalize
              - generic [ref=e41]: Registry Filing & Subscription
        - generic [ref=e42]:
          - paragraph [ref=e43]: Guidance Required?
          - link "haragangian@gmail.com" [ref=e44] [cursor=pointer]:
            - /url: mailto:haragangian@gmail.com
      - generic [ref=e46]:
        - generic [ref=e47]:
          - generic [ref=e48]:
            - heading "Background & Milestones" [level=2] [ref=e49]
            - paragraph [ref=e50]: Declare your academic timeline and professional career milestones.
          - generic [ref=e51]:
            - heading "🎓 Academic Records" [level=3] [ref=e52]
            - generic [ref=e54]:
              - 'heading "Educational Milestone #1" [level=4] [ref=e56]'
              - generic [ref=e57]:
                - generic [ref=e58]:
                  - generic [ref=e59]: Institution / University Name *
                  - textbox "Select or type institution" [disabled] [ref=e60]: Govt. Haraganga College
                - generic [ref=e62] [cursor=pointer]:
                  - checkbox "Was this achieved at Govt. Haraganga College?" [checked] [disabled] [ref=e63]
                  - generic [ref=e64]: Was this achieved at Govt. Haraganga College?
                - generic [ref=e65]:
                  - generic [ref=e66]: Degree Conferred *
                  - combobox [ref=e67]:
                    - option "Select Degree" [disabled]
                    - option "HSC" [selected]
                    - option "Bachelor (Pass)"
                    - option "Bachelor (Honours)"
                    - option "Masters"
                    - option "PGD"
                    - option "PhD"
                    - option "Medicine"
                    - option "Engineering"
                    - option "Law"
                - generic [ref=e68]:
                  - generic [ref=e69]: Major Cluster / Subject *
                  - combobox [ref=e70]:
                    - option "Select Major/Subject" [disabled]
                    - option "Science"
                    - option "Arts & Humanities"
                    - option "Business Studies"
                - generic [ref=e71]:
                  - generic [ref=e72]: Admission / Enrollment Year *
                  - combobox [ref=e73]:
                    - option "Select Year" [disabled]
                    - option "2026"
                    - option "2025"
                    - option "2024"
                    - option "2023"
                    - option "2022"
                    - option "2021"
                    - option "2020"
                    - option "2019"
                    - option "2018"
                    - option "2017"
                    - option "2016"
                    - option "2015"
                    - option "2014"
                    - option "2013"
                    - option "2012"
                    - option "2011"
                    - option "2010"
                    - option "2009"
                    - option "2008"
                    - option "2007"
                    - option "2006"
                    - option "2005"
                    - option "2004"
                    - option "2003"
                - generic [ref=e74]:
                  - generic [ref=e75]: Passing Year *
                  - combobox [ref=e76]:
                    - option "Select Year" [disabled]
                    - option "2026"
                    - option "2025"
                    - option "2024"
                    - option "2023"
                    - option "2022"
                    - option "2021"
                    - option "2020"
                    - option "2019"
                    - option "2018"
                    - option "2017"
                    - option "2016"
                    - option "2015"
                    - option "2014"
                    - option "2013"
                    - option "2012"
                    - option "2011"
                    - option "2010"
                    - option "2009"
                    - option "2008"
                    - option "2007"
                    - option "2006"
                    - option "2005"
                    - option "2004"
                    - option "2003"
            - button "+ Add Educational Milestone" [ref=e77] [cursor=pointer]:
              - generic [ref=e78]: +
              - text: Add Educational Milestone
          - generic [ref=e79]:
            - heading "💼 Vocation & Career" [level=3] [ref=e80]
            - generic [ref=e82]:
              - 'heading "Professional Milestone #1" [level=4] [ref=e84]'
              - generic [ref=e85]:
                - generic [ref=e86]:
                  - generic [ref=e87]: Organisation / Company *
                  - textbox "Employer Title" [ref=e88]
                - generic [ref=e89]:
                  - generic [ref=e90]: Designation / Role *
                  - textbox "e.g. Chief Technologist" [ref=e91]
                - generic [ref=e92]:
                  - generic [ref=e93]: Functional Sector
                  - combobox [ref=e94]:
                    - option "Select Sector" [disabled] [selected]
                    - option "Ready-made Garments (RMG)"
                    - option "Textiles & Spinning"
                    - option "Pharmaceuticals"
                    - option "Banking & Financial Services"
                    - option "Information Technology (IT) & Software"
                    - option "Telecommunications"
                    - option "Agriculture & Crop Production"
                    - option "Fisheries & Aquaculture"
                    - option "Livestock & Poultry"
                    - option "Agro-processing & Food Production"
                    - option "Leather & Footwear"
                    - option "Jute & Jute Goods"
                    - option "Light Engineering"
                    - option "Electronics & Electrical Appliances"
                    - option "Real Estate & Housing"
                    - option "Construction & Infrastructure"
                    - option "Healthcare & Medical Services"
                    - option "Education & Research"
                    - option "Tourism & Hospitality"
                    - option "Power, Energy & Mineral Resources"
                    - option "Steel & Re-rolling"
                    - option "Cement"
                    - option "Ceramics"
                    - option "Chemicals & Fertilizers"
                    - option "Shipbuilding"
                    - option "Transportation & Logistics"
                    - option "Fast-Moving Consumer Goods (FMCG)"
                    - option "Paper & Printing"
                    - option "Plastic & Rubber Products"
                    - option "Insurance"
                    - option "Advertising & Media"
                    - option "Legal & Consultancy Services"
                    - option "Public Administration & Defense"
                - generic [ref=e95]:
                  - generic [ref=e96]: Work Location
                  - textbox "City, Country" [ref=e97]
                - generic [ref=e98]:
                  - generic [ref=e99]: Start Date *
                  - textbox "dd-mm-yyyy" [ref=e100]
                - generic [ref=e101]:
                  - generic [ref=e102]: Relief Date (End) *
                  - textbox "dd-mm-yyyy" [ref=e103]
                - generic [ref=e105] [cursor=pointer]:
                  - checkbox "I currently serve in this capacity (Active Occupation)" [ref=e106]
                  - generic [ref=e107]: I currently serve in this capacity (Active Occupation)
            - button "+ Add Professional Experience" [ref=e108] [cursor=pointer]:
              - generic [ref=e109]: +
              - text: Add Professional Experience
          - generic [ref=e110]:
            - heading "🆘 Emergency Protocol Personnel" [level=4] [ref=e111]
            - generic [ref=e112]:
              - generic [ref=e113]:
                - generic [ref=e114]: Emergency Contact Person Legal Name *
                - textbox [ref=e115]
              - generic [ref=e116]:
                - generic [ref=e117]: Consanguinity / Relationship *
                - textbox [ref=e118]
              - generic [ref=e119]:
                - generic [ref=e120]: Direct Phone Channel *
                - textbox "01XXXXXXXXX" [ref=e121]
        - generic [ref=e122]:
          - button "← Previous Step" [ref=e124] [cursor=pointer]
          - button "Continue Assessment →" [active] [ref=e126] [cursor=pointer]
  - contentinfo [ref=e128]:
    - generic [ref=e129]:
      - generic [ref=e130]:
        - img "Official Seal" [ref=e131]
        - paragraph [ref=e132]: "\"Sharing Heritage, Aligning Lives, Integrating Networks\""
        - paragraph [ref=e133]: ঐতিহ্যের বিনিময়, জীবনের সমন্বয় ও সংহতির সেতুবন্ধন
      - generic [ref=e134]:
        - heading "Alumni Hub" [level=4] [ref=e135]
        - link "Member Directory" [ref=e136] [cursor=pointer]:
          - /url: /directory
        - link "Member Registry" [ref=e137] [cursor=pointer]:
          - /url: /register
        - link "Mentorship Hub" [ref=e138] [cursor=pointer]:
          - /url: /mentorship
        - link "Professional Hub" [ref=e139] [cursor=pointer]:
          - /url: /professionals
      - generic [ref=e140]:
        - heading "Resources" [level=4] [ref=e141]
        - link "News & Media" [ref=e142] [cursor=pointer]:
          - /url: /news
        - link "Digital Magazine" [ref=e143] [cursor=pointer]:
          - /url: /magazine
        - link "Community Blog" [ref=e144] [cursor=pointer]:
          - /url: /articles
      - generic [ref=e145]:
        - heading "Governance" [level=4] [ref=e146]
        - link "Executive Committee" [ref=e147] [cursor=pointer]:
          - /url: /committee
        - link "Financial Portal" [ref=e148] [cursor=pointer]:
          - /url: /financials
        - link "Vision & Mission" [ref=e149] [cursor=pointer]:
          - /url: /about
        - link "Contact Secretariat" [ref=e150] [cursor=pointer]:
          - /url: /contact
      - generic [ref=e151]:
        - heading "Sitemap" [level=4] [ref=e152]
        - paragraph [ref=e153]:
          - link "Secretariat Login" [ref=e154] [cursor=pointer]:
            - /url: /login
        - paragraph [ref=e155]:
          - strong [ref=e156]: "Registered Office:"
          - text: Govt. Haraganga College Campus, Munshiganj, Bangladesh.
        - paragraph [ref=e157]:
          - strong [ref=e158]: "Enquiries:"
          - text: haragangian@gmail.com
        - generic [ref=e159]:
          - link "FB" [ref=e160] [cursor=pointer]:
            - /url: "#"
          - link "WA" [ref=e161] [cursor=pointer]:
            - /url: "#"
          - link "YT" [ref=e162] [cursor=pointer]:
            - /url: "#"
    - generic [ref=e164]:
      - img "Logo" [ref=e165]
      - paragraph [ref=e166]: © 2026 | Govt. Haraganga College Alumni Association (HARAGANGIAN). Registered under the Registrar of Societies.
```

# Test source

```ts
  27  |   const tmpDir = path.join(process.cwd(), 'test-results', 'stubs');
  28  |   fs.mkdirSync(tmpDir, { recursive: true });
  29  |   const imgPath = path.join(tmpDir, 'stub.jpg');
  30  |   if (!fs.existsSync(imgPath)) {
  31  |     const jpegBytes = Buffer.from(
  32  |       '/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8U' +
  33  |       'HRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgN' +
  34  |       'DRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIy' +
  35  |       'MjL/wAARCAABAAEDASIAAhEBAxEB/8QAFAABAAAAAAAAAAAAAAAAAAAACf/EABQQAQAAAAAA' +
  36  |       'AAAAAAAAAAAAAP/EABQBAQAAAAAAAAAAAAAAAAAAAAD/xAAUEQEAAAAAAAAAAAAAAAAAAAAA' +
  37  |       '/9oADAMBAAIRAxEAPwCwABmX/9k=',
  38  |       'base64'
  39  |     );
  40  |     fs.writeFileSync(imgPath, jpegBytes);
  41  |   }
  42  |   return imgPath;
  43  | }
  44  | 
  45  | // ─────────────────────────────────────────────────────────────────────────────
  46  | // Helpers
  47  | // ─────────────────────────────────────────────────────────────────────────────
  48  | async function loginAs(page: Page, username: string, password: string, role: 'admin' | 'member') {
  49  |   await page.goto('/login');
  50  |   await page.waitForLoadState('networkidle');
  51  | 
  52  |   const userInput = page.locator(
  53  |     'input[formControlName="username"], input[name="username"], input[placeholder*="Username"]'
  54  |   ).first();
  55  |   const passInput = page.locator(
  56  |     'input[formControlName="password"], input[name="password"], input[placeholder*="Password"]'
  57  |   ).first();
  58  |   const submitBtn = page.locator('button[type="submit"], button:has-text("Login")').first();
  59  | 
  60  |   await userInput.fill(username);
  61  |   await passInput.fill(password);
  62  |   await submitBtn.click();
  63  | 
  64  |   if (role === 'admin') {
  65  |     await expect(page).toHaveURL(/.*admin\/(dashboard|approvals|members|events)/, { timeout: 15000 });
  66  |   } else {
  67  |     await expect(page).toHaveURL(/.*portal\/(dashboard|profile|events)/, { timeout: 15000 });
  68  |   }
  69  | }
  70  | 
  71  | async function logout(page: Page) {
  72  |   await page.evaluate(() => { localStorage.clear(); sessionStorage.clear(); });
  73  |   await page.goto('/login');
  74  | }
  75  | 
  76  | // ─────────────────────────────────────────────────────────────────────────────
  77  | // UNIFIED E2E WORKFLOW — Optimized for speed and deterministic execution
  78  | // ─────────────────────────────────────────────────────────────────────────────
  79  | test('Comprehensive GHCAA Ecosystem Workflow', async ({ page, request }) => {
  80  |   test.slow();
  81  |   page.on('dialog', (dialog) => dialog.accept());
  82  |   const imgPath = stubImagePath();
  83  |   page.on('console', msg => console.log(`BROWSER [${msg.type()}]: ${msg.text()}`));
  84  |   page.on('pageerror', err => console.log(`BROWSER ERROR: ${err.message}`));
  85  |   page.on('requestfailed', request => console.log(`BROWSER REQ FAILED: ${request.url()} - ${request.failure()?.errorText}`));
  86  |   page.on('response', async response => {
  87  |     if (response.status() >= 400) {
  88  |       const body = await response.text().catch(() => 'No body');
  89  |       console.log(`BROWSER REQ ERROR: ${response.url()} -> ${response.status()}\nBody: ${body}`);
  90  |     }
  91  |   });
  92  | 
  93  |   // ── STEP 1: New Member Registration ───────────────────────────────────────
  94  |   console.log('--- Step 1: Member Registration ---');
  95  |   await page.goto('/register');
  96  |   await page.waitForLoadState('networkidle');
  97  | 
  98  |   await page.locator('input[name="fullName"]').fill(NEW_NAME);
  99  |   await page.locator('input[name="fatherName"]').fill('E2E Father');
  100 |   await page.locator('input[name="motherName"]').fill('E2E Mother');
  101 |   await page.locator('input[name="dateOfBirth"]').fill('15-06-1990'); // Standard format
  102 |   await page.locator('input[name="dateOfBirth"]').blur();
  103 |   
  104 |   await page.locator('input[name="nid"]').fill(NEW_NID);
  105 |   await page.locator('select[name="gender"]').selectOption('Male');
  106 |   await page.locator('select[name="bloodGroup"]').selectOption('APositive');
  107 |   await page.locator('select[name="tShirtSize"]').selectOption('L');
  108 |   await page.locator('select[name="membershipType"]').selectOption('General');
  109 | 
  110 |   // Dynamic fee check - using innerText match as fallback for visual consistency
  111 |   await page.waitForFunction(() => document.body.innerText.match(/1[,.]?000|500/), { timeout: 10000 });
  112 | 
  113 |   await page.locator('input[name="email"]').fill(NEW_EMAIL);
  114 |   await page.locator('input[name="mobileNo"]').fill(NEW_MOBILE);
  115 |   await page.locator('textarea[name="presentAddress"]').fill('123 E2E Street, Dhaka');
  116 |   await page.locator('textarea[name="permanentAddress"]').fill('456 Home Village');
  117 | 
  118 |   // Verify no validation errors before continuing
  119 |   const step1Errors = page.locator('.text-red-500, .invalid-feedback').filter({ visible: true });
  120 |   if (await step1Errors.count() > 0) {
  121 |     throw new Error(`Step 1 Validation Error: ${await step1Errors.first().innerText()}`);
  122 |   }
  123 |   await page.locator('button:has-text("Continue Assessment")').click();
  124 |   await page.waitForSelector('text=Background & Milestones', { timeout: 15000 });
  125 | 
  126 |   // Academic Info — first record is pre-seeded with Haraganga College
> 127 |   await page.locator('select[name="deg_0"]').selectOption('HSC');
      |                                              ^ Error: locator.selectOption: Test timeout of 90000ms exceeded.
  128 |   await page.locator('select[name="sub_0"]').selectOption('Science');
  129 |   await page.locator('select[name="adm_0"]').selectOption('2006');
  130 |   await page.locator('select[name="pass_0"]').selectOption('2008');
  131 | 
  132 |   // Professional History (Standard date format)
  133 |   await page.getByPlaceholder(/Employer Title/i).first().fill('GHCAA Test Corp');
  134 |   await page.getByPlaceholder(/Chief Technologist/i).first().fill('Software Engineer');
  135 |   await page.locator('select').filter({ hasText: /Select Sector/ }).selectOption({ index: 1 });
  136 |   await page.locator('input[placeholder="dd-mm-yyyy"]').first().fill('01-01-2010'); 
  137 |   
  138 |   const isCurrent = page.locator('label').filter({ hasText: /I currently serve/i }).locator('input[type="checkbox"]');
  139 |   if (await isCurrent.isVisible() && !(await isCurrent.isChecked())) await isCurrent.check();
  140 | 
  141 |   await page.locator('input[name*="emergencyContactName"]').fill('Emergency Contact');
  142 |   await page.locator('input[name*="emergencyContactRelation"]').fill('Sibling');
  143 |   await page.locator('input[name*="emergencyContactPhone"]').fill('01898765432');
  144 | 
  145 |   await page.locator('button:has-text("Continue Assessment")').click();
  146 |   // Wait for the actual content of Step 3, not the stepper
  147 |   await page.waitForSelector('h2:has-text("Registry Filing")', { timeout: 15000 });
  148 |   
  149 |   const cashPayment = page.locator('.payment-card', { hasText: /Cash.*Manual Receipt/i }).first();
  150 |   await cashPayment.waitFor({ state: 'visible', timeout: 30000 });
  151 |   await cashPayment.click();
  152 |   
  153 |   // Targeted file uploads using container labels for precision
  154 |   const photoContainer = page.locator('.form-group', { hasText: /Profile Photo/i });
  155 |   await photoContainer.locator('input[type="file"]').setInputFiles(imgPath);
  156 | 
  157 |   const certContainer = page.locator('.form-group', { hasText: /Academic Certificate/i });
  158 |   await certContainer.locator('input[type="file"]').setInputFiles(imgPath);
  159 | 
  160 |   // Payment portal file drop for receipt
  161 |   const receiptDrop = page.locator('app-payment-portal .file-drop');
  162 |   if (await receiptDrop.count() > 0) {
  163 |     await receiptDrop.locator('input[type="file"]').setInputFiles(imgPath);
  164 |   }
  165 | 
  166 |   // Accept all terms explicitly
  167 |   await page.locator('label', { hasText: /I have read and understood/i }).locator('input').check();
  168 |   await page.locator('label', { hasText: /I explicitly consent/i }).locator('input').check();
  169 |   await page.locator('label', { hasText: /I solemnly affirm/i }).locator('input').check();
  170 | 
  171 |   // Finalize Registry
  172 |   await page.locator('button[type="submit"]:has-text("Finalize Registry")').click();
  173 |   
  174 |   // Wait for success states: OTP screen, success message, or redirect to login
  175 |   await Promise.race([
  176 |     page.waitForSelector('text=Security Authentication', { timeout: 60000 }),
  177 |     page.waitForSelector('text=submitted successfully', { timeout: 60000 }),
  178 |     page.waitForURL(/.*login/, { timeout: 60000 })
  179 |   ]);
  180 |   console.log('✅ Registration Submitted.');
  181 | 
  182 |   // ── STEP 2: Admin Approval ────────────────────────────────────────────────
  183 |   console.log('--- Step 2: Admin Approval ---');
  184 |   const adminToken = await getAuthToken(request, ADMIN_USER, ADMIN_PASS);
  185 |   await completeRegistrationPayment(request, adminToken, NEW_EMAIL);
  186 |   await loginAs(page, ADMIN_USER, ADMIN_PASS, 'admin');
  187 |   await page.goto('/admin/approvals');
  188 | 
  189 |   const searchInput = page.locator('input[placeholder*="Search"]').first();
  190 |   if (await searchInput.isVisible()) {
  191 |     await searchInput.fill(NEW_EMAIL);
  192 |     await page.waitForSelector(`tr:has-text("${NEW_EMAIL}")`, { timeout: 10000 });
  193 |   }
  194 | 
  195 |   const pendingRow = page.locator('tr', { hasText: NEW_EMAIL }).first();
  196 |   await pendingRow.locator('button:has-text("Direct Verify")').click();
  197 | 
  198 |   await expect(pendingRow).not.toBeVisible({ timeout: 15000 });
  199 |   console.log('✅ Member Approved.');
  200 | 
  201 |   // ── STEP 3: Admin Event Creation ──────────────────────────────────────────
  202 |   console.log('--- Step 3: Admin Event Creation ---');
  203 |   await page.goto('/admin/events');
  204 |   await page.locator('button:has-text("New Event")').click();
  205 |   
  206 |   await page.locator('input[name="title"]').fill(EVENT_TITLE);
  207 |   await page.locator('textarea[name="description"]').fill('E2E Event Description');
  208 |   await page.locator('input[name="eventDate"]').fill('30-06-2026');
  209 |   await page.locator('input[name="registrationDeadline"]').fill('25-06-2026');
  210 |   await page.locator('input[name="location"]').fill('Munshiganj');
  211 |   await page.locator('input[name="fee"]').fill('500');
  212 |   await page.locator('select[name="category"]').selectOption('Reunion');
  213 |   await page.locator('select[name="status"]').selectOption('Published');
  214 |   
  215 |   await page.locator('input[type="file"]').first().setInputFiles(imgPath);
  216 |   await page.locator('button:has-text("Save Event")').click();
  217 |   await page.waitForSelector(`text=${EVENT_TITLE}`, { timeout: 15000 });
  218 |   console.log('✅ Event Created.');
  219 | 
  220 |   // ── STEP 4: Member Portal Interactions ───────────────────────────────────
  221 |   console.log('--- Step 4: Member Journey ---');
  222 |   await logout(page);
  223 |   // Log in as the new member (NID as password by default)
  224 |   await loginAs(page, NEW_MOBILE, NEW_NID, 'member');
  225 |   
  226 |   // Update Profile
  227 |   await page.goto('/portal/profile');
```