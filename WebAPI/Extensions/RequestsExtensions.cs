﻿using Common.Dto.Requests;
using Common.Models;
using Dapper;
using DataContext.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.Text.RegularExpressions;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Models;

namespace WebAPI.Extensions
{
    public static class RequestsExtensions
    {
        public static async Task ValidateAsync(this AccountUpdateRequestDto request, int _accountId, SqlConnection conn)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new BadRequestException("Укажите Ваш email!");

            if (!Regex.IsMatch(request.Email, @"^[a-z0-9_\.-]{1,32}@[a-z0-9\.-]{1,32}\.[a-z]{2,8}$"))
                throw new BadRequestException("Проверьте корректность email!");

            if (request.Email.Length < StaticData.DB_ACCOUNTS_EMAIL_MIN|| request.Email.Length > StaticData.DB_ACCOUNTS_EMAIL_MAX)
                throw new BadRequestException($"Длина email должна быть от {StaticData.DB_ACCOUNTS_EMAIL_MIN} до {StaticData.DB_ACCOUNTS_EMAIL_MAX} символов!");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Заполните имя учётной записи!");

            if (request.Name.Length < StaticData.DB_ACCOUNTS_NAME_MIN || request.Name.Length > StaticData.DB_ACCOUNTS_NAME_MAX)
                throw new BadRequestException($"Длина имени должна быть от {StaticData.DB_ACCOUNTS_NAME_MIN} до {StaticData.DB_ACCOUNTS_NAME_MAX} символов!");

            if (request.Users == null || request.Users.Where(u => u.IsDeleted == false).Count() == 0)
                throw new BadRequestException("Вы не добавили ни одного партнёра в аккаунт!");

            if (request.Users != null && request.Users.Where(w => w.IsDeleted == false).Count() > 2)
                throw new BadRequestException("Можно добавить не более 2-х партнёров!");

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                if (request.Password != request.Password2)
                    throw new BadRequestException("Пароль и повтор пароля не совпадают!");
            }

            foreach (var user in request.Users!.Where(u => u.IsDeleted == false))
            {
                if (string.IsNullOrWhiteSpace(user.Name))
                    throw new BadRequestException("Не указано имя у одного из партнёров!");

                if (user.Name.Length < StaticData.DB_USERS_NAME_MIN || user.Name.Length > StaticData.DB_USERS_NAME_MAX)
                    throw new BadRequestException($"Длина имени у {user.Name} должна быть от {StaticData.DB_USERS_NAME_MIN} до {StaticData.DB_USERS_NAME_MAX} символов!");

                if (user.Height < StaticData.DB_USERS_HEIGHT_MIN || user.Height > StaticData.DB_USERS_HEIGHT_MAX)
                    throw new BadRequestException($"Рост у {user.Name} должна быть от {StaticData.DB_USERS_HEIGHT_MIN} до {StaticData.DB_USERS_HEIGHT_MAX} см!");

                if (user.Weight < StaticData.DB_USERS_WEIGHT_MIN || user.Weight > StaticData.DB_USERS_WEIGHT_MAX)
                    throw new BadRequestException($"Вес у {user.Name} должен быть от {StaticData.DB_USERS_WEIGHT_MIN} до {StaticData.DB_USERS_WEIGHT_MAX} кг!");

                if (user.Gender < 0 || user.Gender > 1)
                    throw new BadRequestException($"Укажите пол у {user.Name}!");

                if (user.BirthDate.Date < DateTime.Now.AddYears(-75).Date || user.BirthDate.Date > DateTime.Now.AddYears(-20).Date)
                    throw new BadRequestException($"Возраст у {user.Name} должен быть от 20 до 75 лет!");
            }

            if (request.Country == null || request.Country.Region == null || request.Country.Region.Id == 0)
                throw new BadRequestException("Вы не указали регион проживания!");

            var sql = $"SELECT TOP 1 Id FROM Accounts WHERE Email = @Email AND Id <> @_accountId";
            if ((await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.Email, _accountId })) != null)
                throw new BadRequestException($"Аккаунт с email {request.Email} уже зарегистрирован!");

            sql = $"SELECT TOP 1 Id FROM Accounts WHERE Name = @Name AND Id <> @_accountId";
            if ((await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.Name, _accountId })) != null)
                throw new BadRequestException($"Аккаунт с именем {request.Name} уже зарегистрирован!");
        }


        public static async Task ValidateAsync(this AccountRegisterRequestDto request, SqlConnection conn)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new BadRequestException("Укажите Ваш email!");

            if (!Regex.IsMatch(request.Email, @"^[a-z0-9_\.-]{1,32}@[a-z0-9\.-]{1,32}\.[a-z]{2,8}$"))
                throw new BadRequestException("Проверьте корректность email!");

            if (request.Email.Length < StaticData.DB_ACCOUNTS_EMAIL_MIN || request.Email.Length > StaticData.DB_ACCOUNTS_EMAIL_MAX)
                throw new BadRequestException($"Длина email должна быть от {StaticData.DB_ACCOUNTS_EMAIL_MIN} до {StaticData.DB_ACCOUNTS_EMAIL_MAX} символов!");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Заполните имя учётной записи!");

            if (request.Name.Length < StaticData.DB_ACCOUNTS_NAME_MIN || request.Name.Length > StaticData.DB_ACCOUNTS_NAME_MAX)
                throw new BadRequestException($"Длина имени должна быть от {StaticData.DB_ACCOUNTS_NAME_MIN} до {StaticData.DB_ACCOUNTS_NAME_MAX} символов!");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new BadRequestException("Заполните поле с паролем!");

            if (string.IsNullOrWhiteSpace(request.Password2))
                throw new BadRequestException("Заполните поле с дубликатом пароля!");

            if (request.Password.Length < StaticData.DB_ACCOUNTS_PASSWORD_MIN || request.Password.Length > StaticData.DB_ACCOUNTS_PASSWORD_MAX)
                throw new BadRequestException($"Длина пароля должна быть от {StaticData.DB_ACCOUNTS_PASSWORD_MIN} до {StaticData.DB_ACCOUNTS_PASSWORD_MAX} символов!");

            if (request.Password != request.Password2)
                throw new BadRequestException("Пароли не совпадают!");

            if (request.Country == null || request.Country.Region == null || request.Country.Region.Id == 0)
                throw new BadRequestException("Вы не указали регион проживания!");

            if (request.Users != null && request.Users.Count > 2)
                throw new BadRequestException("Можно добавить не более 2-х партнёров!");

            if (request.Users == null || request.Users.Count == 0)
                throw new BadRequestException("Вы не добавили ни одного партнёра в аккаунт!");

            foreach (var user in request.Users)
            {
                if (string.IsNullOrWhiteSpace(user.Name))
                    throw new BadRequestException("Не указано имя у одного из партнёров!");

                if (user.Name.Length < StaticData.DB_USERS_NAME_MIN || user.Name.Length > StaticData.DB_USERS_NAME_MAX)
                    throw new BadRequestException($"Длина имени у {user.Name} должна быть от {StaticData.DB_USERS_NAME_MIN} до {StaticData.DB_USERS_NAME_MAX} символов!");

                if (user.Height < StaticData.DB_USERS_HEIGHT_MIN || user.Height > StaticData.DB_USERS_HEIGHT_MAX)
                    throw new BadRequestException($"Рост у {user.Name} должна быть от {StaticData.DB_USERS_HEIGHT_MIN} до {StaticData.DB_USERS_HEIGHT_MAX} см!");

                if (user.Weight < StaticData.DB_USERS_WEIGHT_MIN || user.Weight > StaticData.DB_USERS_WEIGHT_MAX)
                    throw new BadRequestException($"Вес у {user.Name} должен быть от {StaticData.DB_USERS_WEIGHT_MIN} до {StaticData.DB_USERS_WEIGHT_MAX} кг!");

                if (user.Gender < 0 || user.Gender > 1)
                    throw new BadRequestException($"Укажите пол у {user.Name}!");

                if (user.BirthDate.Date < DateTime.Now.AddYears(-75).Date || user.BirthDate.Date > DateTime.Now.AddYears(-20).Date)
                    throw new BadRequestException($"Возраст у {user.Name} должен быть от 20 до 75 лет!");
            }

            var sql = "SELECT TOP 1 Id FROM Accounts WHERE Email = @Email";
            var result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.Email });
            if (result != null)
                throw new BadRequestException($"Аккаунт с email {request.Email} уже зарегистрирован! Укажите другой адрес или запросите пароль на email.");

            sql = "SELECT TOP 1 Id FROM Accounts WHERE Name = @Name";
            result = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { request.Name });
            if (result != null)
                throw new BadRequestException($"Аккаунт с именем {request.Name} уже зарегистрирован!");

            if (!request.AcceptTerms)
                throw new BadRequestException("Вы не приняли условия пользования сайтом!");
        }


        /// <summary>
        /// Обновление мероприятия
        /// </summary>
        /// <returns>SQL скрипт</returns>
        public static async Task<int> UpdateEventAsync(this UpdateEventRequestDto request, UnitOfWork unitOfWork)
        {
            var sql = $"UPDATE Events SET " +
                $"{nameof(EventsEntity.Name)} = @{nameof(EventsEntity.Name)}, " +
                $"{nameof(EventsEntity.Description)} = @{nameof(EventsEntity.Description)}, " +
                $"{nameof(EventsEntity.MaxMen)} = @{nameof(EventsEntity.MaxMen)}, " +
                $"{nameof(EventsEntity.MaxWomen)} = @{nameof(EventsEntity.MaxWomen)}, " +
                $"{nameof(EventsEntity.MaxPairs)} = @{nameof(EventsEntity.MaxPairs)} " +
                $"WHERE Id = @Id AND {nameof(EventsEntity.AdminId)} = @AccountId";

            var result = await unitOfWork.SqlConnection.ExecuteAsync(sql,
                new { request.Event.Id, request.Event.Name, request.Event.Description, request.Event.MaxMen, request.Event.MaxWomen, request.Event.MaxPairs, unitOfWork.AccountId },
                transaction: unitOfWork.SqlTransaction);

            return result;
        }

        public static async Task UpdateSchedulesAsync(this UpdateEventRequestDto request, UnitOfWork unitOfWork)
        {
            string sql;

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

                        var insertedScheduleId = await unitOfWork.SqlConnection.QuerySingleAsync<int>(sql,
                            new { EventId = request.Event.Id, schedule.Description, schedule.StartDate, schedule.EndDate, schedule.CostMan, schedule.CostWoman, schedule.CostPair },
                            transaction: unitOfWork.SqlTransaction);

                        // Доп. услуги расписания (features)
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

                                var result = await unitOfWork.SqlConnection.ExecuteAsync(sql,
                                    new { ScheduleId = insertedScheduleId, FeatureId = feature.Id },
                                    transaction: unitOfWork.SqlTransaction);
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

                        var result = await unitOfWork.SqlConnection.ExecuteAsync(sql,
                            new { schedule.Id, EventId = request.Event.Id, schedule.Description, schedule.StartDate, schedule.EndDate, schedule.CostMan, schedule.CostWoman, schedule.CostPair, schedule.IsDeleted },
                            transaction: unitOfWork.SqlTransaction);
                    }
                }
            }
        }
    }
}
