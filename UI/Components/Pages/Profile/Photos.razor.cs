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

        [Inject] IRepository<UploadPhotosFromTempRequestDto, ResponseDtoBase> _repoUploadPhotos { get; set; } = null!;

        bool processingPhoto;

        void SetAvatarClick(PhotosForAccountsDto photo)
        {
            
        }

        async void UploadPhotos(IReadOnlyList<IBrowserFile> photos)
        {
            processingPhoto = true;

            var request = new UploadPhotosFromTempRequestDto
            {
                PhotosTempFileNames = new List<string>(),
                Token = CurrentState.Account!.Token
            };

            List<string> files = new List<string>();

            foreach (var photo in photos)
            {
                var baseFileName = DateTime.Now.ToString("yyyyMMddmmss") + "_" + Guid.NewGuid().ToString();
                var originalFileName = baseFileName + Path.GetExtension(photo.Name);

                await using (FileStream fs = new(StaticData.AccountsPhotosTempDir + originalFileName, FileMode.Create))
                    await photo.OpenReadStream(photo.Size).CopyToAsync(fs);

                request.PhotosTempFileNames.Add(originalFileName);
            }

            var apiResponse = await _repoUploadPhotos.HttpPostAsync(request);

            await CurrentState.ReloadAccountAsync();

            processingPhoto = false;
            StateHasChanged();
        }
    }
}
