using Studio.Foundation;

Application app = new();

app.Get("/", "Studio.Controllers.WelcomeController", "Index");
app.Get("/about", "Studio.Controllers.AboutController", "Index");
app.Get("/contact", "Studio.Controllers.ContactController", "Index");

app.Run();
