using Common.Dto;
using Common.Dto.Requests;
using Common.Dto.Responses;
using Common.Dto.Views;
using Common.Models;
using Common.Repository;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

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

            RegisterModel.Users.Add(new UsersDto()
            {
                Name = "Имя партнёра",
                BirthDate = DateTime.Parse("01.01.1979"),
                Gender = 0,
                Weight = 90,
                Height = 185
            });
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


        string? fileName;
        long? fileSize;
        string? photo;

        void AddUser()
        {
            if (RegisterModel.Users.Count < 4)
                RegisterModel.Users.Add(new UsersDto()
                {
                    Name = "Новый партнёр",
                    BirthDate = DateTime.Parse("01.01.1979"),
                    Gender = 0,
                    Weight = 90,
                    Height = 185
                });
        }

        void DeleteUser(UsersDto user)
        {
            RegisterModel.Users.Remove(user);
        }



        private void OnChange()
        {
            name = savedName;
            address = savedAddress;
            aboutMe = savedAboutMe;
        }

        async void OnChange(string value, string name)
        {
            fileName = "Ваш аватар";
            fileSize = null;

            //value = Regex.Replace(value, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            //var base64 = Convert.FromBase64String(value);
            //using (MemoryStream output = new MemoryStream(500000))
            //    await File.WriteAllBytesAsync($"c:\\test.jpg", base64);
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
    }
}
