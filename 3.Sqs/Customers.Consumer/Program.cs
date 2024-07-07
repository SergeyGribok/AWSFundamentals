using Amazon.SQS;
using Customers.Consumer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<QueueSettings>(builder.Configuration.GetSection(QueueSettings.Key));
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddHostedService<QueueConsumerService>();
builder.Services.AddMediatR(opt => opt.RegisterServicesFromAssembly(typeof(Program).Assembly));


var app = builder.Build();

app.Run();
