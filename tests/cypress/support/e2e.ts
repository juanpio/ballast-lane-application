import "cypress-axe";

Cypress.Commands.overwrite(
  "checkA11y",
  (
    original,
    context?: Parameters<typeof cy.checkA11y>[0],
    options?: Parameters<typeof cy.checkA11y>[1],
    violationCallback?: Parameters<typeof cy.checkA11y>[2],
    skipFailures?: boolean
  ) => {
    return original(
      context,
      options,
      (violations) => {
        cy.task("logViolations", violations);
        violationCallback?.(violations);
      },
      skipFailures
    );
  }
);
