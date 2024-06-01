using Studio;

Application app = new();

app.Get("/", "Studio.Controllers.WelcomeController", "Index");

app.Run();
