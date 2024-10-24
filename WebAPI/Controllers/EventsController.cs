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
using System.Text;
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
        public async Task<GetEventsResponseDto?> GetEventsAsync(GetEventsRequestDto request)
        {
            var response = new GetEventsResponseDto();

            var columns = GetRequiredColumns<EventsViewEntity>();

            if (request.IsPhotosIncluded)
                columns.Add(nameof(EventsViewEntity.Photos));

            using (var conn = new SqlConnection(connectionString))
            {
                if (request.EventId != null)
                {
                    var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                        $"FROM EventsView " +
                        $"WHERE Id = @EventId";
                    var result = await conn.QueryFirstOrDefaultAsync<EventsViewEntity>(sql, new { request.EventId }) ?? throw new NotFoundException("Встреча не найдена!");
                    response.Event = _mapper.Map<EventsViewDto>(result);
                }
            }
            return response;
        }


        [Route("GetSchedules"), HttpPost]
        public async Task<GetSchedulesResponseDto?> GetSchedulesAsync(GetSchedulesRequestDto request)
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


        /// <summary>
        /// Проверка при добавлении или обновлении мероприятия
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("Check"), HttpPost, Authorize]
        public async Task<EventCheckResponseDto> CheckAsync(EventCheckRequestDto request)
        {
            var response = new EventCheckResponseDto();
            string sql;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (request.EventName != null)
                {
                    if (request.EventId.HasValue)
                        sql = $"SELECT TOP 1 Id FROM Events WHERE Name = @EventName AND Id <> @EventId";
                    else
                        sql = $"SELECT TOP 1 Id FROM Events WHERE Name = @EventName";
                    var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.EventId, request.EventName });
                    response.EventNameExists = result == null ? false : true;
                }
            }
            return response;
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


        /// <summary>
        /// Получить список всех features мероприятий
        /// </summary>
        /// <param name="request"></param>
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


        /// <summary>
        /// Получить список всех features, которые фигурируют в мероприятиях плюс кол-во мероприятий, в которых они фигурируют
        /// </summary>
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


        /// <summary>
        /// Обновление мероприятия
        /// </summary>
        [Route("Update"), HttpPost, Authorize]
        public async Task<UpdateEventResponseDto> UpdateAsync(UpdateEventRequestDto request)
        {
            AuthenticateUser();

            var response = new UpdateEventResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using var transaction = conn.BeginTransaction();

                var sql = $"UPDATE Events SET " +
                    $"{nameof(EventsEntity.Name)} = @{nameof(EventsEntity.Name)}, " +
                    $"{nameof(EventsEntity.Description)} = @{nameof(EventsEntity.Description)}, " +
                    $"{nameof(EventsEntity.MaxMen)} = @{nameof(EventsEntity.MaxMen)}, " +
                    $"{nameof(EventsEntity.MaxWomen)} = @{nameof(EventsEntity.MaxWomen)}, " +
                    $"{nameof(EventsEntity.MaxPairs)} = @{nameof(EventsEntity.MaxPairs)} " +
                    $"WHERE Id = @Id AND {nameof(EventsEntity.AdminId)} = @_accountId";
                var result = await conn.ExecuteAsync(sql, new { request.Event.Id, request.Event.Name, request.Event.Description, request.Event.MaxMen, request.Event.MaxWomen, request.Event.MaxPairs, _accountId }, transaction: transaction);

                // Расписание
                if (request.Event.Schedule != null)
                {
                    foreach (var schedule in request.Event.Schedule)
                    {
                        if (schedule.Id == 0)
                        {
                            sql = $"INSERT INTO SchedulesForEvents (" +
                                $"{nameof(SchedulesForEventsEntity.EventId)}, " +
                                $"{nameof(SchedulesForEventsEntity.Description)}, " +
                                $"{nameof(SchedulesForEventsEntity.StartDate)}, " +
                                $"{nameof(SchedulesForEventsEntity.EndDate)}, " +
                                $"{nameof(SchedulesForEventsEntity.CostMan)}, " +
                                $"{nameof(SchedulesForEventsEntity.CostWoman)}, " +
                                $"{nameof(SchedulesForEventsEntity.CostPair)}" +
                                $") VALUES (" +
                                $"@{nameof(SchedulesForEventsEntity.EventId)}, " +
                                $"@{nameof(SchedulesForEventsEntity.Description)}, " +
                                $"@{nameof(SchedulesForEventsEntity.StartDate)}, " +
                                $"@{nameof(SchedulesForEventsEntity.EndDate)}, " +
                                $"@{nameof(SchedulesForEventsEntity.CostMan)}, " +
                                $"@{nameof(SchedulesForEventsEntity.CostWoman)}, " +
                                $"@{nameof(SchedulesForEventsEntity.CostPair)});" +
                                $"SELECT CAST(SCOPE_IDENTITY() AS INT)";
                            var insertedScheduleId = await conn.QuerySingleAsync<int>(sql, new { EventId = request.Event.Id, schedule.Description, schedule.StartDate, schedule.EndDate, schedule.CostMan, schedule.CostWoman, schedule.CostPair}, transaction: transaction);

                            if (schedule.Features != null)
                            {
                                foreach (var feature in schedule.Features)
                                {
                                    sql = $"INSERT INTO FeaturesForSchedules (" +
                                        $"{nameof(FeaturesForSchedulesEntity.ScheduleId)}, " +
                                        $"{nameof(FeaturesForSchedulesEntity.FeatureId)}" +
                                        $") VALUES (" +
                                        $"@{nameof(FeaturesForSchedulesEntity.ScheduleId)}, " +
                                        $"@{nameof(FeaturesForSchedulesEntity.FeatureId)})";
                                    result = await conn.ExecuteAsync(sql, new { ScheduleId = insertedScheduleId, FeatureId = feature.Id }, transaction: transaction);
                                }
                            }
                        }
                        else
                        {
                            sql = $"UPDATE SchedulesForEvents SET " +
                                $"{nameof(SchedulesForEventsEntity.Description)} = @{nameof(SchedulesForEventsEntity.Description)}, " +
                                $"{nameof(SchedulesForEventsEntity.StartDate)} = @{nameof(SchedulesForEventsEntity.StartDate)}, " +
                                $"{nameof(SchedulesForEventsEntity.EndDate)} = @{nameof(SchedulesForEventsEntity.EndDate)}, " +
                                $"{nameof(SchedulesForEventsEntity.CostMan)} = @{nameof(SchedulesForEventsEntity.CostMan)}, " +
                                $"{nameof(SchedulesForEventsEntity.CostWoman)} = @{nameof(SchedulesForEventsEntity.CostWoman)}, " +
                                $"{nameof(SchedulesForEventsEntity.CostPair)} = @{nameof(SchedulesForEventsEntity.CostPair)}, " +
                                $"{nameof(SchedulesForEventsEntity.IsDeleted)} = @{nameof(SchedulesForEventsEntity.IsDeleted)} " +
                                $"WHERE Id = @Id AND EventId = @{nameof(SchedulesForEventsEntity.EventId)}";
                            result = await conn.ExecuteAsync(sql, new { schedule.Id, EventId = request.Event.Id, schedule.Description, schedule.StartDate, schedule.EndDate, schedule.CostMan, schedule.CostWoman, schedule.CostPair, schedule.IsDeleted }, transaction: transaction);
                        }
                    }
                }

                transaction.Commit();
            }

            return response;
        }

    }
}
