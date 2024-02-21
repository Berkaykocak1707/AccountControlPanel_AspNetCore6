
# IMPORTANT NOTE
Before using this project, you must carefully configure the email authentication and other necessary settings in the `appsettings.json` file. The project will not function correctly if you do not enter all the required keys.

# AccountControlPanel - ASP.NET Core 6 Web Application
Account Control Panel is an account control panel application developed using ASP.NET Core 6. This platform is designed with a modern interface and a robust backend to simplify user management and account operations. Users can register via social media (twitter, github and google) if they wish. A small project that can manage user systems in projects.

## Technology Stack
- **Framework**: ASP.NET Core 6
- **Database**: [Specify your database, e.g., SQL Server, MySQL]
- **Other Used Tools**:
  - Entity Framework Core
  - AutoMapper
  - ASP.NET Identity (for authentication and user management)

## Project Structure
- `.vs`: Contains Visual Studio configuration and user-specific settings.
- `Business`: Implements the application's business logic.
- `AccountControlPanel_AspNetCore6`: Main project files for the ASP.NET Core application.
- `DataAccess`: Data access layer managing database interactions.
- `Entities`: Defines the database entities and models.

## Setup and Configuration
To set up the project locally:

1. Clone the repository: `git clone [repository link]`.
2. Navigate to the project directory: `cd AccountControlPanel_AspNetCore6`.
3. Install required packages: `dotnet restore`.
4. Configure the database in `appsettings.json`.
5. Apply database migrations: `dotnet ef database update`.
6. Run the application: `dotnet run`.

## Usage Examples
- **User Registration and Authentication**: Managed through ASP.NET Identity.
- **User Management**: Handled in the Business layer and displayed through the main project.

## Contributing
To contribute to this project:

1. Fork the repository.
2. Create a new branch for your feature.
3. Commit your changes.
4. Push to the branch and open a pull request.
