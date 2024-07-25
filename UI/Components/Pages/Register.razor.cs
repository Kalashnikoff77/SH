using Radzen.Blazor;
using Radzen;
using Common.Models;
using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using System;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace UI.Components.Pages
{
    public partial class Register
    {
        [Inject] IRepository<GetCountriesModel, GetCountriesRequestDto, GetCountriesResponseDto> _repoGet { get; set; } = null!;

        AccountRegisterModel RegisterModel = new AccountRegisterModel();
        List<CountriesViewDto> countries { get; set; } = new List<CountriesViewDto>();
        List<RegionsDto>? regions { get; set; } = new List<RegionsDto>();

        protected override async Task OnInitializedAsync()
        {
            var apiResponse = await _repoGet.HttpPostAsync(new GetCountriesModel());
            countries.AddRange(apiResponse.Response.Countries);
        }

        int countryId {
            get { return RegisterModel.Country.Id; }
            set {
                RegisterModel.Country.Id = value;
                regions = countries?
                    .Where(x => x.Id == countryId).FirstOrDefault()?
                    .Regions?.Select(s => s).ToList();
            }
        }

        int regionId {
            get { return RegisterModel.Country.Region.Id; }
            set { RegisterModel.Country.Region.Id = value; }
        }

        private string name;
        private string savedName;

        private string address;
        private string savedAddress;

        private string aboutMe;
        private string savedAboutMe;

        private List<Hobby> hobbies = new List<Hobby>() { new("Games"), new("Sport"), new("Movies"), new("Books"), new("Music") };
        private IList<Hobby> selectedHobbies = new List<Hobby>();
        private List<Hobby> savedHobbies = new List<Hobby>();

        string? Process { get; set; }

        async void OnPhotoChange(UploadChangeEventArgs args)
        {
            Process = null;
            int c = 0;

            foreach (var file in args.Files)
            {
                if ((file.ContentType == "image/jpeg" || file.ContentType == "image/png" || file.ContentType != "image/jpeg") && (file.Size > 50000 && file.Size < 25000000))
                {
                    Process = $"Загружается фото {file.Name} (размер: {file.Size})....";
                    StateHasChanged();

                    try
                    {
                        Thread.Sleep(1500);
                        using (MemoryStream input = new MemoryStream(3500000))
                        {
                            await file.OpenReadStream(25000000).CopyToAsync(input);
                            input.Position = 0;

                            await File.WriteAllBytesAsync($"c:\\!!!\\!Photos\\test_{c.ToString()}.jpg", input.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    c++;
                }
            }

            Process = "Фото загружено!";
            StateHasChanged();
        }

        async void OnPhotoComplete(UploadCompleteEventArgs args)
        {
        }

        void OnProgressPhoto(UploadProgressArgs args)
        {
            if (args.Progress == 100)
            {
                foreach (var file in args.Files)
                {
                }
            }
        }



        private void OnChange()
        {
            name = savedName;
            address = savedAddress;
            aboutMe = savedAboutMe;
            selectedHobbies = savedHobbies;
        }

        private async Task CanChange(StepsCanChangeEventArgs args)
        {
            if (args.SelectedIndex == 0 && savedName == name && savedAddress == address)
            {
                return;
            }

            if (args.SelectedIndex == 1 && savedAboutMe == aboutMe)
            {
                return;
            }

            if (args.SelectedIndex == 2 && savedHobbies.SequenceEqual(selectedHobbies))
            {
                return;
            }

            var response = await DialogService.Confirm(
                "Are you sure you want to continue without saving?",
                "Confirm",
                new ConfirmOptions()
                {
                    CloseDialogOnEsc = false,
                    CloseDialogOnOverlayClick = false,
                    ShowClose = false,
                    CancelButtonText = "No",
                    OkButtonText = "Yes",
                });

            if (response == false)
            {
                args.PreventDefault();
            }
        }

        private void SaveNameAndAdress()
        {
            savedName = name;
            savedAddress = address;
        }

        private void SaveAboutMe()
        {
            savedAboutMe = aboutMe;
        }

        private void SaveHobbies()
        {
            savedHobbies = selectedHobbies.ToList();
        }

        private class Hobby
        {
            public Hobby(string hobbyName)
            {
                HobbyName = hobbyName;
            }

            public string HobbyName { get; set; }

            public override bool Equals(object obj)
            {
                return obj is Hobby hobby && hobby.HobbyName == HobbyName;
            }

            public override int GetHashCode()
            {
                return HobbyName.GetHashCode();
            }
        }

    }
}
