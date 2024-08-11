using Common;
using Common.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace UI.Components.Shared
{
    public partial class EditUserDialog
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public UsersDto User { get; set; } = new UsersDto();

        [Parameter]
        public string Title { get; set; } = null!;

        Color NameIconColor = Color.Default;
        DateTime? birthDate { get; set; } = null;

        string? NameValidator(string name)
        {
            string? errorMessage = null;
            NameIconColor = Color.Success;

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = $"Укажите имя партнёра";
                NameIconColor = Color.Error;
            }

            if (string.IsNullOrWhiteSpace(name) || name.Length < StaticData.DB_USERS_NAME_MIN)
            {
                errorMessage = $"Имя может содержать {StaticData.DB_USERS_NAME_MIN}-{StaticData.DB_USERS_NAME_MAX} символов";
                NameIconColor = Color.Error;
            }

            StateHasChanged();
            return errorMessage;
        }

        string? HeightValidator(short height)
        {
            string? errorMessage = null;

            if (height < StaticData.DB_USERS_HEIGHT_MIN || height > StaticData.DB_USERS_HEIGHT_MAX)
                errorMessage = $"Рост в пределах {StaticData.DB_USERS_HEIGHT_MIN}-{StaticData.DB_USERS_HEIGHT_MAX} см";

            StateHasChanged();
            return errorMessage;
        }

        string? WeightValidator(short height)
        {
            string? errorMessage = null;

            if (height < StaticData.DB_USERS_WEIGHT_MIN || height > StaticData.DB_USERS_WEIGHT_MAX)
                errorMessage = $"Вес в пределах {StaticData.DB_USERS_WEIGHT_MIN}-{StaticData.DB_USERS_WEIGHT_MAX} кг";

            StateHasChanged();
            return errorMessage;
        }

        void Submit()
        {
            if (birthDate.HasValue)
                User.BirthDate = birthDate.Value;
            MudDialog.Close(DialogResult.Ok(User));
        }

        void Cancel() => MudDialog.Cancel();
    }
}
