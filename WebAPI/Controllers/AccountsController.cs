using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Enums;
using Dapper;
using DataContext.Entities;
using DataContext.Entities.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebAPI.Exceptions;
using WebAPI.Extensions;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : MyControllerBase
    {
        public AccountsController(IMapper mapper, IConfiguration configuration) : base(mapper, configuration) { }

        [Route("Get"), HttpPost]
        public async Task<GetAccountsResponseDto> GetAsync(GetAccountsRequestDto request)
        {
            var response = new GetAccountsResponseDto();

            var columns = GetRequiredColumns<AccountsViewEntity>();

            if (request.IsRelationsIncluded)
                columns.Add(nameof(AccountsViewEntity.Relations));

            if (request.IsHobbiesIncluded)
                columns.Add(nameof(AccountsViewEntity.Hobbies));

            if (request.IsPhotosIncluded)
                columns.Add(nameof(AccountsViewEntity.Photos));

            if (request.IsSchedulesIncluded)
                columns.Add(nameof(AccountsViewEntity.Schedules));

            string? where = null;

            // Получить одного пользователя
            if (request.Id.HasValue || request.Guid.HasValue)
            {
                if (request.Id.HasValue && request.Id > 0)
                    where = $"WHERE {nameof(AccountsViewEntity.Id)} = {request.Id}";
                else if (request.Guid.HasValue)
                    where = $"WHERE {nameof(AccountsViewEntity.Guid)} = '{request.Guid}'";

                var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} FROM AccountsView {where}";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<AccountsViewEntity>(sql);
                response.Account = _mapper.Map<AccountsViewDto>(result);
            }
            // Получить несколько пользователей
            else
            {
                string order = null!;

                switch (request.Order)
                {
                    case EnumOrders.IdDesc:
                        order = "ORDER BY Id DESC"; break;
                }

                string? limit = $"OFFSET {request.Skip} ROWS FETCH NEXT {request.Take} ROWS ONLY";

                var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} FROM AccountsView {where} {order} {limit}";
                var result = await _unitOfWork.SqlConnection.QueryAsync<AccountsViewEntity>(sql);
                response.Accounts = _mapper.Map<List<AccountsViewDto>>(result);
            }

            return response;
        }


        [Route("Login"), HttpPost]
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var response = new LoginResponseDto();

            if (request.Email == null || request.Password == null)
                return response;

            var sql = $"SELECT TOP 1 * FROM AccountsView " +
                $"WHERE {nameof(AccountsViewEntity.Email)} = @{nameof(AccountsViewEntity.Email)} AND {nameof(AccountsViewEntity.Password)} = @{nameof(AccountsViewEntity.Password)}";
            var account = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<AccountsViewEntity>(sql, new { request.Email, request.Password }) ?? throw new NotFoundException("Неверный логин и пароль!");
            response.Account = _mapper.Map<AccountsViewDto>(account);

            return response;
        }


        [Route("Reload"), HttpPost, Authorize]
        public async Task<AccountReloadResponseDto> ReloadAsync(AccountReloadRequestDto request)
        {
            AuthenticateUser();

            var response = new AccountReloadResponseDto();

            var sql = $"SELECT TOP 1 * FROM AccountsView WHERE Id = @AccountId";
            var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<AccountsViewEntity>(sql, new { _unitOfWork.AccountId }) ?? throw new NotFoundException($"Аккаунт с Id {_unitOfWork.AccountId} не найден!");
            response.Account = _mapper.Map<AccountsViewDto>(result);

            return response;
        }


        [Route("Register"), HttpPost]
        public async Task<ResponseDtoBase> RegisterAsync(AccountRegisterRequestDto request)
        {
            var response = new ResponseDtoBase();

            await request.ValidateAsync(_unitOfWork.SqlConnection);

            var accountsEntity = _mapper.Map<AccountsEntity>(request);

            // Регион
            var sql = "SELECT TOP 1 Id FROM Regions WHERE Id = @Id";
            var regionId = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.Country.Region.Id }) ?? throw new BadRequestException($"Указанный регион (id: {request.Country.Region.Id}) не найден в базе данных!");

            await _unitOfWork.BeginTransactionAsync();

            // Accounts
            sql = "INSERT INTO Accounts " +
                $"({nameof(AccountsEntity.Email)}, {nameof(AccountsEntity.Name)}, {nameof(AccountsEntity.Password)}, {nameof(AccountsEntity.Informing)}, {nameof(AccountsEntity.RegionId)}) " +
                "VALUES " +
                $"(@{nameof(AccountsEntity.Email)}, @{nameof(AccountsEntity.Name)}, @{nameof(AccountsEntity.Password)}, @{nameof(AccountsEntity.Informing)}, @{nameof(AccountsEntity.RegionId)}) " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT)";
            accountsEntity.Id = await _unitOfWork.SqlConnection.QuerySingleAsync<int>(sql, new { accountsEntity.Email, accountsEntity.Name, accountsEntity.Password, accountsEntity.Informing, accountsEntity.RegionId }, transaction: _unitOfWork.SqlTransaction);

            // Users
            var users = _mapper.Map<List<UsersEntity>>(request.Users);
            foreach (var u in users)
            {
                sql = "INSERT INTO Users " +
                    $"({nameof(UsersEntity.Name)}, {nameof(UsersEntity.Height)}, {nameof(UsersEntity.Weight)}, {nameof(UsersEntity.BirthDate)}, {nameof(UsersEntity.Gender)}, {nameof(UsersEntity.AccountId)}) " +
                    "VALUES " +
                    $"(@{nameof(UsersEntity.Name)}, @{nameof(UsersEntity.Height)}, @{nameof(UsersEntity.Weight)}, @{nameof(UsersEntity.BirthDate)}, @{nameof(UsersEntity.Gender)}, @{nameof(UsersEntity.AccountId)})";
                await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { u.Name, u.Height, u.Weight, u.BirthDate, u.Gender, AccountId = accountsEntity.Id }, transaction: _unitOfWork.SqlTransaction);
            }

            // AccountsWishList
            sql = $"INSERT INTO AccountsWishLists ({nameof(AccountsWishLists.Comment)}, {nameof(AccountsWishLists.AccountId)}) " +
                $"VALUES (@Comment, @AccountId)";
            await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { Comment = "Привет!", AccountId = accountsEntity.Id }, transaction: _unitOfWork.SqlTransaction);

            await _unitOfWork.CommitTransactionAsync();

            await accountsEntity.ProcessPhotoAfterRegistration(_unitOfWork, request);

            return response;
        }


        [Route("CheckRegister"), HttpPost]
        public async Task<AccountCheckRegisterResponseDto> CheckRegisterAsync(AccountCheckRegisterRequestDto request)
        {
            var response = new AccountCheckRegisterResponseDto();

            if (request.AccountName != null)
            {
                var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Name = @AccountName";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountName });
                response.AccountNameExists = result == null ? false : true;
            }

            if (request.AccountEmail != null)
            {
                var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Email = @AccountEmail";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountEmail });
                response.AccountEmailExists = result == null ? false : true;
            }
            return response;
        }


        [Route("CheckUpdate"), HttpPost, Authorize]
        public async Task<AccountCheckUpdateResponseDto> CheckUpdateAsync(AccountCheckUpdateRequestDto request)
        {
            AuthenticateUser();

            var response = new AccountCheckUpdateResponseDto();

            if (request.AccountName != null)
            {
                var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Name = @AccountName AND Id <> @AccountId";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountName, _unitOfWork.AccountId });
                response.AccountNameExists = result == null ? false : true;
            }

            if (request.AccountEmail != null)
            {
                var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Email = @AccountEmail AND Id <> @AccountId";
                var result = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountEmail, _unitOfWork.AccountId });
                response.AccountEmailExists = result == null ? false : true;
            }
            return response;
        }


        [Route("UpdateRelations"), HttpPost, Authorize]
        public async Task<RelationsUpdateResponseDto> UpdateRelationsAsync(RelationsUpdateRequestDto request)
        {
            AuthenticateUser();

            var sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @RecipientId";
            var recipientId = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.RecipientId }) ?? throw new BadRequestException($"Аккаунт с Id {request.RecipientId} не найден!");

            var model = new UpdateRelationModel
            {
                SenderId = _unitOfWork.AccountId!.Value,
                RecipientId = recipientId,
                Conn = _unitOfWork.SqlConnection,
                Response = new RelationsUpdateResponseDto()
            };

            switch (request.EnumRelation)
            {
                case EnumRelations.None:
                    await model.RemoveAllRelationsAsync(); break;

                case EnumRelations.Blocked:
                    await model.BlockUserAsync(); break;

                case EnumRelations.Subscriber:
                    await model.SubscribeUserAsync(); break;

                case EnumRelations.Friend:
                    await model.FriendshipUserAsync(); break;
            }

            return model.Response;
        }


        [Route("UpdateVisits"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> UpdateVisitsAsync(VisitsForAccountsUpdateRequestDto request)
        {
            AuthenticateUser();

            var response = new ResponseDtoBase();

            var sql = $"UPDATE VisitsForAccounts SET {nameof(VisitsForAccountsEntity.LastDate)} = getdate() " +
                $"WHERE {nameof(VisitsForAccountsEntity.AccountId)} = @AccountId";
            await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { _unitOfWork.AccountId });

            return response;
        }


        /// <summary>
        /// Регистрация пользователя на мероприятие (или отмена регистрации)
        /// </summary>
        [Route("RegistrationForEvent"), HttpPost, Authorize]
        public async Task<EventRegistrationResponseDto> RegistrationForEventAsync(EventRegistrationRequestDto request)
        {
            AuthenticateUser();

            // Получим тип учётки (пара, М или Ж)
            var sql = $"SELECT TOP (2) {nameof(UsersEntity.Gender)} FROM Users " +
                $"WHERE {nameof(UsersEntity.AccountId)} = {_unitOfWork.AccountId} AND {nameof(UsersEntity.IsDeleted)} = 0";
            var users = (await _unitOfWork.SqlConnection.QueryAsync<int>(sql)).ToList();
            if (users.Count == 0)
                throw new NotFoundException("Пользователь с указанным Id не найден!");
            int? AccountGender = null;
            if (users.Count() == 1)
                AccountGender = users[0];

            // Получим данные о расписании
            sql = $"SELECT * FROM SchedulesForEvents WHERE Id = {request.ScheduleId}";
            var evt = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<SchedulesForEventsEntity>(sql) ?? throw new NotFoundException("Указанное расписание события не найдено!");

            // Получим стоимость для учётки
            int TicketCost = AccountGender switch
            {
                0 => evt.CostMan!.Value,
                1 => evt.CostWoman!.Value,
                _ => evt.CostPair!.Value
            };

            sql = $"SELECT TOP (1) Id FROM SchedulesForAccounts WHERE {nameof(SchedulesForAccountsEntity.AccountId)} = @AccountId AND {nameof(SchedulesForAccountsEntity.ScheduleId)} = @ScheduleId AND IsDeleted = 0";
            var scheduleId = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { _unitOfWork.AccountId, request.ScheduleId });
            if (scheduleId == null)
            {
                sql = $"INSERT INTO SchedulesForAccounts " +
                    $"({nameof(SchedulesForAccountsEntity.ScheduleId)}, {nameof(SchedulesForAccountsEntity.AccountId)}, {nameof(SchedulesForAccountsEntity.AccountGender)}, {nameof(SchedulesForAccountsEntity.TicketCost)}) " +
                    $"VALUES (@{nameof(SchedulesForAccountsEntity.ScheduleId)}, @AccountId, @{nameof(SchedulesForAccountsEntity.AccountGender)}, @{nameof(SchedulesForAccountsEntity.TicketCost)})";
                await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { request.ScheduleId, _unitOfWork.AccountId, AccountGender, TicketCost });
            }
            else
            {
                sql = $"UPDATE SchedulesForAccounts SET {nameof(SchedulesForAccountsEntity.IsDeleted)} = 1 " +
                    $"WHERE {nameof(SchedulesForAccountsEntity.AccountId)} = @AccountId AND {nameof(SchedulesForAccountsEntity.ScheduleId)} = @ScheduleId";
                await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { request.ScheduleId, _unitOfWork.AccountId });
            }

            var response = new EventRegistrationResponseDto { ScheduleId = request.ScheduleId };
            return response;
        }


        [Route("GetHobbies"), HttpPost]
        public async Task<GetHobbiesResponseDto> GetHobbiesAsync(GetHobbiesRequestDto request)
        {
            var response = new GetHobbiesResponseDto();

            var result = await _unitOfWork.SqlConnection.QueryAsync<HobbiesEntity>("SELECT * FROM Hobbies ORDER BY Name ASC");
            response.Hobbies = _mapper.Map<List<HobbiesDto>>(result);

            return response;
        }


        /// <summary>
        /// Получить список друзей, подписчиков и т.п. указанного пользователя
        /// </summary>
        [Route("GetRelations"), HttpPost]
        public async Task<GetAccountsResponseDto> GetRelationsAsync(GetRelationsForAccountsRequestDto request)
        {
            var response = new GetAccountsResponseDto();

            var columns = GetRequiredColumns<AccountsViewEntity>();
            columns.Add(nameof(AccountsViewEntity.Avatar));

            string sql;

            switch(request.Relation)
            {
                case EnumRelations.Friend:
                    sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} FROM AccountsView " +
                        $"WHERE Id IN (" +
                        $"SELECT {nameof(RelationsForAccountsEntity.RecipientId)} FROM RelationsForAccounts WHERE {nameof(RelationsForAccountsEntity.SenderId)} = @AccountId AND Type = @Relation AND IsConfirmed = 1 " +
                        $"UNION " +
                        $"SELECT {nameof(RelationsForAccountsEntity.SenderId)} FROM RelationsForAccounts WHERE {nameof(RelationsForAccountsEntity.RecipientId)} = @AccountId AND Type = @Relation AND IsConfirmed = 1)";
                    break;

                case EnumRelations.Subscriber:
                    sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} FROM AccountsView " +
                        $"WHERE Id IN (" +
                        $"SELECT {nameof(RelationsForAccountsEntity.SenderId)} FROM RelationsForAccounts WHERE {nameof(RelationsForAccountsEntity.RecipientId)} = @AccountId AND Type = @Relation AND IsConfirmed = 1)";
                    break;

                default:
                    throw new BadRequestException($"Неверно указан тип взаимосвязи: {request.Relation}!");
            }

            var result = await _unitOfWork.SqlConnection.QueryAsync<AccountsViewEntity>(sql, new { request.AccountId, Relation = (int)request.Relation });
            response.Accounts = _mapper.Map<List<AccountsViewDto>>(result);

            return response;
        }


        [Route("GetWishList"), HttpPost]
        public async Task<GetWishListResponseDto> GetWishListAsync(GetWishListRequestDto request)
        {
            var response = new GetWishListResponseDto();

            var result = await _unitOfWork.SqlConnection.QueryAsync<WishListViewEntity>("SELECT * FROM WishListView");
            response.WishList = _mapper.Map<List<WishListViewDto>>(result);

            return response;
        }

        [Route("Update"), HttpPost, Authorize]
        public async Task<UpdateAccountResponseDto> UpdateAsync(UpdateAccountRequestDto request)
        {
            AuthenticateUser();

            var response = new UpdateAccountResponseDto();

            await request.ValidateAsync(_unitOfWork.AccountId!.Value, _unitOfWork.SqlConnection);

            await _unitOfWork.BeginTransactionAsync();

            // Обновление Users
            await request.UpdateUsersAsync(_unitOfWork);

            // Обновление HobbiesForAccounts
            await request.UpdateHobbiesAsync(_unitOfWork);

            // Обновление Accounts
            await request.UpdateAccountAsync(_unitOfWork);

            // Обновление пароля
            await request.UpdatePasswordAsync(_unitOfWork);

            // Обновление фото аккаунта
            await request.UpdatePhotosAsync(_unitOfWork);

            await _unitOfWork.CommitTransactionAsync();

            // Вернём для дальнейшего вызова AccountLogin, чтобы в UI Storage обновить данные пользователя
            response.Email = request.Email;
            response.Password = request.Password; // Вернёт null, если новый пароль не был указан в запросе

            return response;
        }


    }
}
