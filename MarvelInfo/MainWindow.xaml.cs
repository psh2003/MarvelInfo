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

namespace MarvelInfo
{
    public partial class MainWindow : Window
    {
        private const string NextMarvelFilmEndpoint = "https://www.whenisthenextmcufilm.com/api";
        private const string MarvelAPIEndpoint = "https://gateway.marvel.com:443/v1/public/series?";
        private const string apiKey = "69b2dcec13e3059839e0e7a5b957efd9";
        private const string hash = "37c882cceb8777d6616fe502f276c39c396d23ae";

        private HttpClient _httpClient;

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
        }

        private async void LoadMoviesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 다음 마블 영화 정보 호출
                var nextFilm = await GetNextMarvelFilm();

                // 영화 정보를 ListBox에 표시
                NextMarvelFilm nextMarvelFilm = nextFilm;
                /*img.Source = new BitmapImage(new Uri(nextMarvelFilm.poster_url, UriKind.Absolute));*/
                imgTxt.Text = "제목:" + nextMarvelFilm.title + "개봉일:" + nextMarvelFilm.release_date + "타입:" + nextMarvelFilm.type;

                // Marvel API를 사용하여 검색 결과 가져오기
                var characters = await SearchMarvelCharacters(nextMarvelFilm.title);

                // 검색 결과를 ListBox에 표시
                CharactersListBox.ItemsSource = characters;
                
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

                // Marvel API를 사용하여 검색 결과 가져오기
                var characters = await SearchMarvelCharacters(searchTerm);

                // 검색 결과를 ListBox에 표시
                CharactersListBox.ItemsSource = characters;
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

        private async Task<List<NextMarvelFilm>> SearchMarvelCharacters(string searchTerm)
        {
            long ts = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            string hash2 = GetMd5Hash(ts + hash + apiKey);

            var response = await _httpClient.GetAsync($"{MarvelAPIEndpoint}ts={ts}&titleStartsWith={HttpUtility.UrlEncode(searchTerm, Encoding.UTF8)}&limit=10&apikey={apiKey}&hash={hash2}");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var characters = JsonSerializer.Deserialize<List<NextMarvelFilm>>(json);

            return characters;
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

    public class Characters
    {
        public string Name { get; set; }
        public string esourceUri { get; set; }
    }

}
