using Common.Dto;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Shared
{
    public partial class EditUserForm
    {
        [Parameter] public UsersDto User { get; set; } = null!;

        protected override void OnParametersSet()
        {
            if (User == null)
                User = new UsersDto() { Id = -1 }; // Метка, что пользователя добавляем, а не редактируем
        }

        void OnSubmit(UsersDto User) =>
            dialogService.Close(User);
    }
}
