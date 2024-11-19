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
using System.Text.Json;
using WebAPI.Exceptions;
using WebAPI.Extensions;

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

            if (request.EventId != null)
            {
                var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                    $"FROM EventsView " +
                    $"WHERE Id = @EventId";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<EventsViewEntity>(sql, new { request.EventId }) ?? throw new NotFoundException("Встреча не найдена!");
                response.Event = _mapper.Map<EventsViewDto>(result);
            }
            return response;
        }


        [Route("GetSchedules"), HttpPost]
        public async Task<GetSchedulesResponseDto?> GetSchedulesAsync(GetSchedulesRequestDto request)
        {
            var response = new GetSchedulesResponseDto();

            var columns = GetRequiredColumns<SchedulesForEventsViewEntity>();

            // Получить одну запись
            if (request.ScheduleId.HasValue && request.ScheduleId > 0)
            {
                var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                    $"FROM SchedulesForEventsView " +
                    $"WHERE Id = @ScheduleId";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<SchedulesForEventsViewEntity>(sql, new { request.ScheduleId }) ?? throw new NotFoundException("Встреча не найдена!");
                response.Schedule = _mapper.Map<SchedulesForEventsViewDto>(result);
            }

            // Получить все расписания определённого мероприятия
            else if (request.EventId.HasValue && request.EventId > 0)
            {
                var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                    $"FROM SchedulesForEventsView " +
                    $"WHERE EventId = @EventId";
                var result = await _unitOfWork.SqlConnection.QueryAsync<SchedulesForEventsViewEntity>(sql, new { request.EventId });
                response.Schedules = _mapper.Map<List<SchedulesForEventsViewDto>>(result);
            }

            // Получить несколько записей
            else
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                // Сперва получим Id записей, которые нужно вытянуть + кол-во записей.
                var p = new DynamicParameters();
                p.Add("@GetEventsRequestDto", jsonRequest);
                var ids = await _unitOfWork.SqlConnection.QueryAsync<int>("EventsFilter_sp", p, commandType: System.Data.CommandType.StoredProcedure);
                response.Count = ids.Count();

                if (response.Count > 0)
                {
                    string order = request.IsActualEvents ? $"ORDER BY {nameof(SchedulesForEventsViewDto.StartDate)} " : $"ORDER BY {nameof(SchedulesForEventsViewDto.EndDate)} DESC ";

                    string sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} " +
                        $"FROM SchedulesForEventsView " +
                        $"WHERE Id IN ({string.Join(",", ids)}) " +
                        order +
                        $"OFFSET {request.Skip} ROWS FETCH NEXT {request.Take} ROWS ONLY";

                    var result = await _unitOfWork.SqlConnection.QueryAsync<SchedulesForEventsViewEntity>(sql);
                    response.Schedules = _mapper.Map<List<SchedulesForEventsViewDto>>(result);
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

                // Получим кол-во сообщений в чате мероприятия
                var sql = $"SELECT COUNT(*) FROM DiscussionsForEvents " +
                    $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)}";
                response.NumOfDiscussions = await _unitOfWork.SqlConnection.QuerySingleAsync<int>(sql, new { request.EventId });

                // Запрос на получение предыдущих сообщений
                if (request.GetPreviousFromId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM DiscussionsForEventsView " +
                            $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)} " +
                            $"AND Id < {request.GetPreviousFromId} " +
                            $"ORDER BY Id DESC";
                    result = (await _unitOfWork.SqlConnection.QueryAsync<DiscussionsForEventsViewEntity>(sql, new { request.EventId, request.Take })).Reverse();
                }

                // Запрос на получение следующих сообщений
                else if (request.GetNextAfterId.HasValue)
                {
                    sql = $"SELECT TOP (@Take) * FROM DiscussionsForEventsView " +
                            $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)} " +
                            $"AND Id > {request.GetNextAfterId} " +
                            $"ORDER BY Id ASC";
                    result = await _unitOfWork.SqlConnection.QueryAsync<DiscussionsForEventsViewEntity>(sql, new { request.EventId, request.Take });
                }

                // Запрос на получение последних сообщений (по умолчанию)
                else
                {
                    sql = $"SELECT TOP (@Take) * FROM DiscussionsForEventsView " +
                        $"WHERE {nameof(DiscussionsForEventsViewEntity.EventId)} = @{nameof(DiscussionsForEventsViewEntity.EventId)} " +
                        $"ORDER BY Id DESC";
                    result = (await _unitOfWork.SqlConnection.QueryAsync<DiscussionsForEventsViewEntity>(sql, new { request.EventId, request.Take })).Reverse();
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

            var sql = $"INSERT INTO DiscussionsForEvents " +
                $"({nameof(DiscussionsForEventsEntity.EventId)}, {nameof(DiscussionsForEventsEntity.SenderId)}, {nameof(DiscussionsForEventsEntity.RecipientId)}, {nameof(DiscussionsForEventsEntity.DiscussionId)}, {nameof(DiscussionsForEventsEntity.Text)}) " +
                $"OUTPUT INSERTED.Id " +
                $"VALUES " +
                $"(@{nameof(DiscussionsForEventsEntity.EventId)}, @AccountId, @{nameof(DiscussionsForEventsEntity.RecipientId)}, @{nameof(DiscussionsForEventsEntity.DiscussionId)}, @{nameof(DiscussionsForEventsEntity.Text)})";
            var newId = await _unitOfWork.SqlConnection.QuerySingleAsync<int>(sql, new { request.EventId, _unitOfWork.AccountId, request.RecipientId, request.DiscussionId, request.Text });

            return new AddDiscussionsForEventsResponseDto { NewDiscussionId = newId };
        }


        /// <summary>
        /// Проверка при добавлении или обновлении мероприятия
        /// </summary>
        [Route("Check"), HttpPost, Authorize]
        public async Task<EventCheckResponseDto> CheckAsync(EventCheckRequestDto request)
        {
            var response = new EventCheckResponseDto();

            if (request.EventName != null)
            {
                string sql;
                if (request.EventId.HasValue)
                    sql = $"SELECT TOP 1 Id FROM Events WHERE Name = @EventName AND Id <> @EventId";
                else
                    sql = $"SELECT TOP 1 Id FROM Events WHERE Name = @EventName";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.EventId, request.EventName });
                response.EventNameExists = result == null ? false : true;
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

            var sql = "SELECT * FROM AdminsForEventsView ORDER BY Name";
            var result = await _unitOfWork.SqlConnection.QueryAsync<AdminsForEventsViewEntity>(sql);
            response.AdminsForEvents = _mapper.Map<List<AdminsForEventsViewDto>>(result);

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

            var sql = $"SELECT * FROM Features ORDER BY Name";
            var result = await _unitOfWork.SqlConnection.QueryAsync<FeaturesEntity>(sql);
            response.Features = _mapper.Map<List<FeaturesDto>>(result);

            return response;
        }


        /// <summary>
        /// Получить список всех features, которые фигурируют в мероприятиях плюс кол-во мероприятий, в которых они фигурируют
        /// </summary>
        [Route("GetFeaturesForEvents"), HttpPost]
        public async Task<GetFeaturesForEventsResponseDto> GetFeaturesForEventsAsync(GetFeaturesForEventsRequestDto request)
        {
            var response = new GetFeaturesForEventsResponseDto();

            var sql = $"SELECT * FROM FeaturesForEventsView";
            var result = await _unitOfWork.SqlConnection.QueryAsync<FeaturesForEventsViewEntity>(sql);
            response.FeaturesForEvents = _mapper.Map<List<FeaturesForEventsViewDto>>(result);

            return response;
        }

        /// <summary>
        /// Добавление мероприятия
        /// </summary>
        [Route("Add"), HttpPost, Authorize]
        public async Task<AddEventResponseDto> AddAsync(AddEventRequestDto request)
        {
            AuthenticateUser();

            var response = new AddEventResponseDto();

            // Проверка
            request.Validate(_unitOfWork);

            await _unitOfWork.BeginTransactionAsync();

            // Добавление мероприятия
            request.Event.Id = await request.AddEventAsync(_unitOfWork);

            //// Добавление расписаний мероприятия
            await request.AddSchedulesAsync(_unitOfWork);

            //// Добавление фото меропириятия
            await request.AddPhotosAsync(_unitOfWork);

            await _unitOfWork.CommitTransactionAsync();

            response.NewEventId = request.Event.Id;
            return response;
        }


        /// <summary>
        /// Обновление мероприятия
        /// </summary>
        [Route("Update"), HttpPost, Authorize]
        public async Task<UpdateEventResponseDto> UpdateAsync(UpdateEventRequestDto request)
        {
            AuthenticateUser();

            // Проверка
            request.Validate(_unitOfWork);

            var response = new UpdateEventResponseDto();

            await _unitOfWork.BeginTransactionAsync();

            // Обновление мероприятия
            await request.UpdateEventAsync(_unitOfWork);

            // Обновление расписаний мероприятия
            await request.UpdateSchedulesAsync(_unitOfWork);

            // Обновление фото меропириятия
            await request.UpdatePhotosAsync(_unitOfWork);

            await _unitOfWork.CommitTransactionAsync();

            return response;
        }
    }
}
