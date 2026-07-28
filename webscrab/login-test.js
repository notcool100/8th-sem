const { chromium } = require('playwright');

const MAIL_URL = 'http://202.166.217.97/mail/';
const USERNAME = 'events';
const PASSWORD = 'T0ur!sm#1998';

async function run() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();

  try {
    console.log(`Navigating to ${MAIL_URL} ...`);
    await page.goto(MAIL_URL, { waitUntil: 'domcontentloaded' });

    await page.fill('#rcmloginuser', USERNAME);
    await page.fill('#rcmloginpwd', PASSWORD);

    console.log('Submitting login form...');
    await Promise.all([
      page.waitForLoadState('networkidle'),
      page.click('#rcmloginsubmit'),
    ]);

    const loginError = await page.locator('.error, .alert-danger').first();
    const hasError = await loginError.count() > 0;

    const url = page.url();
    const loggedIn = url.includes('_task=mail') || (await page.locator('#mailboxlist, .mailbox').count()) > 0;

    if (loggedIn && !hasError) {
      console.log('Login SUCCESS — inbox loaded at', url);
      await page.screenshot({ path: 'login-success.png' });
    } else {
      console.log('Login FAILED — current URL:', url);
      await page.screenshot({ path: 'login-failed.png' });
      process.exitCode = 1;
    }
  } catch (err) {
    console.error('Error during login test:', err);
    process.exitCode = 1;
  } finally {
    await browser.close();
  }
}

run();
