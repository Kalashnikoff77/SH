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
using Microsoft.Data.SqlClient;
using System.Text.Json;
using WebAPI.Exceptions;

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

            var columns = GetRequiredColumns<SchedulesForEventsViewEntity>();

            using (var conn = new SqlConnection(connectionString))
            {
                SchedulesForEventsViewEntity result;

                var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                    $"FROM SchedulesForEventsView " +
                    $"WHERE Id = @ScheduleId";
                result = await conn.QueryFirstOrDefaultAsync<SchedulesForEventsViewEntity>(sql, new { request.ScheduleId }) ?? throw new NotFoundException("Встреча не найдена!");

                response.Event = _mapper.Map<SchedulesForEventsViewDto>(result);
            }

            return response;
        }


        [Route("Get"), HttpPost]
        public async Task<GetEventsResponseDto?> GetAsync(GetEventsRequestDto request)
        {
            var response = new GetEventsResponseDto();

            var columns = GetRequiredColumns<SchedulesForEventsViewEntity>();

            using (var conn = new SqlConnection(connectionString))
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                // Сперва получим Id записей, которые нужно вытянуть + кол-во записей.
                var p = new DynamicParameters();
                p.Add("@GetEventsRequestDto", jsonRequest);
                var ids = await conn.QueryAsync<int>("EventsFilter_sp", p, commandType: System.Data.CommandType.StoredProcedure);
                response.Count = ids.Count();

                if (response.Count > 0)
                {
                    var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                        $"FROM SchedulesForEventsView WHERE Id IN ({string.Join(",", ids)}) " +
                        $"ORDER BY {nameof(SchedulesForEventsViewDto.StartDate)} " +
                        $"OFFSET {request.Skip} ROWS FETCH NEXT {request.Take} ROWS ONLY";
                    var result = await conn.QueryAsync<SchedulesForEventsViewEntity>(sql);
                    response.Events = _mapper.Map<List<SchedulesForEventsViewDto>>(result);
                }
            }
            return response;
        }

        

        [Route("GetDiscussions"), HttpPost]
        public async Task<GetDiscussionsForEventsResponseDto?> GetDiscussions(GetDiscussionsForEventsRequestDto request)
        {
            if (request.EventId == 0)
                throw new NotFoundException("Не указано мероприятие (EventId = 0)!");

            var response = new GetDiscussionsForEventsResponseDto();
            IEnumerable<DiscussionsForEventsEntity> result;

            using (var conn = new SqlConnection(connectionString))
            {
                // Получим кол-во сообщений в чате мероприятия
                var sql = $"SELECT COUNT(*) FROM DiscussionsForEvents " +
                    $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)}";
                response.NumOfDiscussions = await conn.QuerySingleAsync<int>(sql, new { request.EventId });

                // Запрос на получение предыдущих сообщений
                if (request.GetPreviousFromId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM DiscussionsForEventsView " +
                            $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)} " +
                            $"AND Id < {request.GetPreviousFromId} " +
                            $"ORDER BY Id DESC";
                    result = (await conn.QueryAsync<DiscussionsForEventsViewEntity>(sql, new { request.EventId, request.Take })).Reverse();
                }

                // Запрос на получение следующих сообщений
                else if (request.GetNextAfterId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM DiscussionsForEventsView " +
                            $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)} " +
                            $"AND Id > {request.GetNextAfterId} " +
                            $"ORDER BY Id ASC";
                    result = await conn.QueryAsync<DiscussionsForEventsViewEntity>(sql, new { request.EventId, request.Take });
                }

                // Запрос на получение последних сообщений (по умолчанию)
                else
                {
                    int offset = response.NumOfDiscussions > StaticData.EVENT_DISCUSSIONS_PER_BLOCK ? response.NumOfDiscussions - StaticData.EVENT_DISCUSSIONS_PER_BLOCK : 0;
                    sql = $"SELECT * FROM DiscussionsForEventsView " +
                        $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)} " +
                        $"ORDER BY Id ASC " +
                        $"OFFSET {offset} ROWS";
                    result = await conn.QueryAsync<DiscussionsForEventsViewEntity>(sql, new { request.EventId });
                }
            }

            response.Discussions = _mapper.Map<List<DiscussionsForEventsViewDto>>(result);

            return response;
        }


        [Route("AddDiscussion"), HttpPost, Authorize]
        public async Task<AddDiscussionsForEventsResponseDto?> AddDiscussion(AddDiscussionsForEventsRequestDto request)
        {
            if (request.EventId == 0)
                throw new NotFoundException("Не указано мероприятие (EventId = 0)!");

            if (string.IsNullOrWhiteSpace(request.Text))
                throw new BadRequestException("Вы не ввели текст сообщения!");

            AuthenticateUser();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"INSERT INTO DiscussionsForEvents " +
                    $"({nameof(DiscussionsForEventsEntity.EventId)}, {nameof(DiscussionsForEventsEntity.SenderId)}, {nameof(DiscussionsForEventsEntity.RecipientId)}, {nameof(DiscussionsForEventsEntity.DiscussionId)}, {nameof(DiscussionsForEventsEntity.Text)}) " +
                    $"OUTPUT INSERTED.Id " +
                    $"VALUES " +
                    $"(@{nameof(DiscussionsForEventsEntity.EventId)}, @_accountId, @{nameof(DiscussionsForEventsEntity.RecipientId)}, @{nameof(DiscussionsForEventsEntity.DiscussionId)}, @{nameof(DiscussionsForEventsEntity.Text)})";
                var newId = await conn.QuerySingleAsync<int>(sql, new { request.EventId, _accountId, request.RecipientId, request.DiscussionId, request.Text });

                return new AddDiscussionsForEventsResponseDto { NewDiscussionId = newId };
            }
        }


        [Route("UpdateRegistration"), HttpPost, Authorize]
        public async Task<UpdateEventRegistrationResponseDto> UpdateRegistrationAsync(UpdateEventRegistrationRequestDto request)
        {
            AuthenticateUser();

            using (var conn = new SqlConnection(connectionString))
            {
                //var sql = $"SELECT TOP 1 * FROM EventsForAccounts WHERE {nameof(EventsForAccountsEntity.AccountId)} = @_accountId AND {nameof(EventsForAccountsEntity.EventId)} = @EventId";
                //var accountEvent = await conn.QueryFirstOrDefaultAsync<EventsForAccountsEntity>(sql, new { _accountId, request.EventId });

                //if (accountEvent == null)
                //{
                //    sql = $"INSERT INTO EventsForAccounts " +
                //        $"({nameof(EventsForAccountsEntity.AccountId)}, {nameof(EventsForAccountsEntity.EventId)}) " +
                //        $"VALUES (@_accountId, @EventId, 0, 0)";
                //    await conn.ExecuteAsync(sql, new { _accountId, request.EventId });
                //    accountEvent = new EventsForAccountsEntity();
                //}

                var response = new UpdateEventRegistrationResponseDto 
                {
                    EventId = request.EventId,
                };

                if (request.ToRegister)
                    response.IsRegistered = !response.IsRegistered;

                // Доработать
                //if (response.IsRegistered)
                //    sql = $"UPDATE AccountsEvents SET {nameof(EventsForAccountsEntity.IsRegistered)} = @{nameof(EventsForAccountsEntity.IsRegistered)} " +
                //        $"WHERE {nameof(EventsForAccountsEntity.AccountId)} = @_accountId AND {nameof(EventsForAccountsEntity.EventId)} = @EventId";
                //else
                //    sql = $"DELETE FROM EventsForAccounts " +
                //        $"WHERE {nameof(EventsForAccountsEntity.AccountId)} = @_accountId AND {nameof(EventsForAccountsEntity.EventId)} = @EventId";

                //await conn.ExecuteAsync(sql, new { _accountId, request.EventId, response.IsRegistered });

                return response;
            }
        }


        [Route("GetFeatures"), HttpPost]
        public async Task<GetFeaturesResponseDto> GetFeaturesAsync(GetFeaturesRequestDto request)
        {
            var response = new GetFeaturesResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT * FROM Features ORDER BY Name";
                var result = await conn.QueryAsync<FeaturesEntity>(sql);

                response.Features = _mapper.Map<List<FeaturesDto>>(result);
            }
            return response;
        }
    }
}
