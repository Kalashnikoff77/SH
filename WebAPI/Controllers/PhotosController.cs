using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
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
                $"FROM AccountsPhotosView WHERE {nameof(PhotosForAccountsViewEntity.AccountId)} = @accountId " +
                "ORDER BY Id DESC";
            var result = await _unitOfWork.SqlConnection.QueryAsync<PhotosForAccountsViewEntity>(sql, new { accountId, request.Take });
            response.Photos = _mapper.Map<List<PhotosForAccountsViewDto>>(result);

            return response;
        }


        [Route("Update"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> UpdateAsync(UpdatePhotoRequestDto request)
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
                    sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsAvatar)} = 0 WHERE {nameof(PhotosForAccountsEntity.AccountId)} = @AccountId";
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
        /// Загрузка в базу и каталог фото, которые в Блейзоре были помещены в каталог temp
        /// </summary>
        [Route("UploadFromTemp"), HttpPost, Authorize]
        public async Task<UploadPhotoFromTempResponseDto> UploadFromTempAsync(UploadPhotoFromTempRequestDto request)
        {
            AuthenticateUser();

            var response = new UploadPhotoFromTempResponseDto();

            if (!string.IsNullOrWhiteSpace(request.PhotosTempFileNames))
            {
                var guid = Guid.NewGuid();
                Directory.CreateDirectory($"{StaticData.AccountsPhotosDir}/{_unitOfWork.AccountId}/{guid}");

                foreach (var image in StaticData.Images)
                {
                    var fileName = $"{StaticData.AccountsPhotosDir}/{_unitOfWork.AccountId}/{guid}/{image.Key}.jpg";

                    using (MemoryStream output = new MemoryStream(500000))
                    {
                        MagicImageProcessor.ProcessImage($"{StaticData.AccountsPhotosTempDir}/{request.PhotosTempFileNames}", output, image.Value);
                        await System.IO.File.WriteAllBytesAsync(fileName, output.ToArray());
                    }
                }

                var sql = "INSERT INTO PhotosForAccounts " +
                    $"({nameof(PhotosForAccountsEntity.Guid)}, {nameof(PhotosForAccountsEntity.AccountId)}) " +
                    "VALUES " +
                    $"(@{nameof(PhotosForAccountsEntity.Guid)}, @{nameof(PhotosForAccountsEntity.AccountId)});" +
                    $"SELECT CAST(SCOPE_IDENTITY() AS INT)";
                var newId = await _unitOfWork.SqlConnection.QuerySingleAsync<int>(sql, new { Guid = guid, _unitOfWork.AccountId });

                response.NewPhoto = new PhotosForAccountsDto
                {
                    Id = newId,
                    Guid = guid
                };
            }

            System.IO.File.Delete($"{StaticData.AccountsPhotosTempDir}/{request.PhotosTempFileNames}");

            return response;
        }
    }
}
