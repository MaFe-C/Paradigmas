## Purpose

Defines the HTTP behavior of the Library Service API: CRUD operations for libraries and the books they contain, plus JWT authentication, which the integration tests validate.

## ADDED Requirements

### Requirement: Authenticate a user
The system SHALL expose `POST /login` that accepts user credentials and returns a JWT token when the credentials are valid, and `401 Unauthorized` when they are not. The login endpoint SHALL be publicly accessible.

#### Scenario: Valid credentials
- **WHEN** a request is made to `POST /login` with valid credentials
- **THEN** the response is `200 OK` and contains a JWT token

#### Scenario: Invalid credentials
- **WHEN** a request is made to `POST /login` with invalid credentials
- **THEN** the response is `401 Unauthorized`

### Requirement: Library CRUD
The system SHALL expose CRUD operations for libraries at `api/libraries`. Deleting a library SHALL remove its books as well.

#### Scenario: List libraries
- **WHEN** a request is made to `GET api/libraries`
- **THEN** the response is `200 OK` with an array of libraries

#### Scenario: Get library by id
- **WHEN** a request is made to `GET api/libraries/{libraryId}` for an existing library
- **THEN** the response is `200 OK` with the library
- **AND WHEN** the library does not exist
- **THEN** the response is `404 Not Found`

#### Scenario: Add a library
- **WHEN** a request is made to `POST api/libraries` with a valid library body
- **THEN** the response is `200 OK` and contains the created library

#### Scenario: Update a library
- **WHEN** a request is made to `PUT api/libraries/{libraryId}` for an existing library
- **THEN** the response is `204 No Content` and the library is updated
- **AND WHEN** the library does not exist
- **THEN** the response is `404 Not Found`

#### Scenario: Delete a library
- **WHEN** a request is made to `DELETE api/libraries/{libraryId}` for an existing library
- **THEN** the response is `204 No Content` and the library, along with its books, is removed
- **AND WHEN** the library does not exist
- **THEN** the response is `404 Not Found`

### Requirement: Book CRUD scoped to a library
The system SHALL expose CRUD operations for books at `api/libraries/{libraryId}/books`. All book endpoints SHALL require a valid JWT token.

#### Scenario: List books in an existing library
- **WHEN** an authenticated request is made to `GET api/libraries/{libraryId}/books` for an existing library
- **THEN** the response is `200 OK` with an array of the library's books

#### Scenario: List books in a missing library
- **WHEN** an authenticated request is made to `GET api/libraries/{libraryId}/books` for a library that does not exist
- **THEN** the response is `404 Not Found`

#### Scenario: Add a book to an existing library
- **WHEN** an authenticated request is made to `POST api/libraries/{libraryId}/books` with a valid book body and the library exists
- **THEN** the response is `201 Created` and contains the created book

#### Scenario: Add a book to a missing library
- **WHEN** an authenticated request is made to `POST api/libraries/{libraryId}/books` and the library does not exist
- **THEN** the response is `404 Not Found`

#### Scenario: Update a book in an existing library
- **WHEN** an authenticated request is made to `PUT api/libraries/{libraryId}/books/{bookId}` for an existing book
- **THEN** the response is `204 No Content` and the book is updated
- **AND WHEN** the book or library does not exist
- **THEN** the response is `404 Not Found`

#### Scenario: Delete a book from a library
- **WHEN** an authenticated request is made to `DELETE api/libraries/{libraryId}/books/{bookId}` for an existing book
- **THEN** the response is `204 No Content` and the book is removed
- **AND WHEN** the book or library does not exist
- **THEN** the response is `404 Not Found`

#### Scenario: Request books without a token
- **WHEN** a request is made to any book endpoint without a valid JWT token
- **THEN** the response is `401 Unauthorized`
