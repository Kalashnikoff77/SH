using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Enums;
using Common.Models;
using Dapper;
using DataContext.Entities;
using DataContext.Entities.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoSauce.MagicScaler;
using WebAPI.Exceptions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : MyControllerBase
    {
        public PhotosController(IMapper mapper, IConfiguration configuration) : base(mapper, configuration) { }

        [Route("Get"), HttpPost, Authorize]
        public async Task<GetPhotosForAccountsResponseDto> GetAsync(GetPhotosForAccountRequestDto request)
        {
            AuthenticateUser();

            var response = new GetPhotosForAccountsResponseDto();

            var columns = GetRequiredColumns<PhotosForAccountsViewEntity>();

            // Получаем фото текущего пользователя или указанного в request?
            var accountId = request.AccountId == null ? _unitOfWork.AccountId : request.AccountId;

            var sql = "SELECT TOP (@Take) " +
                $"{columns.Aggregate((a, b) => a + ", " + b)} " +
                $"FROM AccountsPhotosView WHERE {nameof(PhotosForAccountsViewEntity.RelatedId)} = @accountId " +
                "ORDER BY Id DESC";
            var result = await _unitOfWork.SqlConnection.QueryAsync<PhotosForAccountsViewEntity>(sql, new { accountId, request.Take });
            response.Photos = _mapper.Map<List<PhotosForAccountsViewDto>>(result);

            return response;
        }


        [Route("UpdatePhotoForAccount"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> UpdatePhotoForAccountAsync(UpdatePhotoForAccountRequestDto request)
        {
            AuthenticateUser();

            var response = new ResponseDtoBase();

            var sql = "SELECT TOP 1 * FROM PhotosForAccounts WHERE AccountId = @AccountId AND Guid = @Guid";
            var photo = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<PhotosForAccountsEntity>(sql, new { _unitOfWork.AccountId, request.Guid }) 
                ?? throw new NotFoundException("Соответствующее фото не найдено!");

            // Смена аватара
            if (request.IsAvatarChanging)
            {
                if (photo.IsAvatar)
                {
                    sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsAvatar)} = 0 WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                    await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { photo.Id });
                }
                else
                {
                    sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsAvatar)} = 0 WHERE {nameof(PhotosForAccountsEntity.RelatedId)} = @AccountId";
                    await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { _unitOfWork.AccountId });
                    sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsAvatar)} = 1 WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                    await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { photo.Id });
                }
            }

            // Смена комментария
            if (request.IsCommentChanging)
            {
                sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.Comment)} = @Comment WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { request.Comment, photo.Id });
            }

            // Удаление фото
            if (request.IsDeleting)
            {
                sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsDeleted)} = 1 WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { photo.Id });
            }

            return response;
        }


        /// <summary>
        /// Загрузка в базу и каталог фото, которое в Блейзоре было помещено в каталог temp
        /// </summary>
        [Route("UploadAccountFromTemp"), HttpPost, Authorize]
        public async Task<UploadAccountPhotoFromTempResponseDto> UploadAccountFromTempAsync(UploadAccountPhotoFromTempRequestDto request)
        {
            AuthenticateUser();

            var response = new UploadAccountPhotoFromTempResponseDto();

            var guid = Guid.NewGuid();
            var tempDir = StaticData.TempPhotosDir;
            var dir = $"{StaticData.AccountsPhotosDir}/{_unitOfWork.AccountId}/{guid}";

            if (!string.IsNullOrWhiteSpace(request.PhotosTempFileNames))
            {
                Directory.CreateDirectory(dir);

                foreach (var image in StaticData.Images)
                {
                    var fileName = $"{dir}/{image.Key}.jpg";

                    using (MemoryStream output = new MemoryStream(300000))
                    {
                        MagicImageProcessor.ProcessImage($"{tempDir}/{request.PhotosTempFileNames}", output, image.Value);
                        await System.IO.File.WriteAllBytesAsync(fileName, output.ToArray());
                    }
                }

                var sql = "INSERT INTO PhotosForAccounts " +
                    $"({nameof(PhotosForAccountsEntity.Guid)}, {nameof(PhotosForAccountsEntity.RelatedId)}) " +
                    "VALUES " +
                    $"(@{nameof(PhotosForAccountsEntity.Guid)}, @{nameof(PhotosForAccountsEntity.RelatedId)});" +
                    $"SELECT CAST(SCOPE_IDENTITY() AS INT)";
                var newId = await _unitOfWork.SqlConnection.QuerySingleAsync<int>(sql, new { Guid = guid, _unitOfWork.AccountId });

                response.NewPhoto = new PhotosForAccountsDto
                {
                    Id = newId,
                    Guid = guid
                };
            }

            System.IO.File.Delete($"{tempDir}/{request.PhotosTempFileNames}");

            return response;
        }


        /// <summary>
        /// Загрузка фото во временный каталог temp: оригинал и аватар 250х250
        /// </summary>
        [Route("UploadPhotoToTemp"), HttpPost, Authorize]
        public async Task<UploadPhotoToTempResponseDto> UploadPhotoToTempAsync(UploadPhotoToTempRequestDto request)
        {
            AuthenticateUser();

            if (request.EventId.HasValue)
            {
                var sql = $"SELECT TOP 1 Id FROM Events WHERE Id = @EventId AND {nameof(EventsEntity.AdminId)} = @{nameof(EventsEntity.AdminId)}";
                var evtId = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.EventId, AdminId = _unitOfWork.AccountId })
                    ?? throw new NotFoundException("Соответствующее мероприятие не найдено!");
            }
            else if (request.AccountId.HasValue)
            {
                var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Id = @AccountId";
                var accId = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { _unitOfWork.AccountId })
                    ?? throw new NotFoundException($"Пользователь с Id {request.AccountId} не найден!");
            }
            else
                throw new BadRequestException("Необходимо указать Id аккаунта или мероприятия!");

            var response = new UploadPhotoToTempResponseDto();

            if (request.File != null)
            {
                var guid = Guid.NewGuid();
                var tempDir = $"{StaticData.TempPhotosDir}/{guid}";

                Directory.CreateDirectory(tempDir);

                // Сохраняем оригинал файла
                using (MemoryStream output = new MemoryStream(request.File))
                    await System.IO.File.WriteAllBytesAsync($"{tempDir}/original.jpg", output.ToArray());

                // Сохраняем временный аватар файла 250x250
                using (MemoryStream output = new MemoryStream(50000))
                {
                    MagicImageProcessor.ProcessImage(new MemoryStream(request.File), output, StaticData.Images[EnumImageSize.s250x250]);
                    await System.IO.File.WriteAllBytesAsync($"{tempDir}/{EnumImageSize.s250x250}.jpg", output.ToArray());
                }

                if (request.AccountId.HasValue)
                    response.NewAccountPhoto = new PhotosForAccountsDto { Guid = guid, CreateDate = DateTime.Now };
                else
                    response.NewEventPhoto = new PhotosForEventsDto { Guid = guid, CreateDate = DateTime.Now };
            }

            return response;
        }
    }
}
