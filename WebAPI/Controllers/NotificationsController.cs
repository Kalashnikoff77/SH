using AutoMapper;
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
using WebAPI.Exceptions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : MyControllerBase
    {
        public NotificationsController(IMapper mapper, IConfiguration configuration) : base(mapper, configuration) { }

        [Route("Count"), HttpPost, Authorize]
        public async Task<GetNotificationsCountResponseDto> GetCountAsync(GetNotificationsCountRequestDto request)
        {
            AuthenticateUser();

            var response = new GetNotificationsCountResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT COUNT(*) FROM Notifications WHERE {nameof(NotificationsEntity.RecipientId)} = @_accountId";
                response.TotalCount = await conn.QueryFirstAsync<int>(sql, new { _accountId });

                sql = $"SELECT COUNT(*) FROM Notifications WHERE {nameof(NotificationsEntity.RecipientId)} = @_accountId AND {nameof(NotificationsEntity.ReadDate)} IS NULL";
                response.UnreadCount = await conn.QueryFirstAsync<int>(sql, new { _accountId });
            }

            return response;
        }

        [Route("Get"), HttpPost, Authorize]
        public async Task<GetNotificationsResponseDto?> GetAsync(GetNotificationsRequestDto request)
        {
            AuthenticateUser();

            var response = new GetNotificationsResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                // Получим все уведомления
                var sql = "SELECT TOP (@Take) * FROM NotificationsView " +
                    $"WHERE {nameof(NotificationsViewEntity.RecipientId)} = @_accountId " +
                    $"ORDER BY {nameof(NotificationsViewEntity.CreateDate)} ASC";
                var notifications = await conn.QueryAsync<NotificationsViewEntity>(sql, new { _accountId, request.Take });
                response.Notifications = _mapper.Map<List<NotificationsViewDto>>(notifications);

                // Будем отмечать уведомления, как прочитанные?
                if (request.MarkAsRead)
                {
                    var ids = response.Notifications
                        .Where(w => w.ReadDate == null)
                        .Select(s => s.Id.ToString());

                    if (ids.Any())
                        await conn.ExecuteAsync($"UPDATE Notifications SET {nameof(NotificationsEntity.ReadDate)} = getdate() " +
                            $"WHERE Id IN ({ids.Aggregate((a, b) => a + ", " + b)})");
                }
            }

            return response;
        }


        [Route("Add"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> AddAsync(AddNotificationRequestDto request)
        {
            AuthenticateUser();

            var response = new ResponseDtoBase();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @_accountId";
                var senderId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { _accountId }) ?? throw new NotFoundException($"Пользователь-отправитель с Id {_accountId} не найден!");

                sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @RecipientId";
                var recipientId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.RecipientId }) ?? throw new NotFoundException($"Пользователь-получатель с Id {_accountId} не найден!");

                sql = $"INSERT INTO Notifications ({nameof(NotificationsEntity.SenderId)}, {nameof(NotificationsEntity.RecipientId)}, {nameof(NotificationsEntity.Text)}) " +
                    "VALUES (@senderId, @recipientId, @Text)";
                await conn.ExecuteAsync(sql, new { senderId, recipientId, request.Text });

                return response;
            }
        }
    }
}
