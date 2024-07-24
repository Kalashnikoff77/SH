﻿using Common.Dto.Responses;
using Common.Enums;
using Dapper;
using DataContext.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    internal class UpdateRelationModel
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }

        public SqlConnection Conn { get; set; } = null!;

        public RelationsUpdateResponseDto Response { get; set; } = null!;

        public async Task RemoveAllRelationsAsync()
        {
            var sql = $"DELETE FROM AccountsRelations WHERE ({nameof(AccountsRelationsEntity.SenderId)} = @SenderId AND {nameof(AccountsRelationsEntity.RecipientId)} = @RecipientId) OR ({nameof(AccountsRelationsEntity.SenderId)} = @RecipientId AND {nameof(AccountsRelationsEntity.RecipientId)} = @SenderId)";
            await Conn.ExecuteAsync(sql, new { SenderId, RecipientId });
        }

        public async Task BlockUserAsync()
        {
            // Проверим, заблокирован ли пользователь в данный момент?
            var sql = $"SELECT TOP 1 Id FROM AccountsRelations WHERE " +
                $"(({nameof(AccountsRelationsEntity.SenderId)} = @SenderId AND {nameof(AccountsRelationsEntity.RecipientId)} = @RecipientId) " +
                $"OR " +
                $"({nameof(AccountsRelationsEntity.SenderId)} = @RecipientId AND {nameof(AccountsRelationsEntity.RecipientId)} = @SenderId)) " +
                $"AND Type = @Type";
            var currentBlocked = await Conn.QueryFirstOrDefaultAsync<int?>(sql, new { SenderId, RecipientId, Type = (short)EnumRelations.Blocked });

            // Удалим все связи обоих пользователей
            await RemoveAllRelationsAsync();

            // Добавим связь "Заблокирован"
            if (currentBlocked == null)
            {
                sql = $"INSERT INTO AccountsRelations ({nameof(AccountsRelationsEntity.SenderId)}, {nameof(AccountsRelationsEntity.RecipientId)}, {nameof(AccountsRelationsEntity.Type)}, {nameof(AccountsRelationsEntity.IsConfirmed)}) " +
                    $"VALUES " +
                    $"(@{nameof(AccountsRelationsEntity.SenderId)}, @{nameof(AccountsRelationsEntity.RecipientId)}, {(short)EnumRelations.Blocked}, 1)";
                await Conn.ExecuteAsync(sql, new { SenderId, RecipientId });

                Response.IsRelationAdded = true;
            }
        }

        public async Task SubscribeUserAsync()
        {
            // Проверим, есть ли такая связь?
            var sql = $"SELECT TOP 1 Id FROM AccountsRelations WHERE " +
                $"({nameof(AccountsRelationsEntity.SenderId)} = @SenderId AND {nameof(AccountsRelationsEntity.RecipientId)} = @RecipientId) AND {nameof(AccountsRelationsEntity.Type)} = {(short)EnumRelations.Subscriber}";
            var relationId = await Conn.QueryFirstOrDefaultAsync<int?>(sql, new { SenderId, RecipientId });

            if (relationId == null)
            {
                sql = $"INSERT INTO AccountsRelations ({nameof(AccountsRelationsEntity.SenderId)}, {nameof(AccountsRelationsEntity.RecipientId)}, {nameof(AccountsRelationsEntity.Type)}, {nameof(AccountsRelationsEntity.IsConfirmed)}) " +
                    $"VALUES " +
                    $"(@{nameof(AccountsRelationsEntity.SenderId)}, @{nameof(AccountsRelationsEntity.RecipientId)}, {(short)EnumRelations.Subscriber}, 1)";
                await Conn.ExecuteAsync(sql, new { SenderId, RecipientId });
                Response.IsRelationAdded = true;
            }
            else
            {
                sql = $"DELETE FROM AccountsRelations WHERE Id = @relationId";
                await Conn.ExecuteAsync(sql, new { relationId });
                Response.IsRelationAdded = false;
            }
        }


        public async Task FriendshipUserAsync() 
        {
            // Проверим, есть ли такая связь?
            var sql = $"SELECT TOP 1 * FROM AccountsRelations WHERE " +
                $"(({nameof(AccountsRelationsEntity.SenderId)} = @SenderId AND {nameof(AccountsRelationsEntity.RecipientId)} = @RecipientId) " +
                $"OR ({nameof(AccountsRelationsEntity.SenderId)} = @RecipientId AND {nameof(AccountsRelationsEntity.RecipientId)} = @SenderId)) " +
                $"AND {nameof(AccountsRelationsEntity.Type)} = {(short)EnumRelations.Friend}";
            var currentRelation = await Conn.QueryFirstOrDefaultAsync<AccountsRelationsEntity>(sql, new { SenderId, RecipientId });

            if (currentRelation == null)
            {
                sql = $"INSERT INTO AccountsRelations ({nameof(AccountsRelationsEntity.SenderId)}, {nameof(AccountsRelationsEntity.RecipientId)}, {nameof(AccountsRelationsEntity.Type)}, {nameof(AccountsRelationsEntity.IsConfirmed)}) " +
                    $"VALUES " +
                    $"(@{nameof(AccountsRelationsEntity.SenderId)}, @{nameof(AccountsRelationsEntity.RecipientId)}, {(short)EnumRelations.Friend}, 0)";
                await Conn.ExecuteAsync(sql, new { SenderId, RecipientId });
                Response.IsRelationAdded = true;
            }
            else
            {
                // Если связь подтверждена, то удаляем
                if (currentRelation.IsConfirmed)
                {
                    sql = $"DELETE FROM AccountsRelations WHERE Id = @Id";
                    await Conn.ExecuteAsync(sql, new { currentRelation.Id });
                    Response.IsRelationAdded = false;
                }
                // Если связь не подтверждена, то подтверждаем
                else
                {
                    sql = $"UPDATE AccountsRelations SET {nameof(AccountsRelationsEntity.IsConfirmed)} = 1 WHERE Id = {currentRelation.Id}";
                    await Conn.ExecuteAsync(sql);
                }
            }
        }
    }
}
