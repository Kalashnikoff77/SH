using AutoMapper;
using Common;
using Common.Dto;
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
    public class MessagesController : MyControllerBase
    {
        public MessagesController(IMapper mapper, IConfiguration configuration) : base(mapper, configuration) { }


        [Route("Count"), HttpPost, Authorize]
        public async Task<GetMessagesCountResponseDto> GetCountAsync(GetMessagesCountRequestDto request)
        {
            AuthenticateUser();

            var response = new GetMessagesCountResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT COUNT(*) FROM Messages " +
                    $"WHERE {nameof(MessagesEntity.SenderId)} = @_accountId OR {nameof(MessagesEntity.RecipientId)} = @_accountId";
                response.TotalCount = await conn.QueryFirstAsync<int>(sql, new { _accountId });

                sql = $"SELECT COUNT(*) FROM Messages " +
                    $"WHERE {nameof(MessagesEntity.RecipientId)} = @_accountId AND {nameof(MessagesEntity.ReadDate)} IS NULL";
                response.UnreadCount = await conn.QueryFirstAsync<int>(sql, new { _accountId });
            }

            return response;
        }


        [Route("Get"), HttpPost, Authorize]
        public async Task<GetMessagesResponseDto> GetAsync(GetMessagesRequestDto request)
        {
            AuthenticateUser();

            var response = new GetMessagesResponseDto();
            IEnumerable<MessagesEntity> result;

            using (var conn = new SqlConnection(connectionString))
            {
                // Получим кол-во сообщений
                var sql = $"SELECT COUNT(*) FROM Messages " +
                    $"WHERE ({nameof(MessagesEntity.SenderId)} = @_accountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                    $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @_accountId)";
                response.Count = await conn.QueryFirstAsync<int>(sql, new { _accountId, request.RecipientId });

                // Запрос на получение предыдущих сообщений
                if (request.GetPreviousFromId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM Messages " +
                        $"WHERE (({nameof(MessagesEntity.SenderId)} = @_accountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                        $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @_accountId)) " +
                        $"AND Id < {request.GetPreviousFromId} " +
                        $"ORDER BY Id DESC";
                    result = (await conn.QueryAsync<MessagesEntity>(sql, new { _accountId, request.RecipientId, request.Take })).Reverse();
                }

                // Запрос на получение следующих сообщений
                else if (request.GetNextAfterId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM Messages " +
                        $"WHERE (({nameof(MessagesEntity.SenderId)} = @_accountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                        $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @_accountId)) " +
                        $"AND Id > {request.GetNextAfterId} " +
                        $"ORDER BY Id ASC";
                    result = await conn.QueryAsync<MessagesEntity>(sql, new { _accountId, request.RecipientId, request.Take });
                }

                // Запрос на получение последних сообщений (по умолчанию)
                else
                {
                    int offset = response.Count > StaticData.MESSAGES_PER_BLOCK ? response.Count- StaticData.MESSAGES_PER_BLOCK : 0;
                    sql = $"SELECT * FROM Messages " +
                        $"WHERE ({nameof(MessagesEntity.SenderId)} = @_accountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                        $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @_accountId) " +
                        $"ORDER BY Id ASC " +
                        $"OFFSET {offset} ROWS";
                    result = await conn.QueryAsync<MessagesEntity>(sql, new { _accountId, request.RecipientId });
                }

                response.Messages = _mapper.Map<List<MessagesDto>>(result);

                // Получим отправителя и получателя
                var columns = GetRequiredColumns<AccountsViewEntity>();
                sql = $"SELECT TOP 2 {columns.Aggregate((a, b) => a + ", " + b)} FROM AccountsView WHERE Id = @_accountId OR Id = @RecipientId";
                var accounts = await conn.QueryAsync<AccountsViewEntity>(sql, new { _accountId, request.RecipientId });
                response.Sender = _mapper.Map<AccountsViewDto>(accounts.FirstOrDefault(x => x.Id == _accountId));
                response.Recipient = _mapper.Map<AccountsViewDto>(accounts.FirstOrDefault(x => x.Id == request.RecipientId));

                // Будем отмечать сообщения, как прочитанные?
                if (request.MarkAsRead)
                {
                    var ids = response.Messages
                        .Where(w => w.RecipientId == _accountId && w.ReadDate == null)
                        .Select(s => s.Id.ToString());

                    if (ids.Any())
                        await conn.ExecuteAsync($"UPDATE Messages SET {nameof(MessagesEntity.ReadDate)} = getdate() " +
                            $"WHERE Id IN ({ids.Aggregate((a, b) => a + ", " + b)})");
                }
            }
            return response;
        }


        [Route("GetLastMessagesList"), HttpPost, Authorize]
        public async Task<GetLastMessagesListResponseDto> GetLastMessagesListAsync(GetLastMessagesListRequestDto request)
        {
            AuthenticateUser();

            var response = new GetLastMessagesListResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT TOP (@Take) * FROM LastMessagesListView " +
                    $"WHERE {nameof(LastMessagesListViewEntity.SenderId)} = @_accountId " +
                    $"OR {nameof(LastMessagesListViewEntity.RecipientId)} = @_accountId";
                var result = await conn.QueryAsync<LastMessagesListViewEntity>(sql, new { _accountId, request.Take });

                var sortedResult = result.Where(x => x.RecipientId == _accountId)
                    .Union(result.Where(x => x.SenderId == _accountId))
                    .ToList();

                response.LastMessagesList = _mapper.Map<List<LastMessagesListViewDto>>(sortedResult);
            }

            return response;
        }


        [Route("Add"), HttpPost, Authorize]
        public async Task<AddMessageResponseDto> AddAsync(AddMessageRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                throw new BadRequestException("Вы не ввели текст сообщения!");

            AuthenticateUser();

            var response = new AddMessageResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @_accountId";
                var senderId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { _accountId }) ?? throw new NotFoundException($"Пользователь-отправитель с Id {_accountId} не найден!");

                sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @RecipientId";
                var recipientId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.RecipientId }) ?? throw new NotFoundException($"Пользователь-получатель с Id {_accountId} не найден!");

                sql = $"INSERT INTO Messages ({nameof(MessagesEntity.SenderId)}, {nameof(MessagesEntity.RecipientId)}, {nameof(MessagesEntity.Text)}) " +
                    "VALUES (@senderId, @recipientId, @Text)";
                await conn.ExecuteAsync(sql, new { senderId, recipientId, request.Text });

                var message = new MessagesEntity
                {
                    SenderId = _accountId,
                    RecipientId = request.RecipientId,
                    Text = request.Text
                };

                response.Message = new MessagesDto
                {
                    SenderId = _accountId,
                    RecipientId = request.RecipientId,
                    CreateDate = DateTime.Now,
                    Text = message.Text
                };

                return response;
            }
        }
    }
}
