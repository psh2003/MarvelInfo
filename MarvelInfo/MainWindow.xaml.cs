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
using System.Web.UI.WebControls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.Xml;
using System.Windows.Forms;
using System.Runtime.Remoting.Contexts;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Win32.TaskScheduler;

namespace MarvelInfo
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string NextMarvelFilmEndpoint = "https://www.whenisthenextmcufilm.com/api";
        private const string MarvelAPIEndpoint = "https://gateway.marvel.com:443/v1/public/series?orderBy=-modified&";
        private const string MarvelAPIEndpoint2 = "https://gateway.marvel.com:443/v1/public/characters?orderBy=-modified&";
        private const string MarvelAPIEndpoint3 = "https://gateway.marvel.com:443/v1/public/events?orderBy=-modified&";
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
                List<Comics> comics = await SearchMarvelComics(nextMarvelFilm.title);

                // 검색 결과를 ListBox에 표시
                Board.Clear();
                foreach (Comics a in comics)
                {
                    var fullFilePath = @a.thumbnail["path"] + "." + a.thumbnail["extension"];

                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                    bitmap.EndInit();
                    Board.Add(new items() { Title = a.title, ImageData = bitmap, Description = a.description, Path = a.urls[a.urls.Count - 1]["url"] });
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
                if (searchTerm.Length < 1)
                {
                    MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);    
                    MessageBox.Show("검색어를 입력해주세요.");
                    return;
                }
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
                            Board.Add(new items() { Title = a.name, ImageData = bitmap, Description = a.description, Path = a.urls[a.urls.Count-1]["url"] });
                        }
                        if (character.Count < 1)
                        {
                            Board.Add(new items() { Title = "검색결과가 없습니다." });
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
                            Board.Add(new items() { Title = a.title, ImageData = bitmap, Description = a.description, Path = a.urls[a.urls.Count - 1]["url"] });
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
                            Board.Add(new items() { Title = a.title, ImageData = bitmap, Description = a.description, Path = a.urls[a.urls.Count - 1]["url"] });
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

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }

        private void ReleaseAlarm(object sender, MouseButtonEventArgs e)
        {
            // 토스트 알림 표시
            using (TaskService taskService = new TaskService())
            {
                // 작업 생성
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.RegistrationInfo.Description = "알림 작업";

                // 트리거 생성
                TimeTrigger trigger = new TimeTrigger(Convert.ToDateTime(releaseDate.Text));
                taskDefinition.Triggers.Add(trigger);

                // 액션 생성
                string appPath = Process.GetCurrentProcess().MainModule.FileName;
                string appDir = Path.GetDirectoryName(appPath);
                string appExe = Path.GetFileName(appPath);
                string arguments = $"/showNotification \"{imgTxt.Text + "이(가) 개봉하였습니다."}\" \"{"예매ㄱㄱ"}\" \"{img.Source}\" -AllowStartIfOnBatteries";
                taskDefinition.Actions.Add(new ExecAction(appExe, arguments, appDir));
                //전원이 연결되어 있지 않을 경우 실행되지 않는걸 방지
                taskDefinition.Settings.StopIfGoingOnBatteries = false;
                taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                // 작업 등록
                taskService.RootFolder.RegisterTaskDefinition("알림작업", taskDefinition);
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("알림이 등록되었습니다.");
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
        public List<Dictionary<string, string>> urls { get; set; }
        public Dictionary<string, string> thumbnail { get; set; }
    }

    public class Character
    {
        public string name { get; set; }
        public string description { get; set; }
        public Dictionary<string, string> thumbnail { get; set; }
        public List<Dictionary<string, string>> urls { get; set; }
    }

    public class Event
    {
        public string title { get; set; }
        public string description { get; set; }
        public Dictionary<string, string> thumbnail { get; set; }
        public List<Dictionary<string, string>> urls { get; set; }
    }

    public class items
    {
        public BitmapImage ImageData { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }

    internal static class MessageBoxHelper
    {
        internal static void PrepToCenterMessageBoxOnForm(Window form)
        {
            MessageBoxCenterHelper helper = new MessageBoxCenterHelper();
            helper.Prep(form);
        }

        private class MessageBoxCenterHelper
        {
            private int messageHook;
            private IntPtr parentFormHandle;

            public void Prep(Window form)
            {
                NativeMethods.CenterMessageCallBackDelegate callBackDelegate = new NativeMethods.CenterMessageCallBackDelegate(CenterMessageCallBack);
                GCHandle.Alloc(callBackDelegate);

                parentFormHandle = new WindowInteropHelper(form).Handle;
                messageHook = NativeMethods.SetWindowsHookEx(5, callBackDelegate, new IntPtr(NativeMethods.GetWindowLong(parentFormHandle, -6)), NativeMethods.GetCurrentThreadId()).ToInt32();
            }

            private int CenterMessageCallBack(int message, int wParam, int lParam)
            {
                NativeMethods.RECT formRect;
                NativeMethods.RECT messageBoxRect;
                int xPos;
                int yPos;

                if (message == 5)
                {
                    NativeMethods.GetWindowRect(parentFormHandle, out formRect);
                    NativeMethods.GetWindowRect(new IntPtr(wParam), out messageBoxRect);

                    xPos = (int)((formRect.Left + (formRect.Right - formRect.Left) / 2) - ((messageBoxRect.Right - messageBoxRect.Left) / 2));
                    yPos = (int)((formRect.Top + (formRect.Bottom - formRect.Top) / 2) - ((messageBoxRect.Bottom - messageBoxRect.Top) / 2));

                    NativeMethods.SetWindowPos(wParam, 0, xPos, yPos, 0, 0, 0x1 | 0x4 | 0x10);
                    NativeMethods.UnhookWindowsHookEx(messageHook);
                }

                return 0;
            }
        }

        private static class NativeMethods
        {
            internal struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            internal delegate int CenterMessageCallBackDelegate(int message, int wParam, int lParam);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool UnhookWindowsHookEx(int hhk);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("kernel32.dll")]
            internal static extern int GetCurrentThreadId();

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr SetWindowsHookEx(int hook, CenterMessageCallBackDelegate callback, IntPtr hMod, int dwThreadId);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        }
    }
}
