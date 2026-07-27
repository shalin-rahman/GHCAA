export const environment = {
  production: true,
  // API and SPA are served from the same Render origin (multi-stage Docker build),
  // so call the API relatively — works at any host/domain Render assigns.
  apiUrl: '/api'
};
