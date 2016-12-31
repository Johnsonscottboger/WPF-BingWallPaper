using System;
using System.Collections.Generic;
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

namespace Settings
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow :Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if(IsAutoRun())
            {
                this.Setting_AutoRunToggle.Content = "取消";
            }
            else
            {
                this.Setting_AutoRunToggle.Content = "设置";
            }
        }

        /// <summary>
        /// 设置-开机自启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setting_AutoRunToggle_Click(Object sender,RoutedEventArgs e)
        {
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
                            MessageBox.Show("删除开机自启");
                            run.DeleteValue(registryName,false);
                            if(IsAutoRun())
                            {
                                MessageBox.Show("未删除注册表项");
                            }
                        }
                        else
                        {
                            MessageBox.Show("设置开机启动");
                            run.SetValue(registryName,startupPath);
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
            Open, Close, Toggle
        }
        #endregion
    }
}
