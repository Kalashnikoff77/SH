using Common.Enums;

namespace Common.Models.SignalR2
{
    public class UpdateRelationsModel : SignalRModelBase<UpdateRelationsModel>
    {
        public override EnumSignalRHandlers EnumSignalRHandlersClient => EnumSignalRHandlers.GetRelationsClient;

        public int RecipientId { get; set; }
        public EnumRelations EnumRelation { get; set; }
        public bool IsAdding { get; set; }
        public bool IsRemoving { get; set; }
        public bool IsConfirmed { get; set; }
    }
}