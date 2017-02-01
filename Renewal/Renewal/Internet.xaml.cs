﻿using mshtml;
using SHDocVw;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Renewal
{
    /// <summary>
    /// Internet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Internet : Window
    {
        private mshtml.HTMLDocument doc;

        #region main
        public Internet()
        {
            InitializeComponent();

            Width = Application.Current.MainWindow.Width;

            Back.Width = Width * 0.95;
            Back.Height = Height / 6 * 0.95;

            Search.Width = Width * 0.95;
            Search.Height = Height / 6 * 0.95;

            Login.Width = Width * 0.95;
            Login.Height = Height / 6 * 0.95;

            Wallpaper.Width = Width * 0.95;
            Wallpaper.Height = Height / 6 * 0.95;


            Favorite.Width = Width * 0.95;
            Favorite.Height = Height / 6 * 0.95;

            Exit.Width = Width * 0.95;
            Exit.Height = Height / 6 * 0.95;
        }
        #endregion

        #region focus
        // 창에 focus 가지 않도록 no activate
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion

        #region initialized
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowInteropHelper helper = new WindowInteropHelper(this);
            IntPtr ip = SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }
        #endregion

        #region backButton
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private void Back_Click(object sender, RoutedEventArgs e) // 뒤로
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();
            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    IE.GoBack();
                }
            }
        }
        #endregion

        #region wallpaper
        // 키보드 이벤트 API
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        const int KEYDOWN = 0x0000;
        const int KEYUP = 0x0002;

        private void Wallpaper_Click(object sender, RoutedEventArgs e)
        {
            const byte Window = 0x5B;// window key
            const byte D = 0x44;// D

            keybd_event(Window, 0, KEYDOWN, 0);
            keybd_event(D, 0, KEYDOWN, 0);
            keybd_event(Window, 0, KEYUP, 0);
            keybd_event(D, 0, KEYUP, 0);
        }
        #endregion

        #region search

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hwnd, int msg, int wParam, StringBuilder sb);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string _ClassName, string _WindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern int FindWindowEx(int _Parent, int _ChildAfter, string _ClassName, string _WindowName);

        public const int WM_GETTEXTLENGTH = 0x000E;
        public const int WM_GETTEXT = 0x000D;
        //button click
        Keyboard dlg;
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            dlg = new Keyboard();
            dlg.Closed += new EventHandler(Keyboard_Closed);
            dlg.Show(); // 키보드 열기
        }
        // search input complete
        void Keyboard_Closed(object sender, EventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();

            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    doc = IE.Document as mshtml.HTMLDocument;
                }
            }
            // Document 속성 읽기
            string title = doc.title;
            string url = doc.url;

            Console.WriteLine("title : " + doc.title);
            // google
            if (title.IndexOf("Google") != -1)
            {
                IHTMLElement q = doc.getElementsByName("q").item("q", 0);
                q.setAttribute("value", Clipboard.GetText());

                IHTMLFormElement form_google = doc.forms.item(Type.Missing, 0);
                form_google.submit();
            }
            else if(title.IndexOf("NAVER") != -1)
            {
                //검색어 셋팅
                IHTMLElement query = doc.getElementsByName("query").item("query", 0);
                query.setAttribute("value", Clipboard.GetText());

                //네이버검색버튼 : search_btn
                doc.getElementById("search_btn").click();
            }
            else if (title.IndexOf(" : 네이버 통합검색") != -1)
            {
                mshtml.IHTMLElementCollection elemColl = null;
                elemColl = doc.getElementsByTagName("button") as mshtml.IHTMLElementCollection;

                foreach (mshtml.IHTMLElement elem in elemColl)
                {
                    if (elem.getAttribute("class") != null)
                    {
                        if (elem.className == "bt_search spim")
                        {
                            IHTMLElement query = doc.getElementsByName("query").item("query", 0);
                            //검색어 셋팅
                            query.setAttribute("value", Clipboard.GetText());
                            elem.click();
                            break;
                        }
                    }
                }
            }
            else if(title.IndexOf("Daum") != -1)
            {
                IHTMLElement q_daum = doc.getElementsByName("q").item("q", 0);
                q_daum.setAttribute("value", Clipboard.GetText());

                IHTMLFormElement form_daum = doc.forms.item(Type.Missing, 0);
                form_daum.submit();
            }
            else
            {
                MessageBox.Show("naver google daum 쓰세요");
            }
        }
        #endregion

        #region favorite
        private void Favorite_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region login

        #endregion

        #region exit click
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            IntPtr handle = GetForegroundWindow();
            foreach (SHDocVw.WebBrowser IE in shellWindows)
            {
                if (IE.HWND.Equals(handle.ToInt32()))
                {
                    IE.Quit();
                    MainWindow.internetCount--;
                    Console.WriteLine("cloase: " + MainWindow.internetCount);
                }
            }
            if(MainWindow.internetCount == 0)
            {
                Close();
                MainWindow.isInternet = false;
            }
        }
        #endregion

        #region area set
        // 윈도우 로드, 클로즈 시 Work area 변경
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.Left);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.None);
        }
        #endregion

        private void Login_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

