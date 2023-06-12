using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace MarvelInfo
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon notifyIcon = new NotifyIcon();
        protected override void OnStartup(StartupEventArgs e)
        {
            // 시스템 트레이 아이콘 초기화
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("../../marvel.ico"); // 아이콘 파일의 경로를 지정합니다.
            notifyIcon.Visible = true;

            base.OnStartup(e);

            if (e.Args.Length > 0 && e.Args[0] == "/showNotification")
            {
                if (e.Args.Length >= 4)
                {
                    string title = e.Args[1];
                    string content = e.Args[2];

                    // 알림 표시
                    notifyIcon.BalloonTipTitle = title;
                    notifyIcon.BalloonTipText = content;
                    notifyIcon.ShowBalloonTip(5000);
                    /*notifyIcon.ShowBalloonTip(5000, "test", "test", ToolTipIcon.Info);*/

                }

                // 알림 표시 후 앱 종료
                Shutdown();
            }
        }   
    }
}
