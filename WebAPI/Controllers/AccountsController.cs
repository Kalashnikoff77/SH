﻿using AutoMapper;
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
using Microsoft.Data.SqlClient;
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

            using (var conn = new SqlConnection(connectionString))
            {
                // Получить одного пользователя
                if (request.Id.HasValue || request.Guid.HasValue)
                {
                    if (request.Id.HasValue && request.Id > 0)
                        where = $"WHERE {nameof(AccountsViewEntity.Id)} = {request.Id}";
                    else if (request.Guid.HasValue)
                        where = $"WHERE {nameof(AccountsViewEntity.Guid)} = '{request.Guid}'";

                    var sql = $"SELECT {columns.Aggregate((a, b) => a + ", " + b)} FROM AccountsView {where}";
                    var result = await conn.QueryFirstOrDefaultAsync<AccountsViewEntity>(sql);
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
                    var result = await conn.QueryAsync<AccountsViewEntity>(sql);
                    response.Accounts = _mapper.Map<List<AccountsViewDto>>(result);
                }
            }

            return response;
        }


        [Route("Login"), HttpPost]
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var response = new LoginResponseDto();

            if (request.Email == null || request.Password == null)
                return response;

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT TOP 1 * FROM AccountsView " +
                    $"WHERE {nameof(AccountsViewEntity.Email)} = @{nameof(AccountsViewEntity.Email)} AND {nameof(AccountsViewEntity.Password)} = @{nameof(AccountsViewEntity.Password)}";
                var account = await conn.QueryFirstOrDefaultAsync<AccountsViewEntity>(sql, new { request.Email, request.Password }) ?? throw new NotFoundException("Неверный логин и пароль!");

                response.Account = _mapper.Map<AccountsViewDto>(account);

                return response;
            }
        }


        [Route("Reload"), HttpPost, Authorize]
        public async Task<AccountReloadResponseDto> ReloadAsync(AccountReloadRequestDto request)
        {
            AuthenticateUser();

            var response = new AccountReloadResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"SELECT TOP 1 * FROM AccountsView WHERE Id = @_accountId";
                var result = await conn.QueryFirstOrDefaultAsync<AccountsViewEntity>(sql, new { _accountId }) ?? throw new NotFoundException($"Аккаунт с Id {_accountId} не найден!");

                response.Account = _mapper.Map<AccountsViewDto>(result);

                return response;
            }
        }


        [Route("Register"), HttpPost]
        public async Task<ResponseDtoBase> RegisterAsync(AccountRegisterRequestDto request)
        {
            var response = new ResponseDtoBase();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                await request.ValidateAsync(conn);

                var accountsEntity = _mapper.Map<AccountsEntity>(request);

                // Регион
                var sql = "SELECT TOP 1 Id FROM Regions WHERE Id = @Id";
                var regionId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.Country.Region.Id }) ?? throw new BadRequestException($"Указанный регион (id: {request.Country.Region.Id}) не найден в базе данных!");

                using var transaction = conn.BeginTransaction();

                // Accounts
                sql = "INSERT INTO Accounts " +
                    $"({nameof(AccountsEntity.Email)}, {nameof(AccountsEntity.Name)}, {nameof(AccountsEntity.Password)}, {nameof(AccountsEntity.Informing)}, {nameof(AccountsEntity.RegionId)}) " +
                    "VALUES " +
                    $"(@{nameof(AccountsEntity.Email)}, @{nameof(AccountsEntity.Name)}, @{nameof(AccountsEntity.Password)}, @{nameof(AccountsEntity.Informing)}, @{nameof(AccountsEntity.RegionId)}) " +
                    "SELECT CAST(SCOPE_IDENTITY() AS INT)";
                accountsEntity.Id = await conn.QuerySingleAsync<int>(sql, new { accountsEntity.Email, accountsEntity.Name, accountsEntity.Password, accountsEntity.Informing, accountsEntity.RegionId }, transaction: transaction);

                // Users
                var users = _mapper.Map<List<UsersEntity>>(request.Users);
                foreach (var u in users)
                {
                    sql = "INSERT INTO Users " +
                        $"({nameof(UsersEntity.Name)}, {nameof(UsersEntity.Height)}, {nameof(UsersEntity.Weight)}, {nameof(UsersEntity.BirthDate)}, {nameof(UsersEntity.Gender)}, {nameof(UsersEntity.AccountId)}) " +
                        "VALUES " +
                        $"(@{nameof(UsersEntity.Name)}, @{nameof(UsersEntity.Height)}, @{nameof(UsersEntity.Weight)}, @{nameof(UsersEntity.BirthDate)}, @{nameof(UsersEntity.Gender)}, @{nameof(UsersEntity.AccountId)})";
                    await conn.ExecuteAsync(sql, new { u.Name, u.Height, u.Weight, u.BirthDate, u.Gender, AccountId = accountsEntity.Id }, transaction: transaction);
                }

                // AccountsWishList
                sql = $"INSERT INTO AccountsWishLists ({nameof(AccountsWishLists.Comment)}, {nameof(AccountsWishLists.AccountId)}) " +
                    $"VALUES (@Comment, @AccountId)";
                await conn.ExecuteAsync(sql, new { Comment = "Привет!", AccountId = accountsEntity.Id }, transaction: transaction);

                transaction.Commit();

                await accountsEntity.ProcessPhotoAfterRegistration(conn, request);
            }

            return response;
        }


        [Route("CheckRegister"), HttpPost]
        public async Task<AccountCheckRegisterResponseDto> CheckRegisterAsync(AccountCheckRegisterRequestDto request)
        {
            var response = new AccountCheckRegisterResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (request.AccountName != null)
                {
                    var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Name = @AccountName";
                    var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountName });
                    response.AccountNameExists = result == null ? false : true;
                }

                if (request.AccountEmail != null)
                {
                    var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Email = @AccountEmail";
                    var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountEmail });
                    response.AccountEmailExists = result == null ? false : true;
                }
            }
            return response;
        }


        [Route("Update"), HttpPost, Authorize]
        public async Task<AccountUpdateResponseDto> UpdateAsync(AccountUpdateRequestDto request)
        {
            AuthenticateUser();

            var response = new AccountUpdateResponseDto();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                await request.ValidateAsync(_accountId, conn);

                // Получим данные о текущем пользователе из базы
                var columns = GetRequiredColumns<AccountsViewEntity>();
                var sql = $"SELECT TOP 1 {columns.Aggregate((a, b) => a + ", " + b)}, {nameof(AccountsViewEntity.Password)}, {nameof(AccountsViewEntity.Users)} " +
                    "FROM AccountsView WHERE Id = @_accountId";
                var accountsViewEntity = await conn.QueryFirstOrDefaultAsync<AccountsViewEntity>(sql, new { _accountId }) ?? throw new BadRequestException($"Аккаунт {request.Name} не найден!");
                var accountsView = _mapper.Map<AccountsViewDto>(accountsViewEntity);

                using var transaction = conn.BeginTransaction();

                // Обновление Users
                foreach (var user in request.Users)
                {
                    // Добавление
                    if (user.Id == -1)
                    {
                        sql = $"INSERT INTO Users ({nameof(UsersEntity.Name)}, {nameof(UsersEntity.Height)}, {nameof(UsersEntity.Weight)}, {nameof(UsersEntity.BirthDate)}, {nameof(UsersEntity.About)}, {nameof(UsersEntity.Gender)}, {nameof(UsersEntity.AccountId)}) " +
                            "VALUES " +
                            $"(@{nameof(UsersEntity.Name)}, @{nameof(UsersEntity.Height)}, @{nameof(UsersEntity.Weight)}, @{nameof(UsersEntity.BirthDate)}, @{nameof(UsersEntity.About)}, @{nameof(UsersEntity.Gender)}, @_accountId)";
                        await conn.ExecuteAsync(sql, new { user.Name, user.Height, user.Weight, user.BirthDate, user.About, user.Gender, _accountId }, transaction: transaction);
                    }
                    // Обновление / Удаление
                    else
                    {
                        sql = $"UPDATE Users SET " +
                            $"{nameof(UsersEntity.BirthDate)} = @{nameof(user.BirthDate)}, " +
                            $"{nameof(UsersEntity.Name)} = @{nameof(user.Name)}, " +
                            $"{nameof(UsersEntity.Gender)} = @{nameof(user.Gender)}, " +
                            $"{nameof(UsersEntity.Height)} = @{nameof(user.Height)}, " +
                            $"{nameof(UsersEntity.Weight)} = @{nameof(user.Weight)}, " +
                            $"{nameof(UsersEntity.About)} = @{nameof(user.About)}, " +
                            $"{nameof(UsersEntity.IsDeleted)} = @{nameof(user.IsDeleted)} " +
                            $"WHERE Id = @Id AND AccountId = @_accountId";
                        await conn.ExecuteAsync(sql, new { user.Id, _accountId, user.BirthDate, user.Name, user.Gender, user.Height, user.Weight, user.About, user.IsDeleted }, transaction: transaction);
                    }
                }

                // Обновление HobbiesForAccounts
                if (request.Hobbies != null)
                {
                    var p = new DynamicParameters();
                    p.Add("@AccountId", _accountId);
                    p.Add("@HobbiesIds", string.Join(",", request.Hobbies.Select(s => s.Id)));
                    await conn.ExecuteAsync("UpdateHobbiesForAccounts_sp", p, commandType: System.Data.CommandType.StoredProcedure, transaction: transaction);
                }

                // Обновление Accounts
                sql = $"UPDATE Accounts SET " +
                    $"{nameof(AccountsEntity.Email)} = @{nameof(request.Email)}, " +
                    $"{nameof(AccountsEntity.Name)} = @{nameof(request.Name)}, " +
                    $"{nameof(AccountsEntity.Informing)} = @informing, " +
                    $"{nameof(AccountsEntity.RegionId)} = @{nameof(AccountsEntity.RegionId)} " +
                    "WHERE Id = @_accountId";
                await conn.ExecuteAsync(sql, new { request.Email, request.Name, informing = JsonSerializer.Serialize(request.Informing), RegionId = request.Country.Region.Id, _accountId }, transaction: transaction);

                // Обновление пароля
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    sql = $"UPDATE Accounts SET {nameof(AccountsEntity.Password)} = @{nameof(AccountsEntity.Password)} WHERE {nameof(AccountsEntity.Id)} = @_accountId";
                    await conn.ExecuteAsync(sql, new {Password = request.Password2, _accountId}, transaction: transaction);
                }

                transaction.Commit();

                // Вернём для дальнейшего вызова AccountLogin, чтобы в UI Storage обновить данные пользователя
                response.Email = request.Email;
                response.Password = request.Password; // Вернёт null, если новый пароль не был указан в запросе

                return response;
            }
        }


        [Route("CheckUpdate"), HttpPost, Authorize]
        public async Task<AccountCheckUpdateResponseDto> CheckUpdateAsync(AccountCheckUpdateRequestDto request)
        {
            AuthenticateUser();

            var response = new AccountCheckUpdateResponseDto();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                if (request.AccountName != null)
                {
                    var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Name = @AccountName AND Id <> @_accountId";
                    var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountName, _accountId });
                    response.AccountNameExists = result == null ? false : true;
                }

                if (request.AccountEmail != null)
                {
                    var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Email = @AccountEmail AND Id <> @_accountId";
                    var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.AccountEmail, _accountId });
                    response.AccountEmailExists = result == null ? false : true;
                }
            }
            return response;
        }


        [Route("UpdateRelations"), HttpPost, Authorize]
        public async Task<RelationsUpdateResponseDto> UpdateRelationsAsync(RelationsUpdateRequestDto request)
        {
            AuthenticateUser();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @RecipientId";
                var recipientId = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.RecipientId }) ?? throw new BadRequestException($"Аккаунт с Id {request.RecipientId} не найден!");

                var model = new UpdateRelationModel
                {
                    SenderId = _accountId,
                    RecipientId = recipientId,
                    Conn = conn,
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
        }


        [Route("UpdateVisits"), HttpPost, Authorize]
        public async Task<ResponseDtoBase> UpdateVisitsAsync(VisitsForAccountsUpdateRequestDto request)
        {
            AuthenticateUser();

            var response = new ResponseDtoBase();

            using (var conn = new SqlConnection(connectionString))
            {
                var sql = $"UPDATE VisitsForAccounts SET {nameof(VisitsForAccountsEntity.LastDate)} = getdate() " +
                    $"WHERE {nameof(VisitsForAccountsEntity.AccountId)} = @_accountId";
                await conn.ExecuteAsync(sql, new { _accountId });
            }
            return response;
        }


        /// <summary>
        /// Регистрация пользователя на мероприятие (или отмена регистрации)
        /// </summary>
        [Route("EventRegistration"), HttpPost, Authorize]
        public async Task<EventRegistrationResponseDto> EventRegistrationAsync(EventRegistrationRequestDto request)
        {
            AuthenticateUser();

            using (var conn = new SqlConnection(connectionString))
            {
                // Получим тип учётки (пара, М или Ж)
                var sql = $"SELECT TOP (2) {nameof(UsersEntity.Gender)} FROM Users " +
                    $"WHERE {nameof(UsersEntity.AccountId)} = {_accountId} AND {nameof(UsersEntity.IsDeleted)} = 0";
                var users = (await conn.QueryAsync<int>(sql)).ToList();
                if (users.Count == 0)
                    throw new NotFoundException("Пользователь с указанным Id не найден!");
                int? AccountGender = null;
                if (users.Count() == 1)
                    AccountGender = users[0];

                // Получим данные о расписании
                sql = $"SELECT * FROM SchedulesForEvents WHERE Id = {request.ScheduleId}";
                var evt = await conn.QueryFirstOrDefaultAsync<SchedulesForEventsEntity>(sql) ?? throw new NotFoundException("Указанное расписание события не найдено!");

                // Получим стоимость для учётки
                int TicketCost = AccountGender switch
                {
                    0 => evt.CostMan!.Value,
                    1 => evt.CostWoman!.Value,
                    _ => evt.CostPair!.Value
                };

                sql = $"SELECT TOP (1) Id FROM SchedulesForAccounts WHERE {nameof(SchedulesForAccountsEntity.AccountId)} = @_accountId AND {nameof(SchedulesForAccountsEntity.ScheduleId)} = @ScheduleId AND IsDeleted = 0";
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

                var response = new EventRegistrationResponseDto { ScheduleId = request.ScheduleId };
                return response;
            }
        }


        [Route("GetHobbies"), HttpPost]
        public async Task<GetHobbiesResponseDto> GetHobbiesAsync(GetHobbiesRequestDto request)
        {
            var response = new GetHobbiesResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var result = await conn.QueryAsync<HobbiesEntity>("SELECT * FROM Hobbies ORDER BY Name ASC");
                response.Hobbies = _mapper.Map<List<HobbiesDto>>(result);
                return response;
            }
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

            string sql = null!;

            using (var conn = new SqlConnection(connectionString))
            {
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

                var result = await conn.QueryAsync<AccountsViewEntity>(sql, new { request.AccountId, Relation = (int)request.Relation });

                response.Accounts = _mapper.Map<List<AccountsViewDto>>(result);
            }

            return response;
        }


        [Route("GetWishList"), HttpPost]
        public async Task<GetWishListResponseDto> GetWishListAsync(GetWishListRequestDto request)
        {
            var response = new GetWishListResponseDto();

            using (var conn = new SqlConnection(connectionString))
            {
                var result = await conn.QueryAsync<WishListViewEntity>("SELECT * FROM WishListView");

                response.WishList = _mapper.Map<List<WishListViewDto>>(result);

                return response;
            }
        }
    }
}
