﻿using Mobile.Views;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wellness.Mobile.Views;
using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class LoginViewModel: BaseViewModel
    {
        APIService _apiService = new APIService("Autentifikacija");
        APIService _apiService_Clan = new APIService("Clan");


        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await Login());
            
        }

        #region UserName&PasswordSetup


        string _username = string.Empty;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        string _password = string.Empty;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }
        #endregion UserName&PasswordSetup
       

        public ICommand LoginCommand { get; set; }

        async Task Login()
        {

            IsBusy = true;

            APIService._username = Username;
            APIService._password = Password;
            var request = new Wellness.Model.Requests.AutentifikacijaRequest()
            {
                password = Password,
                username = Username
            };


            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(Username))
            {
                var osoba = await _apiService.Get<Wellness.Model.Osoba>(request);
                if (osoba == null)
                {
                    await PopupNavigation.Instance.PushAsync(new PopupView("Error", "Pogresan Username ili Password"));
                }
                else
                {
                    if (osoba.Uloga.Naziv == "Clan")
                    {
                        var ClanSearchRequest = new Wellness.Model.Requests.ClanSearchRequest()
                        {

                            OsobaId = osoba.Id,
                        };

                        var ClanList = await _apiService_Clan.Get<List<Wellness.Model.Requests.ClanViewRequest>>(ClanSearchRequest);
                        var clan = ClanList[0];
                        Application.Current.MainPage = new MainPage(clan);

                    }
                    else
                    {
                        await PopupNavigation.Instance.PushAsync(new PopupView("Error", "Niste autorizovani"));
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Password))
                    await PopupNavigation.Instance.PushAsync(new PopupView("Error", "Password obavezan"));
                else
                if (string.IsNullOrEmpty(Username))
                    await PopupNavigation.Instance.PushAsync(new PopupView("Error", "Username obavezan"));

            }
        }

    }
}
