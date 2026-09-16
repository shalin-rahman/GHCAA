import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    exclude: ['tests/e2e/**', 'tests/visual/**', 'node_modules/**'],
    coverage: {
      provider: 'v8',
      reporter: ['text-summary', 'json-summary', 'lcov'],
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
              .replace(/templateUrl\s*:\s*['"].*?['"]/g, 'template: ""')
              .replace(/styleUrl\s*:\s*['"].*?['"]/g, 'styles: []')
              .replace(/styleUrls\s*:\s*\[.*?\]/g, 'styles: []')
          };
        }
      }
    }
  ]
});
