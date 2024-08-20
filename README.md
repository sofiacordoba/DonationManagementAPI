# Donation Management API

# Objective:
The DonationManagementAPI project is designed to manage and facilitate donations and payments within a donation management system. This API provides endpoints for managing donors, pledges, payments, and related entities, allowing for comprehensive tracking and reporting of donation activities.

# Implementation:
The API is implemented using ASP.NET Core and follows RESTful principles. It leverages Entity Framework Core for data access, with a MySQL database to store and manage the data. The project includes CRUD operations for donors, pledges, and payments, as well as endpoints for querying and managing related entities. Additionally, it integrates a changelog system to track changes made to the data. Entity filtering is implemented across all controllers to streamline data retrieval and management.

# Features:
Donor Management: Create, update, retrieve, and delete donor information.
Pledge Management: Create, update, retrieve, and delete pledges. Includes validation to ensure that donors exist before creating pledges.
Payment Management: Create, update, retrieve, and delete payments. Each payment can be associated with multiple pledges.
Changelog Tracking: Automatically log changes made to donors, pledges, and payments for audit and tracking purposes.
Associations: Retrieve payments associated with a specific pledge and pledges associated with a specific payment.
Search Functionality: Filter donors, pledges, and payments by various criteria such as donor ID, amount, and date.
Error Handling: Proper error responses for non-existent records and other issues.

# Technologies Used:
ASP.NET Core 8: Framework for building the web API.
Entity Framework Core: ORM for data access and management.
MySQL: Database for storing project data.
Swagger/OpenAPI: API documentation and testing.
CORS: Configuration for cross-origin requests.
Postman: Designed for testing and interacting with the API; no UI included.
