using Common.Extensions;

namespace Common.Dto
{
    public class SchedulesForEventsDto : DtoBase
    {
        public int EventId { get; set; }
        public Guid Guid { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CostMan { get; set; }
        public int CostWoman { get; set; }
        public int CostPair { get; set; }

        /// <summary>
        /// Используется в Tab_About.razor для корректного вывода даты и времени
        /// </summary>
        public override string ToString() => $"{StartDate.ToMyString()} - {EndDate.ToMyString()}";
    }
}
