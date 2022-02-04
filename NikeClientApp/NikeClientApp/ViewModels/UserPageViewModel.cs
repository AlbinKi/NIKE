﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using NikeClientApp.Services;
using NikeClientApp.Models;
using System.Collections.ObjectModel;

namespace NikeClientApp.ViewModels
{
    public class UserPageViewModel : BaseViewModel
    {
        //public ICommand NextPage => new Command(async () => await NavigationService.NavigateTo<MainPageViewModel>());
        public ICommand BackPage => new Command(async () => await NavigationService.GoBack());
        //public ICommand Show => new Command(async () => await OnShow());
        public ICommand Edit => new Command<string>((param) => OnEdit(param));
        public ICommand Delete => new Command<string>(async (endpoint) => await OnDelete(endpoint));
        public ICommand Save => new Command(async (param) => await OnSave(param));
        private HttpService<User> userClient;
        private HttpService<Reaction> reactionClient;
        private HttpService<Comment> commentClient;
        private HttpService<Models.Entry> entryClient;

        private User _user;

        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        private ObservableCollection<Models.Entry> _entries;

        public ObservableCollection<Models.Entry> Entries
        {
            get { return _entries; }
            set { SetProperty(ref _entries, value); }
        }

        private ObservableCollection<Comment> _comments;

        public ObservableCollection<Comment> Comments
        {
            get { return _comments; }
            set { SetProperty(ref _comments, value); }
        }

        private ObservableCollection<Reaction> _reactions;

        public ObservableCollection<Reaction> Reactions
        {
            get { return _reactions; }
            set { SetProperty(ref _reactions, value); }
        }



        private async Task OnShow()
        {
            var user = await userClient.Get("user", "");

            var comments = await commentClient.GetList("entry/comments", "");

            var reactions = await reactionClient.GetList("entry/reactions", "");

            var entries = await entryClient.GetList("entry/list", "");

            Comments = new ObservableCollection<Comment>(comments.Data);

            Reactions = new ObservableCollection<Reaction>(reactions.Data);

            Entries = new ObservableCollection<Models.Entry>(entries.Data);

            User = user.Data;
        }

        private EditUser _userReadOnly;

        public EditUser UserReadOnly
        {
            get { return _userReadOnly; }
            set { SetProperty(ref _userReadOnly, value); }
        }

        private void OnEdit(string param)
        {
            switch (param)
            {
                case "username":
                    UserReadOnly.Username = false;
                    break;
                case "name":
                    UserReadOnly.Firstname = false;
                    break;
                case "lastname":
                    UserReadOnly.Lastname = false;
                    break;
                case "email":
                    UserReadOnly.Email = false;
                    break;
                case "password":
                    UserReadOnly.Password = false;
                    break;

            }
        }


        public async Task OnSave(object obj)
        {
            await userClient.Update("user", User);
        }

        public async Task OnDelete(string endpoint)
        {
            await userClient.Delete(endpoint);
            UserApi.ApiKey = null;
            await NavigationService.NavigateTo<MainPageViewModel>();
        }

        public UserPageViewModel(INaviService naviService) : base(naviService)
        {
            userClient = new HttpService<User>();
            reactionClient = new HttpService<Reaction>();
            commentClient = new HttpService<Comment>();
            entryClient = new HttpService<Models.Entry>();
            UserReadOnly = new EditUser();

        }

        public async override Task InitAsync()
        {
            await OnShow();
        }
    }
}
