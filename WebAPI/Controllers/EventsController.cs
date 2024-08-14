using AutoMapper;
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
using WebAPI.Exceptions;
using WebAPI.Extensions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : MyControllerBase
    {
        public EventsController(IMapper mapper, IConfiguration configuration) : base(mapper, configuration) { }


        [Route("GetOne"), HttpPost]
        public async Task<GetEventOneResponseDto?> GetOneAsync(GetEventOneRequestDto request)
        {
            AuthenticateUser();

            var response = new GetEventOneResponseDto();

            var columns = GetRequiredColumns<EventsViewEntity>();
            columns.Add(nameof(EventsViewEntity.Photos));

            using (var conn = new SqlConnection(connectionString))
            {
                EventsViewEntity result;

                var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                    $"FROM EventsView " +
                    $"WHERE Id = @EventId";
                result = await conn.QueryFirstOrDefaultAsync<EventsViewEntity>(sql, new { request.EventId }) ?? throw new NotFoundException("Событие не найдено!");

                response.Event = _mapper.Map<EventsViewDto>(result);
            }

            return response;
        }


        [Route("Get"), HttpPost]
        public async Task<GetEventsResponseDto?> GetAsync(GetEventsRequestDto request)
        {
            var response = new GetEventsResponseDto();

            var columns = GetRequiredColumns<EventsViewEntity>();

            if (request.IsPhotosIncluded)
                columns.Add(nameof(EventsViewEntity.Photos));

            using (var conn = new SqlConnection(connectionString))
            {
                // (НЕ ДОДЕЛАНО!!!) Получить мероприятия конкретного пользователя
                if (request.AccountId.HasValue && request.AccountId.Value > 0)
                {
                    var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                        "FROM AccountsEventsView " +
                        $"WHERE AccountId = {request.AccountId} " +
                        $"OFFSET {request.Skip} ROWS FETCH NEXT {request.Take} ROWS ONLY";
                    var result = await conn.QueryAsync<EventsViewEntity>(sql);
                }
                // Получить все мероприятия
                //else
                //{
                //    var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                //        "FROM EventsView " +
                //        $"WHERE {nameof(EventsViewDto.EndDate)} > getdate() " + request.Filters() +
                //        $"ORDER BY {nameof(EventsViewDto.StartDate)}";
                //    var result = await conn.QueryAsync<EventsViewEntity>(sql, new { FilterValue = "%" + request.FilterValue + "%" });

                //    response.Count = result.Count();
                //    response.Events = _mapper.Map<List<EventsViewDto>>(result.Skip(request.Skip).Take(request.Take));
                //}
            }

            return response;
        }

        
        // Количество Subscribers, Registers, Discussions (SRD)
        [Route("GetSRD"), HttpPost]
        public async Task<GetEventsResponseDto?> GetSRDAsync(GetEventsSRDRequestDto request)
        {
            var response = new GetEventsResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var result = await conn.QueryAsync<EventsViewEntity>($"SELECT Id, " +
                    $"{nameof(EventsViewEntity.NumOfSubscribers)}, {nameof(EventsViewEntity.NumOfRegisters)}, {nameof(EventsViewEntity.NumOfDiscussions)} " +
                    $"FROM EventsView");

                response.Events = _mapper.Map<List<EventsViewDto>>(result);
            }

            return response;
        }


        [Route("GetDiscussions"), HttpPost]
        public async Task<GetEventDiscussionsResponseDto?> GetDiscussions(GetEventDiscussionsRequestDto request)
        {
            if (request.EventId == 0)
                throw new NotFoundException("Не указано мероприятие (EventId = 0)!");

            var response = new GetEventDiscussionsResponseDto();
            IEnumerable<EventsDiscussionsEntity> result;

            using (var conn = new SqlConnection(connectionString))
            {
                // Получим кол-во сообщений в чате мероприятия
                var sql = $"SELECT COUNT(*) FROM EventsDiscussions " +
                    $"WHERE {nameof(EventsDiscussionsViewEntity.EventId)} = @{nameof(EventsDiscussionsViewEntity.EventId)}";
                response.NumOfDiscussions = await conn.QuerySingleAsync<int>(sql, new { request.EventId });

                // Запрос на получение предыдущих сообщений
                if (request.GetPreviousFromId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM EventsDiscussionsView " +
                            $"WHERE {nameof(EventsDiscussionsViewEntity.EventId)} = @{nameof(EventsDiscussionsViewEntity.EventId)} " +
                            $"AND Id < {request.GetPreviousFromId} " +
                            $"ORDER BY Id DESC";
                    result = (await conn.QueryAsync<EventsDiscussionsViewEntity>(sql, new { request.EventId, request.Take })).Reverse();
                }

                // Запрос на получение следующих сообщений
                else if (request.GetNextAfterId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM EventsDiscussionsView " +
                            $"WHERE {nameof(EventsDiscussionsViewEntity.EventId)} = @{nameof(EventsDiscussionsViewEntity.EventId)} " +
                            $"AND Id > {request.GetNextAfterId} " +
                            $"ORDER BY Id ASC";
                    result = await conn.QueryAsync<EventsDiscussionsViewEntity>(sql, new { request.EventId, request.Take });
                }

                // Запрос на получение последних сообщений (по умолчанию)
                else
                {
                    int offset = response.NumOfDiscussions > StaticData.EVENT_DISCUSSIONS_PER_BLOCK ? response.NumOfDiscussions - StaticData.EVENT_DISCUSSIONS_PER_BLOCK : 0;
                    sql = $"SELECT * FROM EventsDiscussionsView " +
                        $"WHERE {nameof(EventsDiscussionsViewEntity.EventId)} = @{nameof(EventsDiscussionsViewEntity.EventId)} " +
                        $"ORDER BY Id ASC " +
                        $"OFFSET {offset} ROWS";
                    result = await conn.QueryAsync<EventsDiscussionsViewEntity>(sql, new { request.EventId });
                }
            }

            response.Discussions = _mapper.Map<List<EventsDiscussionsViewDto>>(result);

            return response;
        }


        [Route("AddDiscussion"), HttpPost, Authorize]
        public async Task<AddEventDiscussionResponseDto?> AddDiscussion(AddEventDiscussionRequestDto request)
        {
            if (request.EventId == 0)
                throw new NotFoundException("Не указано мероприятие (EventId = 0)!");

            if (string.IsNullOrWhiteSpace(request.Text))
                throw new BadRequestException("Вы не ввели текст сообщения!");

            AuthenticateUser();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"INSERT INTO EventsDiscussions " +
                    $"({nameof(EventsDiscussionsEntity.EventId)}, {nameof(EventsDiscussionsEntity.SenderId)}, {nameof(EventsDiscussionsEntity.RecipientId)}, {nameof(EventsDiscussionsEntity.DiscussionId)}, {nameof(EventsDiscussionsEntity.Text)}) " +
                    $"OUTPUT INSERTED.Id " +
                    $"VALUES " +
                    $"(@{nameof(EventsDiscussionsEntity.EventId)}, @_accountId, @{nameof(EventsDiscussionsEntity.RecipientId)}, @{nameof(EventsDiscussionsEntity.DiscussionId)}, @{nameof(EventsDiscussionsEntity.Text)})";
                var newId = await conn.QuerySingleAsync<int>(sql, new { request.EventId, _accountId, request.RecipientId, request.DiscussionId, request.Text });

                return new AddEventDiscussionResponseDto { NewDiscussionId = newId };
            }
        }


        [Route("UpdateSubscription"), HttpPost, Authorize]
        public async Task<UpdateEventSubscriptionResponseDto> UpdateSubscriptionAsync(UpdateEventSubscriptionRequestDto request)
        {
            AuthenticateUser();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT TOP 1 * FROM AccountsEvents WHERE {nameof(AccountsEventsEntity.AccountId)} = @_accountId AND {nameof(AccountsEventsEntity.EventId)} = @EventId";
                var accountEvent = await conn.QueryFirstOrDefaultAsync<AccountsEventsEntity>(sql, new { _accountId, request.EventId });

                if (accountEvent == null)
                {
                    sql = $"INSERT INTO AccountsEvents " +
                        $"({nameof(AccountsEventsEntity.AccountId)}, {nameof(AccountsEventsEntity.EventId)}, {nameof(AccountsEventsEntity.IsSubscribed)}, {nameof(AccountsEventsEntity.IsRegistered)}) " +
                        $"VALUES (@_accountId, @EventId, 0, 0)";
                    await conn.ExecuteAsync(sql, new { _accountId, request.EventId });
                    accountEvent = new AccountsEventsEntity();
                }

                var response = new UpdateEventSubscriptionResponseDto 
                {
                    EventId = request.EventId,
                    IsSubscribed = accountEvent.IsSubscribed,
                    IsRegistered = accountEvent.IsRegistered
                };

                if (request.ToSubscribe)
                    response.IsSubscribed = !response.IsSubscribed;
                else
                    response.IsRegistered = !response.IsRegistered;

                if (response.IsSubscribed || response.IsRegistered)
                    sql = $"UPDATE AccountsEvents SET {nameof(AccountsEventsEntity.IsSubscribed)} = @{nameof(AccountsEventsEntity.IsSubscribed)}, {nameof(AccountsEventsEntity.IsRegistered)} = @{nameof(AccountsEventsEntity.IsRegistered)} " +
                        $"WHERE {nameof(AccountsEventsEntity.AccountId)} = @_accountId AND {nameof(AccountsEventsEntity.EventId)} = @EventId";
                else
                    sql = $"DELETE FROM AccountsEvents " +
                        $"WHERE {nameof(AccountsEventsEntity.AccountId)} = @_accountId AND {nameof(AccountsEventsEntity.EventId)} = @EventId";

                await conn.ExecuteAsync(sql, new { _accountId, request.EventId, response.IsSubscribed, response.IsRegistered });

                return response;
            }
        }
    }
}
