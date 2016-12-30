using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
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
        private string TitleFormat;

        public MainWindow() {
            getImg = new GetImg();
            InitializeComponent();
            if(IsAdmin())
            {
                TitleFormat = "每日必应（管理员）| {0}";
            }else
            {
                TitleFormat = "每日必应 | {0}";
            }
        }

        /// <summary>
        /// 窗口初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_SourceInitialized(object sender,EventArgs e) {
            try {
                this.Title = string.Format(TitleFormat,"正在加载......");
                await GetImageByDay(day,10000);
                if(IsAdmin())
                {
                    SetAutoRun_Click(sender,null);
                    if(IsAutoRun())
                        this.SetAutoRun.Content = "取消开机自启";
                    else
                        this.SetAutoRun.Content = "设置开机自启";
                }
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

        /// <summary>
        /// 快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    this.Title = string.Format(TitleFormat,"网络连接超时");
                    throw new TimeoutException("网络连接超时");
                }
                this.SetWallpaper.ToolTip = result.Corpyright;
                this.Title = string.Format(TitleFormat,result.Corpyright);
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

        /// <summary>
        /// 设置开机自启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAutoRun_Click(Object sender,RoutedEventArgs e)
        {
            MessageBox.Show("触发“设置开机自启”事件");
            if(!IsAdmin())
            {
                RestartAsAdmin();
            }
            AutoRunMethod(AutoRun.Toggle);
        }

        #region 设置开机自启
        /// <summary>
        /// 设置自动启动
        /// </summary>
        private void AutoRunMethod(AutoRun autoRun)
        {
            var startupPath = AppDomain.CurrentDomain.BaseDirectory + "每日必应.exe";
            Microsoft.Win32.RegistryKey local = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey run = local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            try
            {
                var registryName = "AutoRun_EveryDayBing";
                switch(autoRun)
                {
                    case AutoRun.Open:
                    {
                        run.SetValue(registryName,startupPath);
                        break;
                    }
                    case AutoRun.Close:
                    {
                        run.DeleteValue(registryName,false);
                        break;
                    }
                    case AutoRun.Toggle:
                    {
                        if(IsAutoRun())
                        {
                            run.SetValue(registryName,startupPath);
                        }
                        else
                        {
                            run.DeleteValue(registryName,false);
                        }
                        break;
                    }
                    default:
                    {
                        run.DeleteValue(registryName,false);
                        break;
                    }
                }

                local.Close();
                MessageBox.Show("设置成功");
            }
            catch(Exception ex)
            {
                MessageBox.Show("设置失败：\n" + ex.Message);
            }
        }

        /// <summary>
        /// 是否为开机启动
        /// </summary>
        /// <returns></returns>
        private bool IsAutoRun()
        {
            var startupPath = AppDomain.CurrentDomain.BaseDirectory + "每日必应.exe";
            Microsoft.Win32.RegistryKey local = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey run = local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            var registryName = "AutoRun_EveryDayBing";
            return run.GetValue(registryName) != null;
        }

        private enum AutoRun
        {
            Open,Close,Toggle
        }
        #endregion

        #region 以管理员身份重启程序

        /// <summary>
        /// 以管理员身份重启程序
        /// </summary>
        public void RestartAsAdmin()
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "每日必应.exe";
            processStartInfo.Verb = "runas";
            //判断是否已经为管理员身份运行
            try
            {
                System.Diagnostics.Process.Start(processStartInfo);
                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                MessageBox.Show("以管理员重启失败：\n" + ex.Message);
            }
        }

        /// <summary>
        /// 是否已经为管理员身份运行
        /// </summary>
        /// <returns></returns>
        private bool IsAdmin()
        {
            bool isAdmin = false;
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                isAdmin = false;
            }
            return isAdmin;
        }
        #endregion
    }
}
