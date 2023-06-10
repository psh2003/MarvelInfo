using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Windows.Media.Imaging;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows.Input;

namespace MarvelInfo
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string NextMarvelFilmEndpoint = "https://www.whenisthenextmcufilm.com/api";
        private const string MarvelAPIEndpoint = "https://gateway.marvel.com:443/v1/public/series?";
        private const string MarvelAPIEndpoint2 = "https://gateway.marvel.com:443/v1/public/characters?";
        private const string MarvelAPIEndpoint3 = "https://gateway.marvel.com:443/v1/public/events?";
        private const string apiKey = "69b2dcec13e3059839e0e7a5b957efd9";
        private const string hash = "37c882cceb8777d6616fe502f276c39c396d23ae";

        private HttpClient _httpClient;
        private ObservableCollection<items> _board;

        public ObservableCollection<items> Board
        {
            get { return _board; }
            set
            {
                _board = value;
                OnPropertyChanged(nameof(Board));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }



        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            Board = new ObservableCollection<items>();
            this.DataContext = this;
        }

        private async void LoadMoviesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 다음 마블 영화 정보 호출
                var nextFilm = await GetNextMarvelFilm();

                // 영화 정보를 ListBox에 표시
                NextMarvelFilm nextMarvelFilm = nextFilm;

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(nextFilm.poster_url, UriKind.Absolute);
                bitmap.EndInit();

                img.Source = bitmap;
                imgTxt.Text = nextMarvelFilm.title;
                overview.Text = nextMarvelFilm.overview;
                releaseDate.Text = nextMarvelFilm.release_date.ToString();
                untilDate.Text = nextMarvelFilm.days_until.ToString();
                type.Text = nextMarvelFilm.type;


                // Marvel API를 사용하여 검색 결과 가져오기
                List<Comics> characters = await SearchMarvelComics(nextMarvelFilm.title);

                // 검색 결과를 ListBox에 표시
                /*CharactersListBox.ItemsSource = characters;*/
                Board.Clear();
                foreach (Comics a in characters)
                {
                    var fullFilePath = @a.thumbnail["path"] + "." + a.thumbnail["extension"];

                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                    bitmap.EndInit();
                    Board.Add(new items() { Title = a.title, ImageData = bitmap, Description = a.description});
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load movie: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 텍스트 박스에서 검색어 가져오기
                string searchTerm = SearchTextBox.Text;
                BitmapImage bitmap = new BitmapImage();
                Board.Clear();
                // Marvel API를 사용하여 검색 결과 가져오기
                switch (combo.Text)
                {
                    case "캐릭터":
                        List<Character> character = await SearchMarvelCharacter(searchTerm);
                        foreach (Character a in character)
                        {
                            var fullFilePath = @a.thumbnail["path"] + "." + a.thumbnail["extension"];

                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                            bitmap.EndInit();
                            Board.Add(new items() { Title = a.name, ImageData = bitmap, Description = a.description });
                        }
                        break;
                    case "사건":
                        List<Event> _event = await SearchMarvelEvent(searchTerm);
                        foreach (Event a in _event)
                        {
                            var fullFilePath = @a.thumbnail["path"] + "." + a.thumbnail["extension"];

                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                            bitmap.EndInit();
                            Board.Add(new items() { Title = a.title, ImageData = bitmap, Description = a.description });
                        }
                        break;
                    case "코믹스":
                        List<Comics> comics = await SearchMarvelComics(searchTerm);
                        foreach (Comics a in comics)
                        {
                            var fullFilePath = @a.thumbnail["path"] + "." + a.thumbnail["extension"];

                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                            bitmap.EndInit();
                            Board.Add(new items() { Title = a.title, ImageData = bitmap, Description = a.description });
                        }
                        break;
                }               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to perform search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<NextMarvelFilm> GetNextMarvelFilm()
        {
            var response = await _httpClient.GetAsync(NextMarvelFilmEndpoint);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var nextFilm = JsonSerializer.Deserialize<NextMarvelFilm>(json);

            return nextFilm;
        }

        private async Task<List<Comics>> SearchMarvelComics(string searchTerm)
        {
            long ts = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            string hash2 = GetMd5Hash(ts + hash + apiKey);

            var response = await _httpClient.GetAsync($"{MarvelAPIEndpoint}ts={ts}&titleStartsWith={HttpUtility.UrlEncode(searchTerm, Encoding.UTF8)}&limit={limit.Text}&apikey={apiKey}&hash={hash2}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var jsonObject = JsonDocument.Parse(json).RootElement;
            var data = jsonObject.GetProperty("data").GetProperty("results");

            var characters = JsonSerializer.Deserialize<List<Comics>>(data);

            return characters;
        }

        private async Task<List<Character>> SearchMarvelCharacter(string searchTerm)
        {
            long ts = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            string hash2 = GetMd5Hash(ts + hash + apiKey);

            var response = await _httpClient.GetAsync($"{MarvelAPIEndpoint2}ts={ts}&nameStartsWith={HttpUtility.UrlEncode(searchTerm, Encoding.UTF8)}&limit={limit.Text}&apikey={apiKey}&hash={hash2}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var jsonObject = JsonDocument.Parse(json).RootElement;
            var data = jsonObject.GetProperty("data").GetProperty("results");

            var characters = JsonSerializer.Deserialize<List<Character>>(data);

            return characters;
        }

        private async Task<List<Event>> SearchMarvelEvent(string searchTerm)
        {
            long ts = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            string hash2 = GetMd5Hash(ts + hash + apiKey);

            var response = await _httpClient.GetAsync($"{MarvelAPIEndpoint3}ts={ts}&nameStartsWith={HttpUtility.UrlEncode(searchTerm, Encoding.UTF8)}&limit={limit.Text}&apikey={apiKey}&hash={hash2}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var jsonObject = JsonDocument.Parse(json).RootElement;
            var data = jsonObject.GetProperty("data").GetProperty("results");

            var characters = JsonSerializer.Deserialize<List<Event>>(data);

            return characters;
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SearchButton_Click(sender, e);
            }
        }
    }

    public class NextMarvelFilm
    {
        public int days_until { get; set; }
        public string overview { get; set; }
        public string poster_url { get; set; }
        public DateTime release_date { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Comics
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int startYear { get; set; }
        public int endYear { get; set; }
        public string rating { get; set; }
        public Dictionary<string, string> thumbnail { get; set; }
    }

    public class Character
    {
        public string name { get; set; }
        public string description { get; set; }
        public Dictionary<string, string> thumbnail { get; set; }
    }

    public class Event
    {
        public string title { get; set; }
        public string description { get; set; }
        public Dictionary<string, string> thumbnail { get; set; }
    }

    public class items
    {
        public BitmapImage ImageData { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

}
