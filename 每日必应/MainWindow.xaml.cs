using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace 每日必应
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image result;
        private static int day = -1;
        private static readonly GetImg getImg = new GetImg();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                this.Title = "每日必应 | 正在加载......";
                
                await GetImageByDay(day, 10000);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    MessageBox.Show(ex.InnerException.Message);
                else
                    MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 设置为壁纸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SetWallpaper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Task.Factory.StartNew(() => GetImg.SetWallpaper(getImg));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void left_Click(object sender, RoutedEventArgs e)
        {
            day += 1;
            try
            {
                await GetImageByDay(day, 10000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                day -= 1;
            }
        }

        private async void right_Click(object sender, RoutedEventArgs e)
        {
            day -= 1;
            try
            {
                await GetImageByDay(day, 10000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                day += 1;   //回滚
            }
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key==System.Windows.Input.Key.Left)
            {
                left_Click(sender, null);
            }
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                right_Click(sender, null);
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                SetWallpaper_Click(sender, null);
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            //删除下载的图片
            string path = getImg.bingDownloadDir;
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles();

            #region 替换为Lambda方法语法
            //var query = from s in files
            //            where s.Extension == ".bmp"
            //            select s;
            //foreach (var item in query)
            //{
            //    try
            //    {
            //        item.Delete();
            //    }
            //    catch (Exception ex)
            //    {
            //        continue;
            //    }
            //}
            #endregion

            await Task.Factory.StartNew(() =>
            {
                files.Where(p => p.Extension == ".bmp").AsParallel().ForAll(p =>
                {
                    try
                    {
                        p.Delete();
                    }
                    catch (Exception)
                    {

                    }
                });
            });
        }



        /// <summary>
        /// 通过天获取图片
        /// </summary>
        /// <param name="day">天数</param>
        private async Task GetImageByDay(int day, int timeout)
        {
            try
            {
                var getBingImgUriTask= getImg.GetBingImgUri(day);
                if (getBingImgUriTask == await Task.WhenAny(getBingImgUriTask, Task.Delay(timeout)))
                {
                    result = await getBingImgUriTask;
                }
                else
                {
                    this.Title = "每日必应 | 网络连接超时";
                    throw new TimeoutException("网络连接超时");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            string path = getImg.GetImgPath();
            try
            {
                this.SetWallpaper.ToolTip = result.copyright;
                this.Title = "每日必应 | " + result.copyright;
                ShowImage.Source = new BitmapImage(new Uri(path));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void OpenDownloadDir_Click(object sender, RoutedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory() + "\\BingDownload";
            System.Diagnostics.Process.Start(@"C:\Windows\explorer.exe", path);
        }
    }
}
