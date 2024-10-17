﻿using Common.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Dialogs
{
    public partial class EditScheduleForEventDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter, EditorRequired] public SchedulesForEventsDto Schedule { get; set; } = new SchedulesForEventsDto();

        List<SchedulesForEventsDto> schedules { get; set; } = new List<SchedulesForEventsDto>();

        const int maxStartDateDays = 30 * 3;
        const int maxEndDateDays = 30;
        bool isFormValid = false;
        string? errorMessage;

        protected override void OnInitialized()
        {
            startTime = new TimeSpan(Schedule.StartDate.Hour, Schedule.StartDate.Minute, Schedule.StartDate.Second);
            endTime = new TimeSpan(Schedule.EndDate.Hour, Schedule.EndDate.Minute, Schedule.EndDate.Second);
        }

        DateTime? startDate
        {
            get => Schedule.StartDate == DateTime.MinValue ? null : Schedule.StartDate;
            set { Schedule.StartDate = value!.Value; CheckProperties(); }
        }
        TimeSpan? _startTime;
        TimeSpan? startTime
        {
            get => _startTime; 
            set { _startTime = value!.Value; CheckProperties(); }
        }

        DateTime? endDate
        {
            get => Schedule.EndDate == DateTime.MinValue ? null : Schedule.EndDate; 
            set { Schedule.EndDate = value!.Value; CheckProperties(); }
        }
        TimeSpan? _endTime;
        TimeSpan? endTime
        {
            get => _endTime;
            set { _endTime = value!.Value; CheckProperties(); }
        }

        void CheckProperties()
        {
            isFormValid = false;
            errorMessage = null;

            if (startDate.HasValue && startTime.HasValue && endDate.HasValue && endTime.HasValue)
            {
                if (startDate.Value + startTime.Value >= endDate.Value + endTime.Value)
                {
                    errorMessage = "Дата начала мероприятия должна быть меньше даты его окончания";
                }
                else
                {
                    schedules.Clear();
                    schedules.Add(new SchedulesForEventsDto
                    {
                        EventId = Schedule.EventId,
                        Description = Schedule.Description,
                        StartDate = startDate.Value + startTime.Value,
                        EndDate = endDate.Value + endTime.Value,
                        CostMan = Schedule.CostMan,
                        CostWoman = Schedule.CostWoman,
                        CostPair = Schedule.CostPair
                    });
                    isFormValid = true;
                }
            }
            StateHasChanged();
        }


        void Submit()
        {
            // Финальная проверка перед закрытием окна
            CheckProperties();
            if (isFormValid)
                MudDialog.Close(DialogResult.Ok(schedules));
        }
        void Cancel() => MudDialog.Cancel();
    }
}
