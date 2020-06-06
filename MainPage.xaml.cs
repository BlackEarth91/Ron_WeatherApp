using Newtonsoft.Json;
using Ron_WeatherApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ron_WeatherApp
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<CompiledWeather> compiledWeather = new ObservableCollection<CompiledWeather>();
        DispatcherTimer Timer = new DispatcherTimer();
        private Windows.UI.Xaml.DispatcherTimer Timer1;
        public string LiveTime => DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
        String apiKey = "63fc070c3b0747159a1d4d9564106c90";
        public MainPage()
        {
            this.InitializeComponent();
            SetTimer();
            showTime();
            fetchWeatherData();
            updateGrid();      
        }

        private void showTime()
        {
            DataContext = this;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
        }
        private void Timer_Tick(object sender, object e)
        {
            Time.Text = "Current Time : " + DateTime.Now.ToString("h:mm:ss tt");
        }


        async void updateGrid()
        {
            try
            {
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("data.json");
                var text = await FileIO.ReadTextAsync(file);
                List<CompiledWeather> cw = JsonConvert.DeserializeObject<List<CompiledWeather>>(text);
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

            //_gridview.ItemsSource = compiledWeather;
        }

    
        async void fetchWeatherData()
        {
            compiledWeather.Clear();
            List<String> countriesSeletion = new List<String>();
            countriesSeletion.Add("Singapore");
            countriesSeletion.Add("Taipei");
            countriesSeletion.Add("Austin");
            countriesSeletion.Add("San Francisco");


            //For each country in the list, fetch api and write into local data file
            
                
             
                HttpClient hc = new HttpClient();
            try
            {
                for (int i = 0; i < countriesSeletion.Count; i++)
                {
                    string url = "http://api.openweathermap.org/data/2.5/weather?q=" + countriesSeletion[i] + "&appid=" + apiKey;
                    String res = await hc.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<Rootobject>(res);
                    Debug.WriteLine("API fetch called");
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
                    var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(compiledWeather));
                    StatusUpdate.Text = "Last Status Update: " + DateTime.Now.ToString("dd/MM/yy h:mm:ss tt");
                }
            }
            catch (HttpRequestException e)
            { 
                var messageDialog = new MessageDialog("No internet connection has been detected, using local data");
                await messageDialog.ShowAsync();
                Debug.WriteLine($"Network not found: '{e}'");
            }

            }             
        
        
        private void SetTimer()
        {
            Timer1 = new Windows.UI.Xaml.DispatcherTimer();
            Timer1.Interval = TimeSpan.FromMilliseconds(60000);
            Timer1.Tick += Timer1_Tick;
            Timer1.Start();
        }

        private void Timer1_Tick(object sender, object e)
        {
            fetchWeatherData();
            updateGrid();
        }




        private void Weather_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
            
            //compiledWeather.RemoveAt(1);
        }
    }
}
