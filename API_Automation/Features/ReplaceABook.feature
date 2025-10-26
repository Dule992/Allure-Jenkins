@API
Feature: Replace a book via Bookstore API

  Scenario: Verify that a user can replace a book
    Given A user is created and authorized
    When I get all books
    And I add the first book to user's list
    Then User has only one book and it matches the added one
    When I replace the book with the second one
    Then The user's book list contains only the replaced book
