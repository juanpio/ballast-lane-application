import { defineConfig } from "cypress";

export default defineConfig({
  e2e: {
    baseUrl: "http://localhost:5178",
    specPattern: "e2e/**/*.cy.ts",
    supportFile: "support/e2e.ts",
    video: false,
    screenshotOnRunFailure: true,
    retries: {
      runMode: 1,
      openMode: 0,
    },
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
  },
});
