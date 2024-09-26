using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Models;
using Common.Models.States;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace UI.Components.Pages.Profile
{
    public partial class Photos
    {
        [CascadingParameter] CurrentState CurrentState { get; set; } = null!;

        [Inject] IRepository<UploadPhotoFromTempRequestDto, UploadPhotoFromTempResponseDto> _repoUploadPhoto { get; set; } = null!;
        [Inject] IRepository<UpdatePhotoRequestDto, ResponseDtoBase> _repoUpdatePhoto { get; set; } = null!;

        bool processingPhoto;
        string? avatarBackground;
        bool shouldRender = true;

        async Task UpdateAsync(UpdateType type, PhotosForAccountsDto photo, string? newComment = null)
        {
            shouldRender = false;

            var request = new UpdatePhotoRequestDto
            {
                Token = CurrentState.Account!.Token,
                Guid = photo.Guid,
            };

            switch(type)
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

            await CurrentState.ReloadAccountAsync();
            shouldRender = true;
        }


        async void UploadPhotos(IReadOnlyList<IBrowserFile> photos)
        {
            processingPhoto = true;

            var request = new UploadPhotoFromTempRequestDto { Token = CurrentState.Account!.Token };

            List<string> files = new List<string>();

            foreach (var photo in photos)
            {
                var baseFileName = DateTime.Now.ToString("yyyyMMddmmss") + "_" + Guid.NewGuid().ToString();
                var originalFileName = baseFileName + Path.GetExtension(photo.Name);

                await using (FileStream fs = new(StaticData.AccountsPhotosTempDir + originalFileName, FileMode.Create))
                    await photo.OpenReadStream(photo.Size).CopyToAsync(fs);

                request.PhotosTempFileNames = originalFileName;

                var apiResponse = await _repoUploadPhoto.HttpPostAsync(request);

                CurrentState.Account.Photos!.Add(apiResponse.Response.NewPhoto);
                StateHasChanged();
            }

            await CurrentState.ReloadAccountAsync();

            processingPhoto = false;
            StateHasChanged();
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
