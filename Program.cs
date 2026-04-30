using System.Reflection;
using Microsoft.OpenApi;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка порта для развертывания на Amvera
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Добавление CORS для доступа с Netlify / локального фронтенда
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Добавление фабрики HTTP-клиентов для внешних запросов
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllers();

// Swagger для тестирования API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "University Admission API", 
        Version = "v1",
        Description = "API для автоматизации приёмной кампании университета",
        Contact = new OpenApiContact
        {
            Name = "Приёмная комиссия",
            Email = "admission@university.ru"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Регистрируем бизнес-логику как Singleton (для сохранения состояния в памяти)
builder.Services.AddSingleton<AdmissionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // Включаем CORS
app.UseStaticFiles();    // Раздача index.html из папки wwwroot

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
