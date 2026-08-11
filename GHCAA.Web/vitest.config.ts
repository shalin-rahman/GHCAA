import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    exclude: ['tests/e2e/**', 'tests/visual/**', 'node_modules/**'],
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
