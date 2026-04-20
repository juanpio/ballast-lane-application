describe("Authentication Critical Flow", () => {
  const password = "SecurePass1!";

  function uniqueEmail() {
    return `e2e_user_${Date.now()}_${Cypress._.random(1000, 9999)}@example.com`;
  }

  it("registers, logs in, accesses dashboard, and logs out safely", () => {
    const email = uniqueEmail();

    // Register
    cy.visit("/account/create");
    cy.contains("h1", "Create Account").should("be.visible");

    cy.get("#Email").type(email);
    cy.get("#Password").type(password);
    cy.get("#ConfirmPassword").type(password);
    cy.get("form").contains("button", "Create Account").click();

    // Current behavior redirects to login after successful registration.
    cy.location("pathname").should("eq", "/account/login");
    cy.contains("h1", "Login").should("be.visible");

    // Login
    cy.get("#Email").clear().type(email);
    cy.get("#Password").clear().type(password);
    cy.get("form").contains("button", "Login").click();

    // Authenticated dashboard
    cy.location("pathname").should("eq", "/dashboard");
    cy.contains("h1", "Dashboard").should("be.visible");
    cy.getCookie("bla.session").should("exist");

    // Accessibility smoke check on critical authenticated page.
    cy.injectAxe();
    cy.checkA11y();

    // Logout
    cy.get("form").contains("button", "Logout").click();
    cy.location("pathname").should("eq", "/account/login");

    // Must not access dashboard after logout.
    cy.visit("/dashboard");
    cy.location("pathname").should("eq", "/account/login");
  });

  it("shows a generic error for invalid credentials", () => {
    cy.visit("/account/login");

    cy.get("#Email").type("unknown@example.com");
    cy.get("#Password").type("WrongPass9!");
    cy.get("form").contains("button", "Login").click();

    cy.contains("Invalid email or password.").should("be.visible");
    cy.location("pathname").should("eq", "/account/login");
  });
});
