﻿using AutoMapper;
using Common;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
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
        public async Task<GetAccountPhotosResponseDto> GetAsync(GetAccountPhotosRequestDto request)
        {
            AuthenticateUser();

            var response = new GetAccountPhotosResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var columns = GetRequiredColumns<AccountsPhotosViewEntity>();

                // Получаем фото текущего пользователя или указанного в request?
                var accountId = request.AccountId == null ? _accountId : request.AccountId;

                var sql = "SELECT TOP (@Take) " +
                    $"{columns.Aggregate((a, b) => a + ", " + b)} " +
                    $"FROM AccountsPhotosView WHERE {nameof(AccountsPhotosViewEntity.AccountId)} = @accountId " +
                    "ORDER BY Id DESC";
                var result = await conn.QueryAsync<AccountsPhotosViewEntity>(sql, new { accountId, request.Take });

                response.Photos = _mapper.Map<List<AccountsPhotosViewDto>>(result);
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
                var sql = "SELECT TOP 1 Id FROM AccountsPhotos " +
                    "WHERE AccountId = @_accountId AND Guid = @Guid";
                var photoId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { _accountId, request.Guid }) ?? throw new NotFoundException("Соответствующее фото не найдено!");

                sql = $"UPDATE AccountsPhotos SET " +
                    $"{nameof(AccountsPhotosEntity.Comment)} = @Comment, " +
                    $"{nameof(AccountsPhotosEntity.IsAvatar)} = @IsAvatar, " +
                    $"{nameof(AccountsPhotosEntity.IsDeleted)} = @IsDeleted " +
                    $"WHERE {nameof(AccountsPhotosEntity.AccountId)} = @_accountId AND {nameof(AccountsPhotosEntity.Guid)} = @Guid";
                await conn.ExecuteAsync(sql, new { request.Comment, request.IsAvatar, request.IsDeleted, _accountId, request.Guid });

                return response;
            }
        }


        [Route("Upload"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> UploadAsync(UploadPhotoRequestDto request)
        {
            AuthenticateUser();

            var response = new ResponseDtoBase();

            using (var conn = new SqlConnection(connectionString))
            {
                foreach (var photoName in request.PhotoNames)
                {
                    var guid = Guid.NewGuid();

                    await ProcessPhotoFile(photoName, _accountId, guid);

                    var sql = "INSERT INTO AccountsPhotos " +
                        $"({nameof(AccountsPhotosEntity.Guid)}, {nameof(AccountsPhotosEntity.AccountId)}) " +
                        $"VALUES (@guid, @_accountId)";
                    await conn.ExecuteAsync(sql, new { guid, _accountId });
                }

                return response;
            }
        }

        [Route("UploadFile"), HttpPost]
        public async Task<ResponseDtoBase> UploadFileAsync([FromForm(Name = "file")] IFormFile file)
        {
            var response = new ResponseDtoBase();

            if (file == null)
            {
                return response;
            }

            //create unique name for file
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            //set file url
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "../UI/wwwroot/images/AccountsPhotos/temp", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return response;
        }

        private async Task ProcessPhotoFile(string photo, int accountId, Guid guid)
        {
            if (!string.IsNullOrWhiteSpace(photo))
            {
                var dir = "../UI/wwwroot/images/AccountsPhotos";

                if (!Directory.Exists($"{dir}/{accountId}"))
                    Directory.CreateDirectory($"{dir}/{accountId}");
                
                Directory.CreateDirectory($"{dir}/{accountId}/{guid}");

                foreach (var image in StaticData.Images)
                {
                    var fileName = $"{dir}/{accountId}/{guid}/{image.Key}.jpg";

                    using (MemoryStream output = new MemoryStream(500000))
                    {
                        MagicImageProcessor.ProcessImage($"{dir}/temp/{photo}", output, image.Value);
                        await System.IO.File.WriteAllBytesAsync(fileName, output.ToArray());
                    }
                }

                System.IO.File.Delete($"{dir}/temp/{photo}");
            }
        }
    }
}