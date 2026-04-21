@integration
@docker
Feature: MVC authentication integration
  Dashboard access should follow cookie authentication rules when running against docker compose.

  Background:
    Given the dockerized application is running

  Scenario: Anonymous users are redirected to login from dashboard
    When I GET MVC page "/dashboard" without authentication
    Then the HTTP status code should be 302
    And the response should redirect to "/account/login"

  Scenario: API login establishes MVC authenticated session
    Given a seeded customer "user1" with password "Password1!"
    When I authenticate through the API as "user1"
    And I GET MVC page "/dashboard" with the authenticated session for "user1"
    Then the HTTP status code should be 200
    And the response should include text "Authenticated session active."
