using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto;
using Common.Models.States;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Common.Models.SignalR;

namespace UI.Components.Pages.Profile
{
    public partial class Tab_Photos
    {
        [CascadingParameter] public CurrentState CurrentState { get; set; } = null!;
        [Parameter, EditorRequired] public EventCallback UpdateProfileCallback { get; set; }

        [Inject] IRepository<UpdatePhotoForAccountRequestDto, ResponseDtoBase> _repoUpdatePhoto { get; set; } = null!;
        [Inject] IRepository<UploadAccountPhotoFromTempRequestDto, UploadAccountPhotoFromTempResponseDto> _repoUploadPhoto { get; set; } = null!;

        bool processingPhoto;
        string? avatarBackground;
        bool shouldRender = true;

        async Task UpdateAsync(UpdateType type, PhotosForAccountsDto photo, string? newComment = null)
        {
            if (CurrentState.Account != null)
            {
                shouldRender = false;

                var request = new UpdatePhotoForAccountRequestDto
                {
                    Token = CurrentState.Account.Token,
                    Guid = photo.Guid,
                };

                switch (type)
                {
                    case UpdateType.AvatarChange:
                        request.IsAvatarChanging = true;
                        break;
                    case UpdateType.CommentChange:
                        request.IsCommentChanging = true;
                        request.Comment = newComment;
                        break;
                    case UpdateType.Delete:
                        request.IsDeleting = true;
                        break;
                }

                await _repoUpdatePhoto.HttpPostAsync(request);

                // SignalR: Уведомим всех пользователей при смене аватара или удалении аватара
                if (type == UpdateType.AvatarChange || (type == UpdateType.Delete && photo.IsAvatar))
                {
                    var signalRequest = new SignalGlobalRequest
                    {
                        OnAvatarChanged = new OnAvatarChanged { NewAvatar = photo }
                    };
                    await CurrentState.SignalRServerAsync(signalRequest);
                }

                await CurrentState.ReloadAccountAsync();
                shouldRender = true;
                await UpdateProfileCallback.InvokeAsync();
            }
        }


        async void UploadPhotos(IReadOnlyList<IBrowserFile> photos)
        {
            if (photos.Count > 0 && CurrentState.Account != null)
            {
                processingPhoto = true;
                StateHasChanged();

                var request = new UploadAccountPhotoFromTempRequestDto { Token = CurrentState.Account.Token };

                List<string> files = new List<string>();

                if (CurrentState.Account.Photos == null)
                    CurrentState.Account.Photos = new List<PhotosForAccountsDto>();

                foreach (var photo in photos)
                {
                    var baseFileName = DateTime.Now.ToString("yyyyMMddmmss") + "_" + Guid.NewGuid().ToString();
                    var originalFileName = baseFileName + Path.GetExtension(photo.Name);

                    await using (FileStream fs = new(StaticData.TempPhotosDir + originalFileName, FileMode.Create))
                        await photo.OpenReadStream(photo.Size).CopyToAsync(fs);

                    request.PhotosTempFileNames = originalFileName;

                    var apiResponse = await _repoUploadPhoto.HttpPostAsync(request);

                    CurrentState.Account.Photos.Add(apiResponse.Response.NewPhoto);
                    StateHasChanged();

                    if (CurrentState.Account.Photos.Count >= 20) break;
                }

                await CurrentState.ReloadAccountAsync();
                processingPhoto = false;
                await UpdateProfileCallback.InvokeAsync();
            }
        }

        protected override bool ShouldRender() => shouldRender;
    }

    enum UpdateType
    {
        AvatarChange,
        CommentChange,
        Delete
    }
}
