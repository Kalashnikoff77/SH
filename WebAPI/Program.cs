using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using WebAPI.Filters;
using WebAPI.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

//builder.Services.AddDbContext<SwContext>(options => { options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")); });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // указывает, будет ли валидироваться издатель при валидации токена
            ValidIssuer = builder.Configuration.GetValue<string>("JwtValidIssuer"), // строка, представляющая издателя
            ValidateAudience = true, // будет ли валидироваться потребитель токена
            ValidAudience = builder.Configuration.GetValue<string>("JwtValidAudience"), // установка потребителя токена
            ValidateLifetime = true, // будет ли валидироваться время существования
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("IssuerSigningKey")!)), // установка ключа безопасности
            ValidateIssuerSigningKey = true // валидация ключа безопасности
        };
    });

builder.Services.AddAutoMapper((cfg) => 
{
    cfg.AllowNullCollections = true;
}, typeof(Mapping));

// Отключение автоматической валидации входящих параметров в WebAPI. Автовалидация возвращает немеделнный ответ 400 - Bad Request.
builder.Services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

builder.Services.AddMvc(options =>
{
    //options.Filters.Add(typeof(ApiValidateModelAttribute));
    options.Filters.Add<CustomExceptionFilter>();
});

//builder.Services.AddDbContext<SwContext>(options =>
//{
//    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
//});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
