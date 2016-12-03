using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 每日必应
{
    public class GetImg
    {
        private string imgBaseApi = "http://cn.bing.com/HPImageArchive.aspx?format=js&idx={0}&n=1&nc=1480678894463&pid=hp&scope=web&FORM=HDRSC1&video=1";
        public string imgPath { get; set; }
        public string bingDownloadDir = Directory.GetCurrentDirectory() + "\\BingDownload";

        public async Task<Image> GetBingImgUri(int day)
        {
            var json = await GetBingJsonAsync(day);
            if (json == "null")
                throw new Exception("日期超出范围");
            var bingObj = await GetBingObjectDeserialJsonAsync(json);
            if (bingObj == null)
                throw new Exception("反序列化Json时出现异常");
            await DownloadImgByBingObjectAsync(bingObj);
            return bingObj.images[0];
        }

        /// <summary>
        /// 获取Json数据
        /// </summary>
        /// <param name="day">天</param>
        /// <returns></returns>
        public async Task<string> GetBingJsonAsync(int day)
        {
            string imgApi = string.Format(imgBaseApi, day);
            var uri = new Uri(imgApi);

            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string json = string.Empty;
            try
            {
                json = await webClient.DownloadStringTaskAsync(uri);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return json;
        }

        /// <summary>
        /// 获取Bing返回的Json对象
        /// </summary>
        /// <param name="bingJson"></param>
        /// <returns></returns>
        public async Task<BingRootobject> GetBingObjectDeserialJsonAsync(string bingJson)
        {
            if (string.IsNullOrEmpty(bingJson))
                return null;
            return await Task.Factory.StartNew(()=> JsonConvert.DeserializeObject<BingRootobject>(bingJson));
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="bingObj">Bing对象</param>
        /// <returns></returns>
        public async Task DownloadImgByBingObjectAsync(BingRootobject bingObj)
        {
            if (bingObj == null)
                throw new ArgumentNullException(nameof(bingObj));
            using (var webClient = new WebClient())
            {
                imgPath = Path.GetFileName(GetImgName(bingObj.images[0].url));
                //创建Download文件夹
                if (!Directory.Exists(bingDownloadDir))
                {
                    await Task.Factory.StartNew(()=> Directory.CreateDirectory(bingDownloadDir).Attributes = FileAttributes.Hidden);
                }
                string path = bingDownloadDir+"\\"  + imgPath;

                if (!File.Exists(path))
                {
                    await webClient.DownloadFileTaskAsync(bingObj.images[0].url, path);
                }
            }
        }

        /// <summary>
        /// 获取图片名称
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public string GetImgName(string url)
        {
            return url.Split('/').AsParallel().Where(p => p.Contains(".jpg")).FirstOrDefault();
        }

        /// <summary>
        /// 设为壁纸
        /// </summary>
        /// <param name="uAction"></param>
        /// <param name="uParam"></param>
        /// <param name="lpvParm"></param>
        /// <param name="fuWinIni"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParm, int fuWinIni);
        public static void SetWallpaper(GetImg getImg)
        {
            string path = getImg.bingDownloadDir + "\\" + getImg.imgPath;
            SystemParametersInfo(20, 0, path, 0x01 | 0x02);
        }
    }
}
