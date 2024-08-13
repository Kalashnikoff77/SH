using Common;
using Common.Dto.Requests;
using Dapper;
using DataContext.Entities;
using Microsoft.Data.SqlClient;
using PhotoSauce.MagicScaler;

namespace WebAPI.Extensions
{
    public static class EntitiesExtensions
    {
        /// <summary>
        /// Обработка фото при регистрации аккаунта
        /// </summary>
        /// <param name="accountsEntity"></param>
        /// <param name="photo"></param>
        public static async Task ProcessPhotoAfterRegistration(this AccountsEntity accountsEntity, SqlConnection conn, AccountRegisterRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.OriginalPhoto))
            {
                var dir = "../UI/wwwroot/images/AccountsPhotos";
                var guid = Guid.NewGuid();
                Directory.CreateDirectory($"{dir}/{accountsEntity.Id}/{guid}");

                foreach (var image in StaticData.Images)
                {
                    var fileName = $"{dir}/{accountsEntity.Id}/{guid}/{image.Key}.jpg";

                    using (MemoryStream output = new MemoryStream(500000))
                    {
                        MagicImageProcessor.ProcessImage($"{dir}/temp/{request.OriginalPhoto}", output, image.Value);
                        await File.WriteAllBytesAsync(fileName, output.ToArray());
                    }
                }

                var sql = "INSERT INTO AccountsPhotos " +
                    $"({nameof(AccountsPhotosEntity.Comment)}, {nameof(AccountsPhotosEntity.Guid)}, {nameof(AccountsPhotosEntity.IsAvatar)}, {nameof(AccountsPhotosEntity.AccountId)}) " +
                    "VALUES " +
                    $"(@{nameof(AccountsPhotosEntity.Comment)}, @{nameof(AccountsPhotosEntity.Guid)}, @{nameof(AccountsPhotosEntity.IsAvatar)}, @{nameof(AccountsPhotosEntity.AccountId)})";
                await conn.ExecuteAsync(sql, new { Comment = accountsEntity.Name, Guid = guid, IsAvatar = true, AccountId = accountsEntity.Id});

                File.Delete($"{dir}/temp/{request.OriginalPhoto}");
            }
        }

    }
}
