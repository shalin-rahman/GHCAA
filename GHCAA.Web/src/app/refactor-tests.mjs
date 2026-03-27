import fs from 'fs';
import path from 'path';

function findTestFiles(dir, files = []) {
  const list = fs.readdirSync(dir);
  for (const file of list) {
    const fullPath = path.join(dir, file);
    const stat = fs.statSync(fullPath);
    if (stat.isDirectory() && file !== 'node_modules') {
      findTestFiles(fullPath, files);
    } else if (file.endsWith('.spec.ts')) {
      files.push(fullPath);
    }
  }
  return files;
}

const rootDir = process.cwd();
const testFiles = findTestFiles(rootDir);

let modifiedCount = 0;

for (const filePath of testFiles) {
  let content = fs.readFileSync(filePath, 'utf8');
  let changed = false;

  // Add the import if needed and if AuthService is being mocked
  if (content.includes('AuthService') && content.includes('authServiceMock')) {
    if (!content.includes('createAuthServiceMock')) {
      const importLine = `import { createAuthServiceMock } from '${getRelativeImportPath(filePath, 'core/testing/testing-utils')}';\n`;
      // insert at top
      content = importLine + content;
      changed = true;
    }

    // Replace manually built authServiceMock = { ... } with authServiceMock = createAuthServiceMock();
    const regex = /authServiceMock\s*=\s*\{[\s\S]*?(?=\r?\n\s+(?:routerMock|notificationServiceMock|TestBed|await TestBed|var|let|const|\}))\}/m;
    if (regex.test(content) && !content.includes('authServiceMock = createAuthServiceMock()')) {
        content = content.replace(regex, 'authServiceMock = createAuthServiceMock()');
        changed = true;
    }
  }

  // Support for mockFinancialService across modules?
  // Let's stick strictly to AuthService and NotificationService to not break the build.
  
  if (changed) {
    fs.writeFileSync(filePath, content, 'utf8');
    modifiedCount++;
    console.log(`Refactored: ${filePath}`);
  }
}

function getRelativeImportPath(fromFile, target) {
  const fromDir = path.dirname(fromFile);
  const targetAbs = path.join(rootDir, target);
  let rel = path.relative(fromDir, targetAbs).replace(/\\/g, '/');
  if (!rel.startsWith('.')) rel = './' + rel;
  return rel;
}

console.log(`Successfully refactored ${modifiedCount} test files.`);
