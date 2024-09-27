﻿namespace Common.Dto.Requests
{
    public class GetSchedulesRequestDto : RequestDtoBase
    {
        public override string Uri => "/Events/Get";

        /// <summary>
        /// Получить все записи определённого мероприятия (используется при добавлении сообщения в обсуждение события)
        /// </summary>
        public int? EventId { get; set; }

        /// <summary>
        /// Получить одну запись с указанным ScheduleId
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Мероприятия каких организаторов выводить
        /// </summary>
        public IEnumerable<int>? AdminsIds { get; set; }

        /// <summary>
        /// Мероприятия из каких регионов выводить
        /// </summary>
        public IEnumerable<int>? RegionsIds { get; set; }

        /// <summary>
        /// Мероприятия с какими тегами выводить
        /// </summary>
        public IEnumerable<int>? FeaturesIds { get; set; } = null;

        public bool IsPhotosIncluded { get; set; }
    }
}
