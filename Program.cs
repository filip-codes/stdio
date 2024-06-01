using Studio;

Application app = new("http://localhost:8080/");

app.Get("/", "Studio.Controllers.WelcomeController", "Index");

app.Run();
