using Common.Dto;
using Common.Enums;

namespace Common.Models.SignalR
{
    public class GetRelationsModel : SignalRModelBase<GetRelationsModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.GetRelationsClient;

        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public List<RelationsForAccountsDto>? Relations { get; set; }
    }
}
