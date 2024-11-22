﻿using AutoMapper;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebAPI.Exceptions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : MyControllerBase
    {
        public MessagesController(IMapper mapper, IConfiguration configuration, IMemoryCache cache) : base(configuration, mapper, cache) { }


        [Route("Count"), HttpPost, Authorize]
        public async Task<GetMessagesCountResponseDto> GetCountAsync(GetMessagesCountRequestDto request)
        {
            AuthenticateUser();

            var response = new GetMessagesCountResponseDto();

            var sql = $"SELECT COUNT(*) FROM Messages " +
                $"WHERE {nameof(MessagesEntity.SenderId)} = @AccountId OR {nameof(MessagesEntity.RecipientId)} = @AccountId";
            response.TotalCount = await _unitOfWork.SqlConnection.QueryFirstAsync<int>(sql, new { _unitOfWork.AccountId });

            sql = $"SELECT COUNT(*) FROM Messages " +
                $"WHERE {nameof(MessagesEntity.RecipientId)} = @AccountId AND {nameof(MessagesEntity.ReadDate)} IS NULL";
            response.UnreadCount = await _unitOfWork.SqlConnection.QueryFirstAsync<int>(sql, new { _unitOfWork.AccountId });

            return response;
        }


        [Route("Get"), HttpPost, Authorize]
        public async Task<GetMessagesResponseDto> GetAsync(GetMessagesRequestDto request)
        {
            AuthenticateUser();

            var response = new GetMessagesResponseDto();
            IEnumerable<MessagesEntity> result;

            // Получим кол-во сообщений
            var sql = $"SELECT COUNT(*) FROM Messages " +
                $"WHERE ({nameof(MessagesEntity.SenderId)} = @AccountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @AccountId)";
            response.Count = await _unitOfWork.SqlConnection.QueryFirstAsync<int>(sql, new { _unitOfWork.AccountId, request.RecipientId });

            // Запрос на получение предыдущих сообщений
            if (request.GetPreviousFromId.HasValue)
            {
                sql = $"SELECT TOP (@Take) * FROM Messages " +
                    $"WHERE (({nameof(MessagesEntity.SenderId)} = @AccountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                    $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @AccountId)) " +
                    $"AND Id < {request.GetPreviousFromId} " +
                    $"ORDER BY Id DESC";
                result = (await _unitOfWork.SqlConnection.QueryAsync<MessagesEntity>(sql, new { _unitOfWork.AccountId, request.RecipientId, request.Take })).Reverse();
            }

            // Запрос на получение следующих сообщений
            else if (request.GetNextAfterId.HasValue)
            {
                sql = $"SELECT TOP (@Take) * FROM Messages " +
                    $"WHERE (({nameof(MessagesEntity.SenderId)} = @AccountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                    $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @AccountId)) " +
                    $"AND Id > {request.GetNextAfterId} " +
                    $"ORDER BY Id ASC";
                result = await _unitOfWork.SqlConnection.QueryAsync<MessagesEntity>(sql, new { _unitOfWork.AccountId, request.RecipientId, request.Take });
            }

            // Запрос на получение последних сообщений (по умолчанию)
            else
            {
                int offset = response.Count.Value > StaticData.MESSAGES_PER_BLOCK ? response.Count.Value - StaticData.MESSAGES_PER_BLOCK : 0;
                sql = $"SELECT * FROM Messages " +
                    $"WHERE ({nameof(MessagesEntity.SenderId)} = @AccountId AND {nameof(MessagesEntity.RecipientId)} = @RecipientId) " +
                    $"OR ({nameof(MessagesEntity.SenderId)} = @RecipientId AND {nameof(MessagesEntity.RecipientId)} = @AccountId) " +
                    $"ORDER BY Id ASC " +
                    $"OFFSET {offset} ROWS";
                result = await _unitOfWork.SqlConnection.QueryAsync<MessagesEntity>(sql, new { _unitOfWork.AccountId, request.RecipientId });
            }

            response.Messages = _unitOfWork.Mapper.Map<List<MessagesDto>>(result);

            // Получим отправителя и получателя
            var columns = GetRequiredColumns<AccountsViewEntity>();
            sql = $"SELECT TOP 2 {columns.Aggregate((a, b) => a + ", " + b)} FROM AccountsView WHERE Id = @AccountId OR Id = @RecipientId";
            var accounts = await _unitOfWork.SqlConnection.QueryAsync<AccountsViewEntity>(sql, new { _unitOfWork.AccountId, request.RecipientId });
            response.Sender = _unitOfWork.Mapper.Map<AccountsViewDto>(accounts.FirstOrDefault(x => x.Id == _unitOfWork.AccountId));
            response.Recipient = _unitOfWork.Mapper.Map<AccountsViewDto>(accounts.FirstOrDefault(x => x.Id == request.RecipientId));

            // Будем отмечать сообщения, как прочитанные?
            if (request.MarkAsRead)
            {
                var ids = response.Messages
                    .Where(w => w.RecipientId == _unitOfWork.AccountId && w.ReadDate == null)
                    .Select(s => s.Id.ToString());

                if (ids.Any())
                    await _unitOfWork.SqlConnection.ExecuteAsync($"UPDATE Messages SET {nameof(MessagesEntity.ReadDate)} = getdate() " +
                        $"WHERE Id IN ({ids.Aggregate((a, b) => a + ", " + b)})");
            }

            return response;
        }


        [Route("GetLastMessagesList"), HttpPost, Authorize]
        public async Task<GetLastMessagesListResponseDto> GetLastMessagesListAsync(GetLastMessagesListRequestDto request)
        {
            AuthenticateUser();

            var response = new GetLastMessagesListResponseDto();

            var sql = $"SELECT TOP (@Take) * FROM LastMessagesListView " +
                $"WHERE {nameof(LastMessagesListViewEntity.SenderId)} = @AccountId " +
                $"OR {nameof(LastMessagesListViewEntity.RecipientId)} = @AccountId";
            var result = await _unitOfWork.SqlConnection.QueryAsync<LastMessagesListViewEntity>(sql, new { _unitOfWork.AccountId, request.Take });

            var sortedResult = result.Where(x => x.RecipientId == _unitOfWork.AccountId)
                .Union(result.Where(x => x.SenderId == _unitOfWork.AccountId))
                .ToList();

            response.LastMessagesList = _unitOfWork.Mapper.Map<List<LastMessagesListViewDto>>(sortedResult);

            return response;
        }


        [Route("Add"), HttpPost, Authorize]
        public async Task<AddMessageResponseDto> AddAsync(AddMessageRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                throw new BadRequestException("Вы не ввели текст сообщения!");

            AuthenticateUser();

            var response = new AddMessageResponseDto();

            var sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @AccountId";
            var senderId = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { _unitOfWork.AccountId }) ?? throw new NotFoundException($"Пользователь-отправитель с Id {_unitOfWork.AccountId} не найден!");

            sql = "SELECT TOP 1 Id FROM Accounts WHERE Id = @RecipientId";
            var recipientId = await _unitOfWork.SqlConnection.QueryFirstOrDefaultAsync<int?>(sql, new { request.RecipientId }) ?? throw new NotFoundException($"Пользователь-получатель с Id {_unitOfWork.AccountId} не найден!");

            sql = $"INSERT INTO Messages ({nameof(MessagesEntity.SenderId)}, {nameof(MessagesEntity.RecipientId)}, {nameof(MessagesEntity.Text)}) " +
                "VALUES (@senderId, @recipientId, @Text)";
            await _unitOfWork.SqlConnection.ExecuteAsync(sql, new { senderId, recipientId, request.Text });

            var message = new MessagesEntity
            {
                SenderId = _unitOfWork.AccountId!.Value,
                RecipientId = request.RecipientId,
                Text = request.Text
            };

            response.Message = new MessagesDto
            {
                SenderId = _unitOfWork.AccountId!.Value,
                RecipientId = request.RecipientId,
                CreateDate = DateTime.Now,
                Text = message.Text
            };

            return response;
        }
    }
}
