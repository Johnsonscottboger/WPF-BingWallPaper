using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace 每日必应
{
    public static class Config
    {
        /// <summary>
        /// 读取value
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// 设置value
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static bool SetValue(string key,string value)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if(string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            try
            {
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
