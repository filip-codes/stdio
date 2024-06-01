using Studio.Foundation;

Application app = new();

app.Get("/", "Studio.Controllers.WelcomeController", "Index");

app.Run();
