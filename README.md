# Issue Tracker

A simple and effective tool to track and manage issues in your projects

![project-details](/documentation/screenshots/project-details.png)

## Table of Contents
- [Getting Started](#getting-started)
- [Development](#development)
- [Architecture](#architecture)
- [Screenshots](#screenshots)
- [License](#license)


## Getting Started

1. Use your terminal or command prompt to clone the repo onto your machine

```
git clone https://github.com/williamang171/issue-tracker.git
```
2. Change into the directory
```
cd issue-tracker
```
3. Download docker and docker desktop, you can install these tools and review installation instructions for your Operating System [here](https://docs.docker.com/desktop/)

4. (Optional) If you want to upload attachments, create `src/ProjectIssueService/.env` with the following code and replace `cloudinary_url` with the actual URL, you can refer to [here](https://cloudinary.com/documentation/dotnet_quickstart) for instructions
```
CLOUDINARY_URL=cloudinary_url
```

5. Build the services locally on your computer by running (NOTE: this may take several minutes to complete):
```
docker compose build
```
6. Once this completes you can use the following to run the services:
```
docker compose up -d
```
7. You should now be able to browse to the app on http://localhost:3000

8. A few demo users have been added to the application, you can login with the following credentials

| Username   | Password | Role   |
|------------|----------|--------|
| root       | Pass123$ | Admin  |
| demoadmin  | Pass123$ | Admin  |
| alice      | Pass123$ | Admin  |
| bob        | Pass123$ | Member |
| demomember | Pass123$ | Member |
| demoviewer | Pass123$ | Viewer |

## Development

If you would like to make adjustments to this application, you can follow the instructions below

1. Complete step 1 to step 4 of docker compose section [here](#getting-started)

2. Have [.NET](https://dotnet.microsoft.com/en-us/download) installed

3. Have [Node.js](https://nodejs.org/en) installed

4. Stop existing containers

```
docker compose stop
```

5. In `docker-compose.yml`, comment out all services except `postgres`, `rabbitmq`, and `redis`

6. Restart docker containers with limited services

```
docker compose up -d
```

7. Run the frontend application

```
cd frontend/web-app
npm install
npm run dev
```

8. Build the Contracts project

```
cd src/Contracts
dotnet build
```

9. Run the backend projects `GatewayService`, `IdentityService`, `IssueStatsService`, `ProjectIssueService`, `UserService`

- Running all the services at once (for VSCode)
    - Run `Show all commands`
        - `CTRL` + `Shift` + `P` for Windows / Linux
        - `CMD` + `Shift` + `P` for MacOS
    - `Tasks:Run Task` -> `dotnet run all services`

- Running service individually (for VSCode)
    - Run `Show all commands`
    - `Tasks:Run Task` -> `dotnet run <service-name>`

- Running service individually (command prompt / terminal)
    - `cd src/<ProjectName>`
    - `dotnet run --profile http`

> [!IMPORTANT]
> Note that after updating the code of the service it is required to restart it


## Architecture

![architecture](/documentation/architecture/architecture.drawio.png)

## Screenshots

You can go to [here](documentation/screenshots/README.md) to view more screenshots

## License

Distributed under the MIT license. See `LICENSE` for more information.