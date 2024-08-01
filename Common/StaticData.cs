using Common.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhotoSauce.MagicScaler;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Common
{
    public static class StaticData
    {
        // CropScaleMode.Max - размер фото не увеличивать (фото может быть меньше указанных размеров). При необходимости урезать высоту или ширину
        // CropScaleMode.Crop - размер фото увеличить до указанных размеров. При необходимости урезать высоту или ширину

        /// <summary>
        /// Настройки размеров изображений
        /// </summary>
        public static Dictionary<EnumImageSize, ProcessImageSettings> Images = new ()
        {
            {
                EnumImageSize.s64x64,
                new ProcessImageSettings { Width=64, Height=64, ResizeMode=CropScaleMode.Crop }
            },
            {
                EnumImageSize.s150x150,
                new ProcessImageSettings { Width=150, Height=150, ResizeMode=CropScaleMode.Crop }
            },
            {
                EnumImageSize.s600x450,
                new ProcessImageSettings { Width=600, Height=450, ResizeMode=CropScaleMode.Max, MatteColor = System.Drawing.Color.Black }
            },
            {
                EnumImageSize.s768x1024,
                new ProcessImageSettings { Width=768, Height=1024, ResizeMode=CropScaleMode.Max, MatteColor = System.Drawing.Color.Black }
            }
        };

        public class Gender
        {
            public string ShortName { get; set; } = null!;
            public string Name { get; set; } = null!;
        }

        public static Dictionary<int, Gender> Genders = new ()
        {
            { 0, new Gender { ShortName = "М", Name = "Муж" } },
            { 1, new Gender { ShortName = "Ж", Name = "Жен" } }
        };


        /// <summary>
        /// Кол-во сообщений в обсуждениях мероприятий в блоке
        /// </summary>
        public const short EVENT_DISCUSSIONS_PER_BLOCK = 5;

        /// <summary>
        /// Кол-во сообщений в чате
        /// </summary>
        public const short MESSAGES_PER_BLOCK = 5;


        // Константы полей БД
        public const short DB_ACCOUNTS_EMAIL_MIN = 5;
        public const short DB_ACCOUNTS_EMAIL_MAX = 75;
        public const short DB_ACCOUNTS_PASSWORD_MIN = 4;
        public const short DB_ACCOUNTS_PASSWORD_MAX = 35;
        public const short DB_ACCOUNTS_NAME_MIN = 3;
        public const short DB_ACCOUNTS_NAME_MAX = 40;
        public const short DB_USERS_NAME_MIN = 3;
        public const short DB_USERS_NAME_MAX = 40;
        public const short DB_USERS_ABOUT_MAX = 255;


        /// <summary>
        /// Генерация токена JWT
        /// </summary>
        /// <param name="Id">Id пользователя</param>
        /// <param name="Guid">Guid пользователя</param>
        /// <param name="configuration">Ссылка на IConfiguration</param>
        /// <returns>JWT токен</returns>
        public static string GenerateToken(int Id, Guid Guid, IConfiguration configuration)
        {
            var claims = new List<Claim>()
            {
                new Claim("Id", Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                new Claim("Guid", Guid.ToString()),
                new Claim("IssueDate", DateTime.Now.ToString())
            };
            var jwt = new JwtSecurityToken(
                issuer: configuration.GetRequiredSection("JWT:JwtValidIssuer").Value,
                audience: configuration.GetRequiredSection("JWT:JwtValidAudience").Value,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(180)),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetRequiredSection("JWT:IssuerSigningKey").Value!)), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }


        public static IServiceCollection AddJwtToken(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // указывает, будет ли валидироваться издатель при валидации токена
                        ValidIssuer = builder.Configuration.GetRequiredSection("JWT:JwtValidIssuer").Value, // строка, представляющая издателя
                        ValidateAudience = true, // будет ли валидироваться потребитель токена
                        ValidAudience = builder.Configuration.GetRequiredSection("JWT:JwtValidAudience").Value, // установка потребителя токена
                        ValidateLifetime = true, // будет ли валидироваться время существования
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetRequiredSection("JWT:IssuerSigningKey").Value!)), // установка ключа безопасности
                        ValidateIssuerSigningKey = true // валидация ключа безопасности
                    };
                });
            return services;
        }
    }
}
