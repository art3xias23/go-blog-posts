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

app.MapGet("/events", async (HttpContext context, ITemplateParser parser) =>
{
    var questions = new List<string>
    {
        "What is your name?",
        "What is your age?",
        "What is your favorite color?"
    };

    var clientOptions = new OpenAIClientOptions
    {
        Retry =
        {
            MaxRetries = 1,
            NetworkTimeout = TimeSpan.FromMinutes(3)
        }
    };
    var azureUrl = "https://transcriptionai.openai.azure.com/";
    var azureKey = "8f60e1d4fe274cc4b7ba57ed14dddec5";
    Uri azureOpenAIResourceUri = new(azureUrl);
    AzureKeyCredential azureOpenAIApiKey = new(azureKey);
    var client = new OpenAIClient(azureOpenAIResourceUri, azureOpenAIApiKey, clientOptions);

    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.Add("Cache-Control", "no-store, no-cache");
    context.Response.Headers.Add("Pragma", "no-cache");
    context.Response.Headers.Add("Expires", "0");

    for (int i = 0; i < questions.Count; i++)
    {
        var questionData = new { questionnumber = i + 1, prompt = questions[i] };
        var firstTemplate = parser.Render(questionData, @"<p>{{ questionnumber }}. {{ prompt }}</p><textarea id='response-{{ questionnumber }}'></textarea>");

        // Send the initial text area template
        await context.Response.WriteAsync($"data: {firstTemplate}\n\n");
        await context.Response.Body.FlushAsync();


        var conversation = new List<ChatRequestMessage>
        {
            new ChatRequestSystemMessage(
                "Answer subsequent questions based on the text. Answer with 5 words each of the questions."),
            new ChatRequestUserMessage("Name is Mike. Age is 37. Color favorite is blue."),
            new ChatRequestUserMessage(questions[i])
        };

        var chatOptions = new ChatCompletionsOptions("APGPT4", conversation)
        {
            MaxTokens = 500,
            Temperature = 0.5f
        };

        var chatCompletionsResponse = await client.GetChatCompletionsStreamingAsync(chatOptions);
        var textareaId = $"response-{i + 1}";

        await foreach (var chatChoice in chatCompletionsResponse.EnumerateValues())
        {
            var contentUpdate = chatChoice.ContentUpdate;
            var script = $"<script>document.getElementById('{textareaId}').value += `{contentUpdate}`;</script>";
            await context.Response.WriteAsync($"data: {script}\n\n");
            await context.Response.Body.FlushAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }
    }

    // Signal the end of the event stream
    await context.Response.WriteAsync("event: end\ndata: Stream closed\n\n");
    await context.Response.Body.FlushAsync();

});

//app.MapGet("/", (ITemplateParser parser, HttpContext context) =>
//{
//    var template = "Templates/button.html";
//    var templateText = parser.RenderFile(new { name = "Tino" }, template);
//    context.Response.ContentType = "text/html";
//    context.Response.WriteAsync(templateText);
//});

app.MapPost("/handle", async (ITemplateParser parser, HttpContext context) =>
{
    var questions = new List<string>
    {
        "What is your name?",
        "What is your age?",
        "What is your favorite color?"
    };

    var clientOptions = new OpenAIClientOptions
    {
        Retry =
            {
                MaxRetries = 1,
                NetworkTimeout = TimeSpan.FromMinutes(3)
            }
    };
    var azureUrl = "https://transcriptionai.openai.azure.com/";
    var azureKey = "8f60e1d4fe274cc4b7ba57ed14dddec5";
    Uri azureOpenAIResourceUri = new(azureUrl);
    AzureKeyCredential azureOpenAIApiKey = new(azureKey);
    var client = new OpenAIClient(azureOpenAIResourceUri, azureOpenAIApiKey, clientOptions);

    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.Add("Cache-Control", "no-store, no-cache");
    context.Response.Headers.Add("Pragma", "no-cache");
    context.Response.Headers.Add("Expires", "0");

    for (int i = 0; i < questions.Count; i++)
    {
        var questionData = new { questionNumber = i + 1, prompt = questions[i] };
        var firstTemplate = parser.RenderFile(questionData, "Templates/textarea.html");

        // Send question template
        await context.Response.WriteAsync($"data: {firstTemplate}\n\n");
        await context.Response.Body.FlushAsync();

        var conversation = new List<ChatRequestMessage>()
        {
            new ChatRequestSystemMessage("Answer subsequent questions based on the text. Answer with 5 words each of the questions."),
            new ChatRequestUserMessage("Name is Mike. Age is 37. Color favorite is blue."),
            new ChatRequestUserMessage(questions[i])
        };

        var chatOptions = new ChatCompletionsOptions("APGPT4", conversation)
        {
            MaxTokens = 500,
            Temperature = 0.5f
        };

        CancellationTokenSource cts = new CancellationTokenSource();
        var chatCompletionsResponse = await client.GetChatCompletionsStreamingAsync(chatOptions, cts.Token);

        var textareaId = $"response-{i + 1}";

        await foreach (var chatChoice in chatCompletionsResponse.EnumerateValues())
        {
            var contentUpdate = chatChoice.ContentUpdate;
            var script = $"<script>document.getElementById('{textareaId}').value += `{contentUpdate}`;</script>";
            await context.Response.WriteAsync($"data: {script}\n\n");
            await context.Response.Body.FlushAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }
    }
});

//async Task ContinueConversationAsync(string question,
//    IList<ChatRequestMessage> conversation)
//{
//    var chatOptions = new ChatCompletionsOptions("APGPT4", conversation)
//    {
//        MaxTokens = 500,
//        Temperature = 0.5f
//    };
//    CancellationTokenSource cts = new CancellationTokenSource();

//    //Task cancellationTask = Task.Run(() =>
//    //{
//    //    Console.WriteLine("Press any key to cancel...");
//    //    Console.ReadKey();
//    //    cts.Cancel();
//    //});
//    chatOptions.Messages.Add(new ChatRequestUserMessage(question));

//    var chatCompletionsResponse = await Client.GetChatCompletionsStreamingAsync(chatOptions, cts.Token);
//    await foreach (var chatChoice in chatCompletionsResponse.EnumerateValues())
//    {

//        Console.Write(chatChoice.ContentUpdate);
//        await Task.Delay(TimeSpan.FromMilliseconds(200));
//    }
//}


//async Task GetChatResponseAsync(string systemMessage, string transcriptMessage, IList<ChatRequestUserMessage> messages)
//{
//    try
//    {

//        var conversation = new List<ChatRequestMessage>();

//        conversation.Add(new ChatRequestSystemMessage(systemMessage));
//        conversation.Add(new ChatRequestUserMessage(transcriptMessage));
//        var count = 1;
//        foreach (var question in messages)
//        {

//            await ContinueConversationAsync(question.Content, conversation);
//            count++;
//        }
//    }
//    catch (Exception e)
//    {
//    }
//}


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

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
