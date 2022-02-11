using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NikeClientApp.Models;
using NikeClientApp.Services;
using NikeClientApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Entry = NikeClientApp.Models.Entry;

namespace NikeClientApp.ViewModels
{
    public class MapPageViewModel : BaseViewModel
    {
        public Command NextPage => new Command(async () => await NavigationService.NavigateTo<MainPageViewModel>());

        public ICommand _AddPOI_Clicked => new Command(async () => await AddPOI_Clicked());
        public ICommand _PinIcon_Clicked => new Command(async () => await PinIcon_Clicked());
        public ICommand _RatingAmount => new Command((object sender) => RatingAmount(sender));
        public ICommand _SearchButton_Clicked => new Command(async () => await SearchButton_Clicked());

        public ICommand _StandardMapView => new Command(async () => await StandardMapView());
        public ICommand _SatelliteMapView => new Command(async () => await SatelliteMapView());
        public ICommand _HybridMapView => new Command(async () => await HybridMapView());

        public ICommand _BackArrowClicked => new Command(async () => await BackArrowClicked());
        public ICommand _FoldFrameClicked => new Command(async (object sender) => await FoldFrameClicked((Frame)sender));

        public ICommand _EntryButton_Clicked => new Command(async () => await EntryButton_Clicked());

        public ICommand LikeButtonClicked => new Command(async (object sender) => await LikeButton_Clicked(sender));

        public ICommand GetNextEntries => new Command(async () => await OnGetNextEntries());
        public ICommand GetPreviousEntries => new Command(async () => await OnGetPreviousEntries());



        HttpService<Models.Entry> _entryClient = new HttpService<Models.Entry>();
        HttpService<Forecast> weatherClient = new HttpService<Forecast>();
        HttpService<POI> poiListClient = new HttpService<POI>();
        HttpService<LikeDislikeEntry> httpClientLike = new HttpService<LikeDislikeEntry>();

        //Constructor
        #region Constructor
        public MapPageViewModel(INaviService naviService) : base(naviService)
        {
            //MapPage.CustomMap.MapClicked += MapClicked;
            TestMPVM = this;
        }
        public MapPageViewModel()
        {

        }
        #endregion; 

        public async Task StandardMapView()
        {
            MapPage.CustomMap.MapType = MapType.Street;
        }
        public async Task SatelliteMapView()
        {
            MapPage.CustomMap.MapType = MapType.Satellite;
        }
        public async Task HybridMapView()
        {
            MapPage.CustomMap.MapType = MapType.Hybrid;
        }


        //Properties 
        #region Properties

        public static List<Pin> ListOfPins = new List<Pin>();
        public static Pin pinner { get; set; }

        int _entryRating = 0;
        public int EntryRating { get => _entryRating; set { SetProperty(ref _entryRating, value); } }

        //Map _map = new Map();
        //public Map map { get => _map; set { SetProperty(ref _map, value); } }

        POI _poi = new POI();
        public POI poiToAdd { get => _poi; set { SetProperty(ref _poi, value); } }

        Models.Entry _entry = new Models.Entry();
        public Models.Entry entryToAdd { get => _entry; set { SetProperty(ref _entry, value); } }

        private bool _addPoiModalIsVisible;
        public bool addPoiModalIsVisible { get { return _addPoiModalIsVisible; } set { SetProperty(ref _addPoiModalIsVisible, value); } }

        private bool _addEntryModalIsVisible = false;
        public bool addEntryModalIsVisible { get => _addEntryModalIsVisible; set { SetProperty(ref _addEntryModalIsVisible, value); } }

        private bool _pOIListIsVisible;

        public bool POIListIsVisible { get => _pOIListIsVisible; set { SetProperty(ref _pOIListIsVisible, value); } }

        private bool _entryListIsVisible;
        public bool EntryListIsVisible { get => _entryListIsVisible; set { SetProperty(ref _entryListIsVisible, value); } }

        private bool _entryButtonIsVisible;
        public bool EntryButtonIsVisible { get => _entryButtonIsVisible; set { SetProperty(ref _entryButtonIsVisible, value); } }

        private bool _backArrowIsVisible;
        public bool BackArrowIsVisible { get => _backArrowIsVisible; set { SetProperty(ref _backArrowIsVisible, value); } }

        private bool _foldButtonIsVisible;
        public bool FoldButtonIsVisible { get => _foldButtonIsVisible; set { SetProperty(ref _foldButtonIsVisible, value); } }

        private bool _foldInFrameIsVisible = true;
        public bool FoldInFrameIsVisible { get => _foldInFrameIsVisible; set { SetProperty(ref _foldInFrameIsVisible, value); } }

        private bool _previousEntriesVisible = true;
        public bool PreviousEntriesVisible { get => _previousEntriesVisible; set { SetProperty(ref _previousEntriesVisible, value); } }

        private bool _nextEntriesVisible = true;
        public bool NextEntriesVisible { get => _nextEntriesVisible; set { SetProperty(ref _nextEntriesVisible, value); } }

        POI _selectedPOI;
        public POI SelectedPOI
        {
            get => _selectedPOI; set
            {
                SetProperty(ref _selectedPOI, value);
                ShowEntriesForPOI();
            }
        }

        Entry _selectedEntry;

        public Entry SelectedEntry { get => _selectedEntry; set { SetProperty(ref _selectedEntry, value); } }

        int _currentWeather;

        public int CurrentWeather { get => _currentWeather; set { SetProperty(ref _currentWeather, value); } }


        string _searchBarText;
        public string SearchBarText { get => _searchBarText; set { SetProperty(ref _searchBarText, value); } }

        string _titleResult = "Location";
        public string TitleResult { get => _titleResult; set { SetProperty(ref _titleResult, value); } }

        string _avgRating;

        public string AvgRating { get => _avgRating; set { SetProperty(ref _avgRating, value); } }

        public string LikeButtonNotFilled = @".\Assets\LikeButtonNotFilled.png";

        public string LikeButtonFilled = @".\Assets\LikeButtonFilled.png";


        private PaginationResponse<ObservableCollection<POI>> _listOfPOI;
        public PaginationResponse<ObservableCollection<POI>> ListOfPOI { get => _listOfPOI; set { SetProperty(ref _listOfPOI, value); } }

        private ObservableCollection<Entry> _listOfEntries;
        public ObservableCollection<Entry> ListOfEntries { get => _listOfEntries; set { SetProperty(ref _listOfEntries, value); } }



        #endregion;



        //Methods
        #region Methods
        async Task<bool> AddPOI_Clicked()
        {
            if (!string.IsNullOrEmpty(poiToAdd.Name) && _entryRating > 0 && !string.IsNullOrEmpty(entryToAdd.Description))
            {
                await PopulateEntry();
                poiToAdd.Category = "";

                try
                {
                    //För att får fram Namn och Adress på pinnen man precis skapade
                    var geoCoder = new Geocoder();

                    var fullgeoIEnumerable = await geoCoder.GetAddressesForPositionAsync(ListOfPins.Last().Position);
                    var _fullgeoList = fullgeoIEnumerable.ToList();
                    var _geoString = _fullgeoList[0].ToString();
                    var address = GetAddressFromDataString(_geoString);
                    var city = await GetCityFromDataString(ListOfPins.Last().Position.Longitude,
                                                     ListOfPins.Last().Position.Latitude);

                    ListOfPins.Last().Address = address + ", " + city;
                    ListOfPins.Last().Label = poiToAdd.Name;

                    await _entryClient.Post("entry", entryToAdd);
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Error!", ex.Message, "OK");
                    MapPage.CustomMap.Pins.Remove(MapPage.CustomMap.Pins.Last());
                }
                if (addEntryModalIsVisible)
                {
                    addEntryModalIsVisible = false;
                    await GetPOIList(SelectedPOI.Country, SelectedPOI.City);
                    SelectedPOI = ListOfPOI.Data.FirstOrDefault(x => x.Name == SelectedPOI.Name && x.City == SelectedPOI.City);
                    ListOfEntries = SelectedPOI.Entries;
                }

                addPoiModalIsVisible = false;
                return true;
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Fel", "Du måste fylla i samtliga fält för att kunna lägga till en sevärdhet!", "OK");
            }
            return false;
        }

        private async Task SearchButton_Clicked()
        {
            AvgRating = null;


            TitleResult = SearchBarText[0].ToString().ToUpper() + SearchBarText.Substring(1);
            POIListIsVisible = true;
            if (_titleResult == "Location")
            {
                return;
            }

            Geocoder geoCoder = new Geocoder();

            IEnumerable<Position> approximateLocations = await geoCoder.GetPositionsForAddressAsync(SearchBarText);

            Position position = approximateLocations.FirstOrDefault();

            string coordinates = $"{position.Latitude}, {position.Longitude}";

            var Address = await geoCoder.GetAddressesForPositionAsync(position);
            var country = GetCountryFromDataString(Address.FirstOrDefault());
            var response = await weatherClient.Get("forecast", $"?longitude={position.Longitude}&latitude={position.Latitude}");
            var city = response.Data.City;


            CurrentWeather = (int)Math.Round(response.Data.WeatherList.FirstOrDefault().Temperature);
            MapSpan maps = new MapSpan(position, 1.10, 0.10);
            MapPage.CustomMap.MoveToRegion(maps);

            var test = await GetPOIList(country, city);

            PinStay(test);
        }

        public static MapPageViewModel TestMPVM { get; set; }

        private async Task PinIcon_Clicked()
        {
            pinner = new Pin()
            {
                Label = "",
                Address = "",
                Type = PinType.Place
            };

        }

        public async Task<bool> MapClicked(object sender, MapClickedEventArgs e)
        {
            if (pinner != null)
            {
                pinner.Position = e.Position;
                MapPage.CustomMap.Pins.Add(pinner);

                var ans = await App.Current.MainPage.DisplayAlert("Hej", "Vill du lägga till en pin?", "Ja", "Nej");
                
                if (ans != true)
                {
                    MapPage.CustomMap.Pins.Remove(pinner);
                    pinner = null;
                }
                else
                {
                    TestMPVM.addPoiModalIsVisible = true;
                    await PopulatePOI(e.Position);
                    ListOfPins.Add(pinner);
                    pinner = null;
                    return true;
                }
            }
            return false;
        }

        public void RatingAmount(object sender)
        {
            EntryRating = int.Parse(sender.ToString());
        }

        private async void Pin_MarkerClicked(object sender, PinClickedEventArgs e) //när man klickar på pinnen
        {
            var ans = await App.Current.MainPage.DisplayAlert("Ta bort pin", "Vill du ta bort den valda pin?", "Ja", "Nej");
            if (ans == true)
            {
                var pin = sender as Pin;
                //Mapsample.Pins.Remove(ListOfPins.Where(x => x.Position == pin.Position).FirstOrDefault());
                ListOfPins.Remove(pin);
                addPoiModalIsVisible = false;
            }
        }

        public string GetCountryFromDataString(string dataString)
        {
            string[] separator = { "\r\n" };
            string[] countryFromDataString = dataString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return countryFromDataString.Last();
        }

        public string GetAddressFromDataString(string dataString)
        {
            string[] separator = { "\r\n" };
            string[] countryFromDataString = dataString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return countryFromDataString[0];
        }
        public async Task<string> GetCityFromDataString(double lon, double lat)
        {
            //Fetch city from weatherApi ((hack!)the geocoder doesn't provide a city properly)
            var response = await weatherClient.Get("forecast", $"?longitude={lon}&latitude={lat}");
            var City = response.Data.City;
            return City;
        }

        public async Task PopulatePOI(Position position)
        {
            var geoCoder = new Geocoder();
            var Address = await geoCoder.GetAddressesForPositionAsync(position);

            poiToAdd.Country = GetCountryFromDataString(Address.First());
            poiToAdd.Longitude = position.Longitude;
            poiToAdd.Latitude = position.Latitude;

            //Fetch city from weatherApi ((hack!)the geocoder doesn't provide a city properly)
            var response = await weatherClient.Get("forecast", $"?longitude={poiToAdd.Longitude}&latitude={poiToAdd.Latitude}");
            poiToAdd.City = response.Data.City;


            addPoiModalIsVisible = true;
        }

        public async Task PopulateEntry()
        {
            entryToAdd.POI = poiToAdd;
            entryToAdd.Rating = EntryRating;
        }

        public async Task<PaginationResponse<ObservableCollection<POI>>> GetPOIList(string country, string city)
        {
            return ListOfPOI = await poiListClient.GetList("poi/list", $"?Country={country}&City={city}&amount=50");
        }

        public static event EventHandler<PaginationResponse<ObservableCollection<POI>>> ShowPinsEventHandler;

        protected virtual void OnShowPinsEventHandler(PaginationResponse<ObservableCollection<POI>> ListOfPOI)
        {
            ShowPinsEventHandler?.Invoke(this, ListOfPOI);
        }

        public async void PinStay(PaginationResponse<ObservableCollection<POI>> ListOfPOI)
        {
            MapPage.CustomMap.Pins.Clear();

            foreach (var item in ListOfPOI.Data)
            {
                var geoCoder = new Geocoder();

                Position Position = new Position(item.Latitude, item.Longitude);
                var fullgeoIEnumerable = await geoCoder.GetAddressesForPositionAsync(Position);
                var _fullgeoList = fullgeoIEnumerable.ToList();
                var _geoString = _fullgeoList[0].ToString();
                var address = GetAddressFromDataString(_geoString);

                pinner = new Pin
                {
                    Position = new Position(item.Latitude, item.Longitude),
                    Address = address + ", " + item.City,
                    Label = item.Name,
                };

                ListOfPins.Add(pinner);
            }

            pinner = null;

            OnShowPinsEventHandler(ListOfPOI);
        }

        private async Task<ObservableCollection<Entry>> ShowEntriesForPOI()
        {

            if (SelectedPOI != null)
            {
                POIListIsVisible = false;
                AvgRating = SelectedPOI.AvgRating.ToString();
                EntryListIsVisible = true;
                BackArrowIsVisible = true;
                FoldButtonIsVisible = false;
                EntryButtonIsVisible = true;
                poiToAdd.Name = SelectedPOI.Name;
                TitleResult = SelectedPOI.Name;
                ShowUserLikes();
                EntryPagination = await _entryClient.GetList("entry", $"?amount=6&poi={SelectedPOI.Name.Replace(" ", "+")}");
                PreviousEntriesVisible = true;
                NextEntriesVisible = true;
                if (EntryPagination.PrevPage == null)
                {
                    PreviousEntriesVisible = false;
                }
                if (EntryPagination.NextPage == null)
                {
                    NextEntriesVisible = false;
                }
                ListOfEntries = EntryPagination.Data;
                return ListOfEntries;
            }
            return null;

        }

        private async Task ShowUserLikes()
        {
            ///TODO : implementera fetchUser metoden som finns i MAIN för att sätta x.UserId == UserId.
            var listOfLikesFromUser = ListOfEntries.SelectMany(x => x.LikeDislikeEntries).Where(x => x.UserId == 7).ToList();
            if (listOfLikesFromUser != null)
            {
                foreach (var itemEntry in ListOfEntries)
                {
                    foreach (var itemLike in listOfLikesFromUser)
                    {
                        if (itemEntry.Id == itemLike.EntryId)
                        {
                            itemEntry.LikeButtonImageSource = @"File:.\Assets\LikeButtonFilled.png";
                        }
                    }
                }
            }
        }

        private async Task BackArrowClicked()
        {
            EntryListIsVisible = false;
            POIListIsVisible = true;
            BackArrowIsVisible = false;
            FoldButtonIsVisible = true;
            TitleResult = SelectedPOI.City;
            AvgRating = null;

        }

        private async Task FoldFrameClicked(Frame sender)
        {
            if (sender.IsVisible)
            {
                await sender.FadeTo(0, 500, Easing.SpringOut);
                FoldButtonIsVisible = false;
                FoldInFrameIsVisible = true;
                sender.IsVisible = false;
            }
            else if (!sender.IsVisible)
            {
                FoldButtonIsVisible = true;
                FoldInFrameIsVisible = false;
                sender.IsVisible = true;
                await sender.FadeTo(1, 500, Easing.SpringIn);
            }
        }
        private async Task EntryButton_Clicked()
        {
            addEntryModalIsVisible = true;
            Position position = new Position(SelectedPOI.Latitude, SelectedPOI.Longitude);
            await PopulatePOI(position);

        }
        private async Task LikeButton_Clicked(object sender)
        {
            var selectedEntry = sender as Entry;
            try
            {
                await httpClientLike.Post($"entry/like/{selectedEntry.Id}");

                if (selectedEntry.LikeButtonImageSource == @"File:.\Assets\LikeButtonFilled.png")
                {
                    selectedEntry.LikeButtonImageSource = @"File:.\Assets\LikeButtonNotFilled.png";
                }
                else
                {
                    selectedEntry.LikeButtonImageSource = @"File:.\Assets\LikeButtonFilled.png";
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public PaginationResponse<ObservableCollection<Models.Entry>> EntryPagination { get; set; }

        private async Task OnGetPreviousEntries()
        {
            EntryPagination = await _entryClient.GetList("entry", "?" + EntryPagination.PrevPage.Split('?')[1]);
            ListOfEntries = EntryPagination.Data;
            PreviousEntriesVisible = true;
            NextEntriesVisible = true;
            if (EntryPagination.PrevPage == null)
            {
                PreviousEntriesVisible = false;
            }
            if (EntryPagination.NextPage == null)
            {
                NextEntriesVisible = false;
            }
        }
        private async Task OnGetNextEntries()
        {
            EntryPagination = await _entryClient.GetList("entry", "?" + EntryPagination.NextPage.Split('?')[1]);
            ListOfEntries = EntryPagination.Data;
            PreviousEntriesVisible = true;
            NextEntriesVisible = true;
            if (EntryPagination.PrevPage == null)
            {
                PreviousEntriesVisible = false;
            }
            if (EntryPagination.NextPage == null)
            {
                NextEntriesVisible = false;
            }

        }
        #endregion;
    }
}
