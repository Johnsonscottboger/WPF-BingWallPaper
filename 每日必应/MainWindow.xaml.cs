using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace 每日必应 {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow :Window {
        private static int day = -1;
        private readonly GetImg getImg;
        private BingImage result;

        public MainWindow() {
            getImg = new GetImg();
            InitializeComponent();

            
        }

        /// <summary>
        /// 窗口初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_SourceInitialized(object sender,EventArgs e) {
            try {
                this.Title = "每日必应 | 正在加载......";
                await GetImageByDay(day,10000);
            }
            catch(Exception ex) {
                if(ex.InnerException != null)
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
        private async void SetWallpaper_Click(object sender,RoutedEventArgs e) {
            try {
                await getImg.SetWallpaper(result);
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 前一天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void left_Click(object sender,RoutedEventArgs e) {
            day += 1;
            try {
                await GetImageByDay(day,10000);
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
                day -= 1;
            }
        }

        /// <summary>
        /// 后一天
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void right_Click(object sender,RoutedEventArgs e) {
            day -= 1;
            try {
                await GetImageByDay(day,10000);
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
                day += 1;   //回滚
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Closed(object sender,EventArgs e) {
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

            await Task.Factory.StartNew(() => {
                files.Where(p => p.Extension == ".jpg").ToList().ForEach(p => {
                    try {
                        p.Delete();
                    }
                    catch(Exception) {

                    }
                });
            });
        }

        /// <summary>
        /// 打开文件夹位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenDownloadDir_Click(object sender,RoutedEventArgs e)
        {
            string path = getImg.bingDownloadDir;
            System.Diagnostics.Process.Start(@"C:\Windows\explorer.exe",path);
        }

        private void Window_KeyUp(object sender,System.Windows.Input.KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Left) {
                left_Click(sender,null);
            }
            else if(e.Key == System.Windows.Input.Key.Right) {
                right_Click(sender,null);
            }
            else if(e.Key == System.Windows.Input.Key.Enter) {
                SetWallpaper_Click(sender,null);
            }
        }


        /// <summary>
        /// 通过天获取图片
        /// </summary>
        /// <param name="day">天数</param>
        private async Task GetImageByDay(int day,int timeout) {
#if DEBUG
            timeout = 3600*1000;
#endif
            try {
                var getBingImgUriTask = getImg.GetBingImgUri(day);
                if(getBingImgUriTask == await Task.WhenAny(getBingImgUriTask,Task.Delay(timeout))) {
                    result = await getBingImgUriTask;
                }
                else {
                    this.Title = "每日必应 | 网络连接超时";
                    throw new TimeoutException("网络连接超时");
                }
                this.SetWallpaper.ToolTip = result.Corpyright;
                this.Title = "每日必应 | " + result.Corpyright;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(result.Bytes);
                bitmapImage.EndInit();
                ShowImage.Source = bitmapImage;
            }
            catch(Exception ex) {
                throw ex;
            }
        }
    }
}
