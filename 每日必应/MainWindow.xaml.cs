using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 每日必应
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string source { get; set; }
        int day { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            day = -1;

            source = EverydayBingWallpaper.GetBingwallpaper(day);

            if(source== "日期不合法或超出范围")
            {
                MessageBox.Show(source);
                return;
            }

            ShowImage.Source = new BitmapImage(new Uri(source));

        }
        private void SetWallpaper_Click(object sender, RoutedEventArgs e)
        {
            EverydayBingWallpaper.SetWallpaper(source);
        }

        private void left_Click(object sender, RoutedEventArgs e)
        {
            day += 1;
            source = EverydayBingWallpaper.GetBingwallpaper(day);
            if (source == "日期不合法或超出范围")
            {
                MessageBox.Show(source);
                return;
            }

            ShowImage.Source = new BitmapImage(new Uri(source));
        }

        private void right_Click(object sender, RoutedEventArgs e)
        {
            day -= 1;
            source = EverydayBingWallpaper.GetBingwallpaper(day);
            if (source == "日期不合法或超出范围")
            {
                MessageBox.Show(source);
                return;
            }

            ShowImage.Source = new BitmapImage(new Uri(source));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
            //删除下载的图片
            string path = Directory.GetCurrentDirectory();
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles();

            var query = from s in files
                        where s.Extension == ".jpg"
                        select s;
            foreach(var item in query)
            {
                try
                {
                    File.Delete(item.Name);
                }
                catch(Exception ex)
                {
                    continue;
                }
            }
        }
    }
}
