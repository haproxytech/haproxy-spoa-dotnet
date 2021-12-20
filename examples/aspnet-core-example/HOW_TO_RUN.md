This project is an ASP.NET Core web app, which was created with the command:

```
dotnet new web
```

Build an run the Docker Compose file:

```
docker-compose build
docker-compose up -d
```

Go to http://localhost:8000 to see the web app.

Watch the agent's log:

```
docker-compose logs -f agent
```

You can also go to the Agent's HTTP status page at http://localhost:5000.