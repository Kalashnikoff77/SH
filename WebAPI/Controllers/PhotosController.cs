using AutoMapper;
using Azure.Core;
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
using Microsoft.Identity.Client;
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
                var sql = "SELECT TOP 1 Id FROM PhotosForAccounts " +
                    "WHERE AccountId = @_accountId AND Guid = @Guid";
                var photoId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { _accountId, request.Guid }) ?? throw new NotFoundException("Соответствующее фото не найдено!");

                sql = $"UPDATE PhotosForAccounts SET " +
                    $"{nameof(PhotosForAccountsEntity.Comment)} = @Comment, " +
                    $"{nameof(PhotosForAccountsEntity.IsAvatar)} = @IsAvatar, " +
                    $"{nameof(PhotosForAccountsEntity.IsDeleted)} = @IsDeleted " +
                    $"WHERE {nameof(PhotosForAccountsEntity.AccountId)} = @_accountId AND {nameof(PhotosForAccountsEntity.Guid)} = @Guid";
                await conn.ExecuteAsync(sql, new { request.Comment, request.IsAvatar, request.IsDeleted, _accountId, request.Guid });

                return response;
            }
        }


        [Route("UploadFromTemp"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> UploadFromTempAsync(UploadPhotosFromTempRequestDto request)
        {
            AuthenticateUser();

            var response = new ResponseDtoBase();

            using (var conn = new SqlConnection(connectionString))
            {
                foreach (var photoTempFileName in request.PhotosTempFileNames)
                {
                    if (!string.IsNullOrWhiteSpace(photoTempFileName))
                    {
                        var guid = Guid.NewGuid();
                        Directory.CreateDirectory($"{StaticData.AccountsPhotosDir}/{_accountId}/{guid}");

                        foreach (var image in StaticData.Images)
                        {
                            var fileName = $"{StaticData.AccountsPhotosDir}/{_accountId}/{guid}/{image.Key}.jpg";

                            using (MemoryStream output = new MemoryStream(500000))
                            {
                                MagicImageProcessor.ProcessImage($"{StaticData.AccountsPhotosTempDir}/{photoTempFileName}", output, image.Value);
                                await System.IO.File.WriteAllBytesAsync(fileName, output.ToArray());
                            }
                        }

                        var sql = "INSERT INTO PhotosForAccounts " +
                            $"({nameof(PhotosForAccountsEntity.Guid)}, {nameof(PhotosForAccountsEntity.AccountId)}) " +
                            "VALUES " +
                            $"(@{nameof(PhotosForAccountsEntity.Guid)}, @{nameof(PhotosForAccountsEntity.AccountId)})";
                        await conn.ExecuteAsync(sql, new { Guid = guid, AccountId = _accountId });
                    }
                }

                foreach (var photoTempFileName in request.PhotosTempFileNames)
                    System.IO.File.Delete($"{StaticData.AccountsPhotosTempDir}/{photoTempFileName}");
            }
            return response;
        }
    }
}