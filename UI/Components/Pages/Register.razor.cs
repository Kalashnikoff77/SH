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
using Common.Enums;
using Common;
using PhotoSauce.MagicScaler;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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


        string fileName;
        long? fileSize;
        string Photo;

        private void OnChange()
        {
            name = savedName;
            address = savedAddress;
            aboutMe = savedAboutMe;
            selectedHobbies = savedHobbies;
        }

        async void OnChange(string value, string name)
        {
            fileName = "file";

            using (MemoryStream input = new MemoryStream(3500000))
            {
                var test = Convert.FromBase64String(value);

                using (MemoryStream output = new MemoryStream(500000))
                {
                    await File.WriteAllBytesAsync($"c:\\testjpg", test);
                }
            }
        }

        void OnError(UploadErrorEventArgs args, string name)
        {
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
