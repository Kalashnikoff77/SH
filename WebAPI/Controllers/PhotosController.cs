﻿using AutoMapper;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Dapper;
using DataContext.Entities;
using DataContext.Entities.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

            using (var conn = new SqlConnection(connectionString))
            {
                var columns = GetRequiredColumns<PhotosForAccountsViewEntity>();

                // Получаем фото текущего пользователя или указанного в request?
                var accountId = request.AccountId == null ? _accountId : request.AccountId;

                var sql = "SELECT TOP (@Take) " +
                    $"{columns.Aggregate((a, b) => a + ", " + b)} " +
                    $"FROM AccountsPhotosView WHERE {nameof(PhotosForAccountsViewEntity.AccountId)} = @accountId " +
                    "ORDER BY Id DESC";
                var result = await conn.QueryAsync<PhotosForAccountsViewEntity>(sql, new { accountId, request.Take });

                response.Photos = _mapper.Map<List<PhotosForAccountsViewDto>>(result);
            }
            return response;
        }


        [Route("Update"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> UpdateAsync(UpdatePhotoRequestDto request)
        {
            AuthenticateUser();

            var response = new ResponseDtoBase();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT TOP 1 * FROM PhotosForAccounts WHERE AccountId = @_accountId AND Guid = @Guid";
                var photo = await conn.QueryFirstOrDefaultAsync<PhotosForAccountsEntity>(sql, new { _accountId, request.Guid }) 
                    ?? throw new NotFoundException("Соответствующее фото не найдено!");

                // Смена аватара
                if (request.IsAvatarChanging)
                {
                    if (photo.IsAvatar)
                    {
                        sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsAvatar)} = 0 WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                        await conn.ExecuteAsync(sql, new { photo.Id });
                    }
                    else
                    {
                        sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsAvatar)} = 0 WHERE {nameof(PhotosForAccountsEntity.AccountId)} = @_accountId";
                        await conn.ExecuteAsync(sql, new { _accountId });
                        sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsAvatar)} = 1 WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                        await conn.ExecuteAsync(sql, new { photo.Id });
                    }
                }

                // Смена комментария
                if (request.IsCommentChanging)
                {
                    sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.Comment)} = @Comment WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                    await conn.ExecuteAsync(sql, new { request.Comment, photo.Id });
                }

                // Удаление фото
                if (request.IsDeleting)
                {
                    sql = $"UPDATE PhotosForAccounts SET {nameof(PhotosForAccountsEntity.IsDeleted)} = 1 WHERE {nameof(PhotosForAccountsEntity.Id)} = @Id";
                    await conn.ExecuteAsync(sql, new { photo.Id });
                }

                return response;
            }
        }


        /// <summary>
        /// Загрузка в базу и каталог фото, которые в Блейзоре были помещены в каталог temp
        /// </summary>
        [Route("UploadFromTemp"), HttpPost, Authorize]
        public async Task<UploadPhotoFromTempResponseDto> UploadFromTempAsync(UploadPhotoFromTempRequestDto request)
        {
            AuthenticateUser();

            var response = new UploadPhotoFromTempResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                if (!string.IsNullOrWhiteSpace(request.PhotosTempFileNames))
                {
                    var guid = Guid.NewGuid();
                    Directory.CreateDirectory($"{StaticData.AccountsPhotosDir}/{_accountId}/{guid}");

                    foreach (var image in StaticData.Images)
                    {
                        var fileName = $"{StaticData.AccountsPhotosDir}/{_accountId}/{guid}/{image.Key}.jpg";

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
                    var newId = await conn.QuerySingleAsync<int>(sql, new { Guid = guid, AccountId = _accountId });

                    response.NewPhoto = new Common.Dto.PhotosForAccountsDto
                    {
                        Id = newId,
                        Guid = guid
                    };
                }

                System.IO.File.Delete($"{StaticData.AccountsPhotosTempDir}/{request.PhotosTempFileNames}");
            }
            return response;
        }
    }
}
