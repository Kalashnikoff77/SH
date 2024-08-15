namespace Common.Dto
{
    public class EventsPhotosDto : DtoBase
    {
        public Guid Guid { get; set; }

        public int EventId { get; set; }

        public string? Comment { get; set; }

        public bool IsAvatar { get; set; }
    }
}
