using Common.Dto.Requests;
using Common.Models;
using Dapper;
using DataContext.Entities;
using PhotoSauce.MagicScaler;
using WebAPI.Models;

namespace WebAPI.Extensions
{
    public static class EntitiesExtensions
    {
        /// <summary>
        /// Обработка фото при регистрации аккаунта
        /// </summary>
        public static async Task ProcessPhotoAfterRegistration(this AccountsEntity accountsEntity, UnitOfWork unitOfWork, AccountRegisterRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.OriginalPhoto))
            {
                var guid = Guid.NewGuid();
                Directory.CreateDirectory($"{StaticData.AccountsPhotosDir}/{accountsEntity.Id}/{guid}");

                foreach (var image in StaticData.Images)
                {
                    var fileName = $"{StaticData.AccountsPhotosDir}/{accountsEntity.Id}/{guid}/{image.Key}.jpg";

                    using (MemoryStream output = new MemoryStream(300000))
                    {
                        MagicImageProcessor.ProcessImage($"{StaticData.TempPhotosDir}/{request.OriginalPhoto}", output, image.Value);
                        await File.WriteAllBytesAsync(fileName, output.ToArray());
                    }
                }

                var sql = "INSERT INTO PhotosForAccounts " +
                    $"({nameof(PhotosForAccountsEntity.Comment)}, {nameof(PhotosForAccountsEntity.Guid)}, {nameof(PhotosForAccountsEntity.IsAvatar)}, {nameof(PhotosForAccountsEntity.RelatedId)}) " +
                    "VALUES " +
                    $"(@{nameof(PhotosForAccountsEntity.Comment)}, @{nameof(PhotosForAccountsEntity.Guid)}, @{nameof(PhotosForAccountsEntity.IsAvatar)}, @{nameof(PhotosForAccountsEntity.RelatedId)})";
                await unitOfWork.SqlConnection.ExecuteAsync(sql, new { Comment = accountsEntity.Name, Guid = guid, IsAvatar = true, AccountId = accountsEntity.Id});

                File.Delete($"{StaticData.TempPhotosDir}/{request.OriginalPhoto}");
            }
        }

    }
}
