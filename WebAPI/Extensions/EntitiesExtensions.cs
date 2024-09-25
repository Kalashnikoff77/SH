using Common.Dto.Requests;
using Common.Models;
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
                var guid = Guid.NewGuid();
                Directory.CreateDirectory($"{StaticData.AccountsPhotosDir}/{accountsEntity.Id}/{guid}");

                foreach (var image in StaticData.Images)
                {
                    var fileName = $"{StaticData.AccountsPhotosDir}/{accountsEntity.Id}/{guid}/{image.Key}.jpg";

                    using (MemoryStream output = new MemoryStream(500000))
                    {
                        MagicImageProcessor.ProcessImage($"{StaticData.AccountsPhotosTempDir}/{request.OriginalPhoto}", output, image.Value);
                        await File.WriteAllBytesAsync(fileName, output.ToArray());
                    }
                }

                var sql = "INSERT INTO PhotosForAccounts " +
                    $"({nameof(PhotosForAccountsEntity.Comment)}, {nameof(PhotosForAccountsEntity.Guid)}, {nameof(PhotosForAccountsEntity.IsAvatar)}, {nameof(PhotosForAccountsEntity.AccountId)}) " +
                    "VALUES " +
                    $"(@{nameof(PhotosForAccountsEntity.Comment)}, @{nameof(PhotosForAccountsEntity.Guid)}, @{nameof(PhotosForAccountsEntity.IsAvatar)}, @{nameof(PhotosForAccountsEntity.AccountId)})";
                await conn.ExecuteAsync(sql, new { Comment = accountsEntity.Name, Guid = guid, IsAvatar = true, AccountId = accountsEntity.Id});

                File.Delete($"{StaticData.AccountsPhotosTempDir}/{request.OriginalPhoto}");
            }
        }

    }
}
