using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace 每日必应
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings :Window
    {
        public Settings()
        {
            InitializeComponent();

            Init();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void Setting_AutoRunToggle_Click(Object sender,RoutedEventArgs e)
        {
            AutoRunMethod(AutoRun.Toggle);
            Init();
        }

        private void Init()
        {
            if(IsAutoRun())
            {
                this.Setting_AutoRunToggle.Content = "取消";
            }
            else
            {
                this.Setting_AutoRunToggle.Content = "设置";
            }
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
                            run.DeleteValue(registryName,false);
                            if(IsAutoRun())
                            {
                                MessageBox.Show("未成功删除注册表项");
                            }
                        }
                        else
                        {
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
