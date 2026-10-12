import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    exclude: ['tests/e2e/**', 'tests/visual/**', 'node_modules/**'],
    coverage: {
      provider: 'v8',
      // 82.53b: 'html' gives the same browsable, per-file drill-down report the
      // .NET coverage job already publishes (ReportGenerator's Html output).
      reporter: ['text-summary', 'json-summary', 'lcov', 'html'],
      reportsDirectory: './coverage',
      include: ['src/app/**/*.ts'],
      exclude: ['**/*.spec.ts', '**/*.test.ts'],
    },
  },
  plugins: [
    {
      name: 'angular-resource-stripper',
      transform(code, id) {
        if (id.endsWith('.ts')) {
          return {
            code: code
              .replace(/templateUrl\s*:\s*['"](.*?)['"]/g, (_m, file) =>
                `template: ${JSON.stringify(readFileSync(resolve(dirname(id), file), 'utf8'))}`)
              .replace(/styleUrl\s*:\s*['"].*?['"]/g, 'styles: []')
              .replace(/styleUrls\s*:\s*\[.*?\]/g, 'styles: []')
          };
        }
      }
    }
  ]
});
