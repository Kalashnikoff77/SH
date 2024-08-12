using Common;
using Common.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Extensions;

namespace UI.Components.Shared
{
    public partial class EditUserDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter] public UsersDto User { get; set; } = null!;

        Color NameIconColor = Color.Default;
        UsersDto UserCopy { get; set; } = null!;
        DateTime? birthDate { get; set; } = null;

        string Title { get; set; } = null!;
        string StartIcon = null!;
        string ButtonSubmitText = null!;

        protected override void OnParametersSet()
        {
            if (User == null)
            {
                UserCopy = new UsersDto { Id = -1 };
                Title = "Добавление партнёра";
                StartIcon = Icons.Material.Outlined.Add;
                ButtonSubmitText = "Добавить";
            }
            else
            {
                Title = $"Редактирование партнёра - {User.Name}";
                StartIcon = Icons.Material.Outlined.Check;
                ButtonSubmitText = "Сохранить";
                birthDate = User.BirthDate;
                UserCopy = User.DeepCopy<UsersDto>()!;
            }
        }

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
                UserCopy.BirthDate = birthDate.Value;
            MudDialog.Close(DialogResult.Ok(UserCopy));
        }

        void Cancel() => MudDialog.Cancel();
    }
}
