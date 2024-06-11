# Fakebook API

Fakebook is a social media clone API project built with ASP.NET. It allows users to create accounts, make posts, follow other users, and join groups. This project is currently a work in progress.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [API Endpoints](#api-endpoints)
- [License](#license)
- [Contact](#contact)

## Features

- User Authentication and Authorization
- User Profiles
- Posting (Create, Read, Update, Delete)
- Follow/Unfollow Users
- Groups (Create, Join, Leave)
- Real-time Notifications (Planned)
- Direct Messaging (Planned)
- User Feed (Planned)

## Technologies Used

- ASP.NET Core
- Entity Framework Core
- SQL Server
- Swagger for API documentation
- SignalR for real-time features (planned)
- Identity for user authentication

### Prerequisites

- .NET SDK
- SQL Server

## API Endpoints

Here are some of the key API endpoints:

### User Endpoints
- `POST /api/users/register` - Register a new user
- `POST /api/users/login` - Login a user
- `GET /api/users/{id}` - Get user profile

### Post Endpoints
- `POST /api/posts` - Create a new post
- `GET /api/posts` - Get all posts
- `GET /api/posts/{id}` - Get a single post
- `PUT /api/posts/{id}` - Update a post
- `DELETE /api/posts/{id}` - Delete a post

### Follow Endpoints
- `POST /api/follow/{id}` - Follow a user
- `DELETE /api/unfollow/{id}` - Unfollow a user

### Group Endpoints
- `POST /api/groups` - Create a new group
- `GET /api/groups` - Get all groups
- `GET /api/groups/{id}` - Get a single group
- `POST /api/groups/{id}/join` - Join a group
- `POST /api/groups/{id}/leave` - Leave a group

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact

Your Name - [jordanrowland00@gmail.com](mailto:jordanrowland00@gmail.com)

Project Link: 
[https://github.com/Jordan-Rowland/fakebook-csharp](https://github.com/Jordan-Rowland/fakebook-csharp)
