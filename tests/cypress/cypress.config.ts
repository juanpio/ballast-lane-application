import { defineConfig } from "cypress";

export default defineConfig({
  e2e: {
    setupNodeEvents(on) {
      on("task", {
        logViolations(violations: Array<{ id: string; impact: string; description: string; nodes: Array<{ html: string }> }>) {
          violations.forEach((v) => {
            console.error(`[a11y] ${v.id} (${v.impact}): ${v.description}`);
            v.nodes.forEach((n) => console.error(`  -> ${n.html}`));
          });
          return null;
        },
      });
    },
    baseUrl: "http://localhost:8080",
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
