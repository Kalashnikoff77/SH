using AutoMapper;
using Common;
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
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PhotoSauce.MagicScaler;
using Radzen;
using Radzen.Blazor;
using System.Text.Json;
using UI.Components.Shared;

namespace UI.Components.Pages
{
    public partial class AccountSettings
    {
        [Inject] IRepository<AccountUpdateModel, AccountUpdateRequestDto, AccountUpdateResponseDto> _repoAccountUpdate { get; set; } = null!;
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGetCountries { get; set; } = null!;
        [Inject] IRepository<UpdatePhotoModel, UpdatePhotoRequestDto, ResponseDtoBase> _repoPhotoUpdate { get; set; } = null!;
        [Inject] IRepository<UploadPhotoModel, UploadPhotoRequestDto, ResponseDtoBase> _repoPhotoUpload { get; set; } = null!;
        [Inject] IMapper _mapper { get; set; } = null!;
        [Inject] ProtectedLocalStorage _protectedLocalStore { get; set; } = null!;
        [Inject] ProtectedSessionStorage _protectedSessionStore { get; set; } = null!;

        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        AccountUpdateModel UpdatingModel { get; set; } = null!;

        int selectedIndex = 0;
        bool shouldRender = true;

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

        async Task OpenEditUserForm(UsersDto? user)
        {
            var newUser = await DialogService.OpenAsync<EditUserForm>($"Новый партнёр для {UpdatingModel.Name}",
                  new Dictionary<string, object?>() { { "User", user } },
                  new DialogOptions() { Width = "500px", Height = "450px" });

            if (newUser != null)
            {
                // Если редактируем пользователя, то удалим старого и добавим как нового
                if (user != null && UpdatingModel.Users.Contains(user))
                    UpdatingModel.Users.Remove(user);

                newUser.Id = 0;
                UpdatingModel.Users.Add(newUser);
                await usersGrid.InsertRow(newUser);
                await usersGrid.Reload();
            }
        }

        async Task DeleteRow(UsersDto user)
        {
            if (UpdatingModel.Users.Contains(user))
                UpdatingModel.Users.Remove(user);
            await usersGrid.Reload();
        }

        #endregion

        #region /// ШАГ 3: ФОТО ///
        async Task AvatarChangedAsync(bool isChecked, AccountsPhotosDto photo)
        {
            shouldRender = false;

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

            shouldRender = true;
        }

        async Task CommentChangedAsync(string NewComment, AccountsPhotosDto photo)
        {
            shouldRender = false;

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

            shouldRender = true;
        }

        //async void UploadPhotosAsync(InputFileChangeEventArgs e)
        //{
        //    var selectedFiles = e.GetMultipleFiles();

        //    var model = new UploadPhotoModel { Token = CurrentState.Account!.Token };

        //    foreach (var file in selectedFiles)
        //    {
        //        var extension = new FileInfo(file.Name).Extension;

        //        using (MemoryStream input = new MemoryStream(3500000))
        //        {
        //            await file.OpenReadStream(25000000).CopyToAsync(input);
        //            input.Position = 0;

        //            var photoName = $"{Guid.NewGuid()}{extension}";

        //            using (MemoryStream output = new MemoryStream(500000))
        //            {
        //                MagicImageProcessor.ProcessImage(input, output, StaticData.Images[EnumImageSize.s768x1024]);
        //                await File.WriteAllBytesAsync($"wwwroot/images/AccountsPhotos/temp/{photoName}", output.ToArray());
        //            }

        //            model.PhotoNames.Add(photoName);
        //        }
        //    }

        //    await _repoPhotoUpload.HttpPostAsync(model);

        //    await CurrentState.ReloadAccountAsync();

        //    StateHasChanged(); // Рендеринг нужно вызвать вручную
        //}

        async Task DeletePhotoAsync(AccountsPhotosDto photo)
        {
            shouldRender = false;

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

            shouldRender = true;
        }
        #endregion

    }
}
