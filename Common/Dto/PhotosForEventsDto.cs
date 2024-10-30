namespace Common.Dto
{
    public class PhotosForEventsDto : DtoBase
    {
        public Guid Guid { get; set; }

        public DateTime CreateDate { get; set; }

        public int EventId { get; set; }

        public string? Comment { get; set; }

        public bool IsAvatar { get; set; }

        public bool IsDeleted { get; set; }
    }
}
