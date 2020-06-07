using Newtonsoft.Json;
using Ron_WeatherApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static Ron_WeatherApp.Model.OpenMapsHelper;


namespace Ron_WeatherApp
{

    /// <summary>
    /// The weather app fetches the OpenWeather API to display the
    /// current, low, and high temperatures, the humidity, and sunrise/sunset times and displays a selection of countries weather
    /// data in a GridView
    /// 
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<CompiledWeather> compiledWeather = new ObservableCollection<CompiledWeather>();
        private ObservableCollection<Countries> compiledCountries = new ObservableCollection<Countries>();
        DispatcherTimer Timer = new DispatcherTimer();
        ToggleSwitch toggleSwitch = new ToggleSwitch();
        private Windows.UI.Xaml.DispatcherTimer Timer1;
        public string LiveTime => DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
        String apiKey = "63fc070c3b0747159a1d4d9564106c90";

        public MainPage()
        {
            this.InitializeComponent();
            Init_App();
            Thread.Sleep(200); //Wait for Init_App to create Country Selection list
            SetTimer();
            startTime();
            fetchWeatherData();
            updateGrid();
            CountrySelection_Reader();
        }
        /// <remarks>
        ///  This function is first called to initialize the country selection json file
        ///  Values default to Singapore, Taipei, Austin and San Francisco
        /// </remarks>
        async void Init_App()
        {
            StorageFile file;
            try
            {
                file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("countrySel.json");
            }
            catch (FileNotFoundException)
            {
                file = null;
            }

            if (file == null)
            {
                List<Countries> countries = new List<Countries>();
                countries.Add(new Countries { Country = "Singapore" });
                countries.Add(new Countries { Country = "Taipei" });
                countries.Add(new Countries { Country = "Austin" });
                countries.Add(new Countries { Country = "San Francisco" });
         
                var countselfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("countrySel.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(countselfile, JsonConvert.SerializeObject(countries));
               
                Debug.WriteLine("File not found");
            }
            else {
                Debug.WriteLine("File exists");

            } 

        }
        
        /// <remarks>
        ///  This function starts the timer for the current time control
        /// </remarks>
        private void startTime()
        {
            DataContext = this;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
        }
        /// <remarks>
        ///  This function updates the current time control in the form of h:mm:ss tt using system time
        /// </remarks>
        private void Timer_Tick(object sender, object e)
        {
            Time.Text = "Current Time : " + DateTime.Now.ToString("h:mm:ss tt");
        }

        /// <summary>
        /// This function reads and parses the json file stored in {PackageName}/data.json
        /// Updates the data bind on the GridView UI
        /// </summary>
        /// <remarks>
        ///  Call this method to update the GridView, user must be administrator access to read the file
        ///  Input : {PackageName}/data.json
        ///  Object Out: CompiledWeather {temp, temp_min, temp_max, humidity, sunrise, sunset, name, icon, cod, last_update}
        /// </remarks>
        async void updateGrid()
        {
            try
            {
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("data.json");
                var text = await FileIO.ReadTextAsync(file);
                List<CompiledWeather> cw = JsonConvert.DeserializeObject<List<CompiledWeather>>(text);
                //Using CompiledWeather last_update field if app is offline
                foreach (CompiledWeather cwobj in cw)
                {
                    StatusUpdate.Text = "Last Status Update: " + cwobj.last_update;         
                }
                
                _gridview.ItemsSource = cw;
                
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine($"The file was not found: '{e}'");
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.WriteLine($"The directory was not found: '{e}'");
            }
            catch (IOException e)
            {  
                Debug.WriteLine($"The file could not be opened: '{e}'");
            }

        }
        /// <summary>
        /// This function reads off the countries selected to be displayed and fetches the API from OpenWeather
        /// the serialized objects are written to a data.json in the local package folder
        /// </summary>
        /// <remarks>
        ///  Call this method to update the GridView, user must be administrator access to write the file
        ///  Input : countriesSelection List, API OpenWeatherMap 
        ///  Object In: RootObject
        ///  Object Out: CompiledWeather {temp, temp_min, temp_max, humidity, sunrise, sunset, name, icon, cod, last_update}
        /// </remarks>
        async void fetchWeatherData()
        {
            compiledWeather.Clear();
            
            List<String> countriesSeletion = new List<String>();
            countriesSeletion.Clear();
           
            String[] outarr = output.Split(",");
            for (int i = 0; i < outarr.Length - 1; i++)
            {
             
                countriesSeletion.Add(outarr[i]);
            } 
          
            HttpClient hc = new HttpClient();
            try
            {
                for (int i = 0; i < countriesSeletion.Count; i++)
                {
                    string url = "http://api.openweathermap.org/data/2.5/weather?q=" + countriesSeletion[i] + "&appid=" + apiKey;
                    String res = await hc.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<Rootobject>(res);
                    // Debug.WriteLine("API Fetched");
                    List<Weather> li = result.weather;
                    String iconfetched = "";
                    foreach (Weather e in li)
                    {
                        iconfetched = e.icon;
                    }
                    compiledWeather.Add(new CompiledWeather
                    {
                        name = result.name,
                        temp = OpenMapsHelper.KevDegConvert(result.main.temp),
                        temp_min = KevDegConvert(result.main.temp_min),
                        temp_max = KevDegConvert(result.main.temp_max),
                        sunrise = OpenMapsHelper.UnixTimeToDateTime(result.sys.sunrise),
                        sunset = OpenMapsHelper.UnixTimeToDateTime(result.sys.sunset),
                        humidity = result.main.humidity,
                        icon = "Assets/" + iconfetched + ".png",
                        cod = result.cod,
                        last_update = DateTime.Now.ToString("dd/MM/yy h:mm:ss tt")
                    });
                    try { 
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(compiledWeather));
                }
                    catch (Exception e)
                    {

                        Debug.WriteLine(e);
                    }
                    StatusUpdate.Text = "Last Status Update: " + DateTime.Now.ToString("dd/MM/yy h:mm:ss tt");
                }
            }
            catch (HttpRequestException e)
            { 
                var messageDialog = new MessageDialog("No internet connection has been detected");
                await messageDialog.ShowAsync();
                Debug.WriteLine($"Network not found: '{e}'");
            }

        }

        /// <summary>
        /// This function initializes a timer to a 60 seconds interval
        /// </summary>
        private void SetTimer()
        {
            Timer1 = new Windows.UI.Xaml.DispatcherTimer();
            Timer1.Interval = TimeSpan.FromMilliseconds(10000);
            Timer1.Tick += Timer1_Tick;
            Timer1.Start();
        }
        /// <summary>
        /// This function triggers the fetch weather function and updates the grid control after 60 seconds interval reached
        /// </summary>
        private void Timer1_Tick(object sender, object e)
        {
            fetchWeatherData();
            updateGrid();
            
        }
        String output = "";

        List<Countries> countriesList = new List<Countries>();

        async void CountrySelection_Reader()
        {
            output = "";
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync("countrySel.json");
            var text = await FileIO.ReadTextAsync(file);
            List<Countries> countriesList = JsonConvert.DeserializeObject<List<Countries>>(text);
            for (int i = 0; i < countriesList.Count; i++)
            {
                output += countriesList[i].Country + ",";
                compiledCountries.Add(new Countries { Country = countriesList[i].Country });
            }
            CountryListBox.ItemsSource = compiledCountries;

        }

        /// <summary>
        /// This function writes the country selection list to a file and validates against existing list
        /// </summary>
        async void CountrySelection_Writer(String input) {
            //Should match regex (must only be alphanumerical and letters)
            if (!input.Equals(""))
            {
                output = "";
                compiledCountries.Clear();
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile readfile = await folder.GetFileAsync("countrySel.json");
                var text = await FileIO.ReadTextAsync(readfile);
                countriesList = JsonConvert.DeserializeObject<List<Countries>>(text);
                for (int i = 0; i < countriesList.Count; i++)
                {
                    output += countriesList[i].Country + " ";
                    compiledCountries.Add(new Countries { Country = countriesList[i].Country });
                }

                CountryListBox.ItemsSource = compiledCountries;

                try
                {

                    if (countriesList.Any(n => n.Country == input))
                    {
                        var messageDialog = new MessageDialog("Country already exists");
                        await messageDialog.ShowAsync();
                    }

                    else
                    {

                        countriesList.Add(new Countries { Country = input });
                        var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("countrySel.json", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(countriesList));

                    }
                }
                catch (NullReferenceException e)
                {
                    Debug.WriteLine("Null refence exception " + e);
                }
                CountrySelection_Reader();
            }
        }

 
        private void WeatherImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Image img = sender as Image;
            BitmapImage fallbackImage = new BitmapImage(new Uri("ms-appx:///Images/fallback.png"));
            img.Width = 100; //set to known width of this source's natural size
                             //might instead want image to stretch to fill, depends on scenario
            img.Source = fallbackImage;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            fetchWeatherData();
            updateGrid(); 
        }   
        private void AutoUpdateToggle_Toggled(object sender, RoutedEventArgs e)
        {            
            toggleSwitch = sender as ToggleSwitch;                  
        }

        private void btn_CountryAdd_Click(object sender, RoutedEventArgs e)
        {
            String input = textBox_CountryField.Text;         
            CountrySelection_Writer(input);
            fetchWeatherData();
        }
    }
}
