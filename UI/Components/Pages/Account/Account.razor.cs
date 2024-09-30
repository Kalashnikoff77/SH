using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;

namespace UI.Components.Pages.Account
{
    public partial class Account
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;
        [Parameter] public int AccountId { get; set; }

        [Inject] IRepository<GetAccountsRequestDto, GetAccountsResponseDto> _repoGetAccount { get; set; } = null!;

        AccountsViewDto account { get; set; } = null!;

        protected override async Task OnParametersSetAsync()
        {
            var request = new GetAccountsRequestDto 
            {
                Id = AccountId,
                IsPhotosIncluded = true,
                IsHobbiesIncluded = true,
                IsUsersIncluded = true
            };
            var response = await _repoGetAccount.HttpPostAsync(request);
            account = response.Response.Account;
        }
    }
}
