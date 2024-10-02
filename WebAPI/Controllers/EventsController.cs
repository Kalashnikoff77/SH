using AutoMapper;
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
using System.Text.Json;
using WebAPI.Exceptions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : MyControllerBase
    {
        public EventsController(IMapper mapper, IConfiguration configuration) : base(mapper, configuration) { }


        [Route("Get"), HttpPost]
        public async Task<GetSchedulesResponseDto?> GetAsync(GetSchedulesRequestDto request)
        {
            var response = new GetSchedulesResponseDto();

            var columns = GetRequiredColumns<SchedulesForEventsViewEntity>();

            using (var conn = new SqlConnection(connectionString))
            {
                // Получить одну запись
                if (request.ScheduleId.HasValue && request.ScheduleId > 0)
                {
                    var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                        $"FROM SchedulesForEventsView " +
                        $"WHERE Id = @ScheduleId";
                    var result = await conn.QueryFirstOrDefaultAsync<SchedulesForEventsViewEntity>(sql, new { request.ScheduleId }) ?? throw new NotFoundException("Встреча не найдена!");
                    response.Schedule = _mapper.Map<SchedulesForEventsViewDto>(result);
                }

                // Получить все расписания определённого мероприятия
                else if (request.EventId.HasValue && request.EventId > 0)
                {
                    var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                        $"FROM SchedulesForEventsView " +
                        $"WHERE EventId = @EventId";
                    var result = await conn.QueryAsync<SchedulesForEventsViewEntity>(sql, new { request.EventId });
                    response.Schedules = _mapper.Map<List<SchedulesForEventsViewDto>>(result);
                }

                // Получить несколько записей
                else
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
                        response.Schedules = _mapper.Map<List<SchedulesForEventsViewDto>>(result);
                    }
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
                    sql = $"SELECT TOP (@Take) * FROM DiscussionsForEventsView " +
                        $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)} " +
                        $"ORDER BY Id DESC";
                    result = (await conn.QueryAsync<DiscussionsForEventsViewEntity>(sql, new { request.EventId, request.Take })).Reverse();
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
                // Получим тип учётки (пара, М или Ж)
                var sql = $"SELECT TOP (2) {nameof(UsersEntity.Gender)} FROM Users WHERE {nameof(UsersEntity.AccountId)} = {_accountId}";
                var users = (await conn.QueryAsync<int>(sql)).ToList();
                if (users == null)
                    throw new NotFoundException("Пользователь с указанным Id не найден!");
                int? AccountGender = null;
                if (users.Count() == 1)
                    AccountGender = users[0];

                // Получим данные о расписании
                sql = $"SELECT * FROM SchedulesForEvents WHERE {nameof(SchedulesForEventsEntity.Id)} = {request.ScheduleId}";
                var evt = await conn.QueryFirstOrDefaultAsync<SchedulesForEventsEntity>(sql) ?? throw new NotFoundException("Указанное расписание события не найдено!");

                // Получим стоимость для учётки
                int TicketCost = AccountGender switch
                {
                    null => evt.CostPair!.Value,
                    0 => evt.CostMan!.Value,
                    1 => evt.CostWoman!.Value
                };

                sql = $"SELECT TOP 1 Id FROM SchedulesForAccounts WHERE {nameof(SchedulesForAccountsEntity.AccountId)} = @_accountId AND {nameof(SchedulesForAccountsEntity.ScheduleId)} = @ScheduleId AND IsDeleted = 0";
                var scheduleId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { _accountId, request.ScheduleId });
                if (scheduleId == null)
                {
                    sql = $"INSERT INTO SchedulesForAccounts " +
                        $"({nameof(SchedulesForAccountsEntity.ScheduleId)}, {nameof(SchedulesForAccountsEntity.AccountId)}, {nameof(SchedulesForAccountsEntity.AccountGender)}, {nameof(SchedulesForAccountsEntity.TicketCost)}) " +
                        $"VALUES (@{nameof(SchedulesForAccountsEntity.ScheduleId)}, @_accountId, @{nameof(SchedulesForAccountsEntity.AccountGender)}, @{nameof(SchedulesForAccountsEntity.TicketCost)})";
                    await conn.ExecuteAsync(sql, new { request.ScheduleId, _accountId, AccountGender, TicketCost });
                }
                else
                {
                    sql = $"UPDATE SchedulesForAccounts SET {nameof(SchedulesForAccountsEntity.IsDeleted)} = 1 " +
                        $"WHERE {nameof(SchedulesForAccountsEntity.AccountId)} = @_accountId AND {nameof(SchedulesForAccountsEntity.ScheduleId)} = @ScheduleId";
                    await conn.ExecuteAsync(sql, new { request.ScheduleId, _accountId });
                }

                var response = new UpdateEventRegistrationResponseDto { ScheduleId = request.ScheduleId };

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


        /// <summary>
        /// Получает список пользователей, которые являются админами текущих мероприятий
        /// </summary>
        [Route("GetAdminsForEvents"), HttpPost]
        public async Task<GetAdminsForEventsResponseDto> GetAdminsForEventsAsync(GetAdminsForEventsRequestDto request)
        {
            var response = new GetAdminsForEventsResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM AdminsForEventsView ORDER BY Name";
                var result = await conn.QueryAsync<AdminsForEventsViewEntity>(sql);
                response.AdminsForEvents = _mapper.Map<List<AdminsForEventsViewDto>>(result);
            }
            return response;
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


        [Route("GetFeaturesForEvents"), HttpPost]
        public async Task<GetFeaturesForEventsResponseDto> GetFeaturesForEventsAsync(GetFeaturesForEventsRequestDto request)
        {
            var response = new GetFeaturesForEventsResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT * FROM FeaturesForEventsView ORDER BY Name";
                var result = await conn.QueryAsync<FeaturesForEventsViewEntity>(sql);

                response.FeaturesForEvents = _mapper.Map<List<FeaturesForEventsViewDto>>(result);
            }
            return response;
        }
    }
}
