using AutoMapper;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Enums;
using Common.Models;
using Common.Models.SignalR;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;
using System.Net;
using System.Text.Json;
using UI.Components.Shared;

namespace UI.Components.Pages
{
    public partial class AccountSettings
    {
        [Inject] IRepository<AccountUpdateModel, AccountUpdateRequestDto, AccountUpdateResponseDto> _repoAccountUpdate { get; set; } = null!;
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<UpdatePhotoModel, UpdatePhotoRequestDto, ResponseDtoBase> _repoPhotoUpdate { get; set; } = null!;
        [Inject] IMapper _mapper { get; set; } = null!;
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;

        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        AccountUpdateModel UpdatingModel { get; set; } = null!;

        int selectedIndex = 0;

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGetCountries.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);
        }

        protected override void OnParametersSet()
        {
            if (CurrentState.Account != null)
            {
                // Делаем глубокое копирование через JSON.
                UpdatingModel = _mapper.Map<AccountUpdateModel>(CurrentState.Account);
                var json = JsonSerializer.Serialize(UpdatingModel);
                UpdatingModel = JsonSerializer.Deserialize<AccountUpdateModel>(json)!;

                regions = countries
                    .Where(x => x.Id == UpdatingModel.Country.Id).FirstOrDefault()?
                    .Regions?.Select(s => s).ToList();

                // Строка с токеном для загрузки фото
                Bearer = $"Bearer {CurrentState.Account.Token}";
            }
        }


        #region /// ШАГ 1: ОБЩЕЕ ///
        int countryId
        {
            get { return UpdatingModel.Country.Id; }
            set
            {
                UpdatingModel.Country.Id = value;
                regions = countries
                    .Where(x => x.Id == countryId).FirstOrDefault()?
                    .Regions?.Select(s => s).ToList();
            }
        }

        int regionId
        {
            get { return UpdatingModel.Country.Region.Id; }
            set { UpdatingModel.Country.Region.Id = value; }
        }
        #endregion


        #region /// ШАГ 2: ПАРТНЁРЫ ///
        RadzenDataGrid<UsersDto> usersGrid = null!;

        List<UsersDto> Users => UpdatingModel.Users.Where(x => x.IsDeleted == false).ToList();

        async Task OpenEditUserForm(UsersDto? user)
        {
            var newUser = await DialogService.OpenAsync<EditUserForm>($"Партнёр {UpdatingModel.Name}",
                  new Dictionary<string, object?>() { { "User", user } },
                  new DialogOptions() { Width = "500px", Height = "450px" });

            // Если пользователь закрыл окно, нажав на крестик, то просто выйдем
            if (newUser == null)
                return;

            // Если user == null, то происходит добавление нового пользователя. Иначе было редактирование.
            if (user == null)
                UpdatingModel.Users.Add(newUser);
        }

        async void DeleteRow(UsersDto user)
        {
            var result = await DialogService.Confirm($"Удалить партнёра {user.Name}?", $"{user.Name} - удаление", new ConfirmOptions() { OkButtonText = "Да", CancelButtonText = "Нет" });
            if (result == true)
            {
                // Помечаем пользователя как удалённого
                user.IsDeleted = true;

                // Если user.Id == 0, то полностью удаляем из массива нового пользователя, которого ещё не занесли в БД.
                if (user.Id == 0)
                    UpdatingModel.Users.Remove(user);

                StateHasChanged();
            }
        }
        #endregion


        #region /// ШАГ 3: ФОТО ///
        int progressUpload;
        class ErrorUpload { public string ErrorMessage { get; set; } = null!; }
        string Bearer = null!;

        async Task AvatarChangedAsync(bool isChecked, AccountsPhotosDto photo)
        {
            // Снимем галку у прежнего аватара
            var oldAvatar = CurrentState.Account?.Photos?.FirstOrDefault(x => x.IsAvatar == true);

            if (oldAvatar != null)
            {
                var oldModel = new UpdatePhotoModel
                {
                    Token = CurrentState.Account!.Token,
                    Guid = oldAvatar.Guid,
                    IsAvatar = false,
                    Comment = oldAvatar.Comment,
                    IsDeleted = false
                };
                await _repoPhotoUpdate.HttpPostAsync(oldModel);
            };

            var model = new UpdatePhotoModel
            {
                Token = CurrentState.Account!.Token,
                Guid = photo.Guid,
                IsAvatar = isChecked,
                Comment = photo.Comment,
                IsDeleted = false
            };
            await _repoPhotoUpdate.HttpPostAsync(model);

            await CurrentState.ReloadAccountAsync();

            // Изменим аватары у всех залогиненных пользователей
            var avatarChangedTriggerModel = new AvatarChangedModel
            {
                AccountId = CurrentState.Account.Id,
                IsAvatar = isChecked,
                Guid = photo.Guid,
                Comment = photo.Comment
            };
            await CurrentState.SignalRServerAsync(EnumSignalRHandlers.AvatarChangedServer, avatarChangedTriggerModel);
        }

        async Task CommentChangedAsync(string NewComment, AccountsPhotosDto photo)
        {
            var model = new UpdatePhotoModel
            {
                Token = CurrentState.Account!.Token,
                Guid = photo.Guid,
                IsAvatar = photo.IsAvatar,
                Comment = string.IsNullOrWhiteSpace(NewComment) ? null : NewComment.Trim(),
                IsDeleted = false
            };
            await _repoPhotoUpdate.HttpPostAsync(model);

            await CurrentState.ReloadAccountAsync();
        }

        void OnProgress(UploadProgressArgs args)
        {
            UpdatingModel.ErrorUploadMessage = null;
            progressUpload = args.Progress;
        }

        async void OnCompleteAsync(UploadCompleteEventArgs args)
        {
            await CurrentState.ReloadAccountAsync();
            progressUpload = 0;
            StateHasChanged(); // Рендеринг нужно вызвать вручную
        }

        void OnError(UploadErrorEventArgs args)
        {
            var response = JsonSerializer.Deserialize<ErrorUpload>(args.Message);
            if (response != null)
                UpdatingModel.ErrorUploadMessage = response.ErrorMessage;
            progressUpload = 0;
        }

        async Task DeletePhotoAsync(AccountsPhotosDto photo)
        {
            var result = await DialogService.Confirm($"Удалить фото?", $"Удаление фото", new ConfirmOptions() { OkButtonText = "Да", CancelButtonText = "Нет" });
            if (result == true)
            {
                var model = new UpdatePhotoModel
                {
                    Token = CurrentState.Account!.Token,
                    Guid = photo.Guid,
                    IsAvatar = photo.IsAvatar,
                    Comment = photo.Comment,
                    IsDeleted = true
                };
                await _repoPhotoUpdate.HttpPostAsync(model);

                await CurrentState.ReloadAccountAsync();

                // Если удалили аватар, то изменим аватары у всех залогиненных пользователей
                if (photo.IsAvatar)
                {
                    var avatarChangedTriggerModel = new AvatarChangedModel
                    {
                        AccountId = CurrentState.Account.Id,
                        IsAvatar = false,
                        Guid = photo.Guid,
                        Comment = photo.Comment
                    };
                    await CurrentState.SignalRServerAsync(EnumSignalRHandlers.AvatarChangedServer, avatarChangedTriggerModel);
                }
            }
        }
        #endregion


        #region /// ШАГ 4: УВЕДОМЛЕНИЯ ///
        async Task InformingChanged(bool value)
        {
            //var updateResponse = await _repoAccountUpdate.HttpPostAsync(UpdatingModel);
            //await CurrentState.ReloadAccountAsync();
        }
        #endregion


        async Task SubmitAsync()
        {
            UpdatingModel.ErrorWhileUpdating = null;

            var updateResponse = await _repoAccountUpdate.HttpPostAsync(UpdatingModel);

            if (updateResponse.StatusCode != HttpStatusCode.OK)
            {
                UpdatingModel.ErrorWhileUpdating = updateResponse.Response.ErrorMessage;
                return;
            }

            var storage = await _protectedLocalStore.GetAsync<LoginModel>(nameof(LoginModel));
            if (!storage.Success)
                storage = await _protectedSessionStore.GetAsync<LoginModel>(nameof(LoginModel));

            if (storage.Success && storage.Value != null)
            {
                // Если меняется email или пароль, то обновим данные для логина в хранилище браузера
                var loginModel = new LoginModel
                {
                    Email = storage.Value.Email,
                    Password = storage.Value.Password
                };

                if (storage.Value.Email != UpdatingModel.Email)
                    loginModel.Email = UpdatingModel.Email;

                if (!string.IsNullOrWhiteSpace(UpdatingModel.NewPassword1))
                    loginModel.Password = UpdatingModel.NewPassword1;

                storage = await _protectedLocalStore.GetAsync<LoginModel>(nameof(LoginModel));
                if (storage.Success)
                    await _protectedLocalStore.SetAsync(nameof(LoginModel), loginModel);

                storage = await _protectedSessionStore.GetAsync<LoginModel>(nameof(LoginModel));
                if (storage.Success)
                    await _protectedSessionStore.SetAsync(nameof(LoginModel), loginModel);

                await CurrentState.ReloadAccountAsync();

                UpdatingModel = _mapper.Map<AccountUpdateModel>(CurrentState.Account);
                var json = JsonSerializer.Serialize(UpdatingModel);
                UpdatingModel = JsonSerializer.Deserialize<AccountUpdateModel>(json)!;
            }
        }
    }
}
