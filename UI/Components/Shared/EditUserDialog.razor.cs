using Common.Dto;
using Common.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using UI.Extensions;
using UI.Models;

namespace UI.Components.Shared
{
    public partial class EditUserDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
        [Parameter] public UsersDto User { get; set; } = null!;

        Dictionary<short, TabPanel> TabPanels { get; set; } = null!;
        bool IsFormValid => TabPanels[1].Items.All(x => x.Value.IsValid == true);

        UsersDto UserCopy { get; set; } = null!;

        string Title { get; set; } = null!;
        string StartIcon = null!;
        string ButtonSubmitText = null!;

        DateTime? BirthDate
        {
            get => UserCopy.BirthDate == DateTime.MinValue ? null : UserCopy.BirthDate;
            set { if (value != null) { UserCopy.BirthDate = value.Value; } }
        }

        protected override void OnParametersSet()
        {
            bool IsValid = false;

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
                UserCopy = User.DeepCopy<UsersDto>()!;
                IsValid = true;
            }

            TabPanels = new Dictionary<short, TabPanel>
            {
                { 1, new TabPanel { Items = new Dictionary<string, TabPanelItem>
                        {
                            { nameof(UserCopy.Name), new TabPanelItem { IsValid = IsValid } },
                            { nameof(UserCopy.BirthDate), new TabPanelItem { IsValid = IsValid } },
                            { nameof(UserCopy.Height), new TabPanelItem { IsValid = IsValid } },
                            { nameof(UserCopy.Weight), new TabPanelItem { IsValid = IsValid } }
                        }
                    }
                }
            };
        }

        Color NameIconColor = Color.Default;
        string? NameValidator(string name)
        {
            string? errorMessage = null;
            if (string.IsNullOrWhiteSpace(name))
                errorMessage = $"Укажите имя партнёра";

            if (string.IsNullOrWhiteSpace(name) || name.Length < StaticData.DB_USERS_NAME_MIN)
                errorMessage = $"Имя должно содержать {StaticData.DB_USERS_NAME_MIN}-{StaticData.DB_USERS_NAME_MAX} символов";

            CheckFormProperties(errorMessage, nameof(UserCopy.Name), ref NameIconColor);
            return errorMessage;
        }

        Color BirthDateIconColor = Color.Default;
        string? BirthDateValidator(DateTime? birthDate)
        {
            string? errorMessage = null;
            if (birthDate == null)
                errorMessage = $"Укажите дату рождения";

            if (birthDate.HasValue && (birthDate < DateTime.Now.AddYears(-75) || birthDate > DateTime.Now.AddYears(-20)))
                errorMessage = $"Возраст от 20 до 75 лет";

            CheckFormProperties(errorMessage, nameof(UserCopy.BirthDate), ref BirthDateIconColor);
            return errorMessage;
        }

        void BirthDateChanged(DateTime? birthDate) =>
            BirthDate = birthDate;

        Color HeightIconColor = Color.Default;
        string? HeightValidator(short height)
        {
            string? errorMessage = null;
            if (height < StaticData.DB_USERS_HEIGHT_MIN || height > StaticData.DB_USERS_HEIGHT_MAX)
                errorMessage = $"Рост в пределах {StaticData.DB_USERS_HEIGHT_MIN}-{StaticData.DB_USERS_HEIGHT_MAX} см";

            CheckFormProperties(errorMessage, nameof(UserCopy.Height), ref HeightIconColor);
            return errorMessage;
        }

        Color WeightIconColor = Color.Default;
        string? WeightValidator(short height)
        {
            string? errorMessage = null;

            if (height < StaticData.DB_USERS_WEIGHT_MIN || height > StaticData.DB_USERS_WEIGHT_MAX)
                errorMessage = $"Вес в пределах {StaticData.DB_USERS_WEIGHT_MIN}-{StaticData.DB_USERS_WEIGHT_MAX} кг";

            CheckFormProperties(errorMessage, nameof(UserCopy.Weight), ref WeightIconColor);
            return errorMessage;
        }

        void CheckFormProperties(string? errorMessage, string property, ref Color iconColor)
        {
            if (errorMessage == null)
            {
                TabPanels[1].Items[property].IsValid = true;
                iconColor = Color.Success;
            }
            else
            {
                TabPanels[1].Items[property].IsValid = false;
                iconColor = Color.Error;
            }
            StateHasChanged();
        }


        void Submit() => MudDialog.Close(DialogResult.Ok(UserCopy));

        void Cancel() => MudDialog.Cancel();
    }
}
