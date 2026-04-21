@integration
@docker
Feature: Orders API authorization and ownership
  The orders API should enforce JWT authorization and customer data isolation.

  Background:
    Given the dockerized application is running

  Scenario: Anonymous users cannot list orders
    When I GET "/api/orders?page=1&pageSize=10" without a JWT token
    Then the HTTP status code should be 401

  Scenario: Authenticated users only receive their own active orders
    Given a seeded customer "user1" with password "Password1!"
    And a seeded customer "user2" with password "Password1!"
    And customer "user1" has an active order "IT-U1-1"
    And customer "user1" has an active order "IT-U1-2"
    And customer "user1" has a soft deleted order "IT-U1-DELETED"
    And customer "user2" has an active order "IT-U2-1"
    When I authenticate through the API as "user1"
    And I GET "/api/orders?page=1&pageSize=10" as customer "user1"
    Then the HTTP status code should be 200
    And the orders payload should only contain customer "user1" orders
    And the orders payload should contain ids "IT-U1-1, IT-U1-2"
    And the orders payload should not contain ids "IT-U2-1, IT-U1-DELETED"

  Scenario: Users cannot get another customer's order
    Given a seeded customer "user1" with password "Password1!"
    And a seeded customer "user2" with password "Password1!"
    And customer "user2" has an active order "IT-U2-1"
    When I authenticate through the API as "user1"
    And I GET "/api/orders/IT-U2-1" as customer "user1"
    Then the HTTP status code should be 404

  Scenario: Users can get their own order by id
    Given a seeded customer "user1" with password "Password1!"
    And customer "user1" has an active order "IT-U1-1"
    When I authenticate through the API as "user1"
    And I GET "/api/orders/IT-U1-1" as customer "user1"
    Then the HTTP status code should be 200
    And the order payload id should be "IT-U1-1"
    And the order payload customer should be "user1"
