using System.Text;
using Azure;
using Azure.AI.OpenAI;
using blog_2_charp_urlfor;
using Microsoft.AspNetCore.Http.Features;
using Scriban;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITemplateParser, TemplateParser>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("Templates/index.html");
});

app.MapGet("/Example", (ITemplateParser parser) =>
{
    var name = "Victor";
    var template = "<p>My name is {{name}}</p>";
    var templateText = parser.Render(new { name = "Tino" }, template);
    return templateText;

});

app.MapGet("/ExampleUrl", (ITemplateParser parser, HttpContext context) =>
{
    var template = $@"<a href=""/examples/{23}"">Hardcoded url</a>";
    var templateText = parser.Render(new { }, template);
    context.Response.ContentType = "text/html";
    context.Response.WriteAsync(templateText);
});

app.MapGet("/GetUrl", (ITemplateParser parser, HttpContext context) =>
{
    var template = @"<a href=""{{url_for 'GetExamplesUrlFor' 23}}"">The href was generated using our custom url_for</a>";
    var templateText = parser.Render(new { }, template);
    context.Response.ContentType = "text/html";
    context.Response.WriteAsync(templateText);

});

app.MapGet("/examples/{id}", (int id, ITemplateParser parser, HttpContext context) =>
{
    var template = $@"<p>The routing param from the url is {id}</p> <br> <p>End of Demo</p>";
    var templateText = parser.Render(new { }, template);
    context.Response.ContentType = "text/html";
    context.Response.WriteAsync(templateText);
})
    .WithName("GetExamplesUrlFor");

app.Run();
