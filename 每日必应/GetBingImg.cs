using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace 每日必应 {
    public class GetImg {
        public string bingDownloadDir
        {
            get { return Config.GetValue("ImagePath") ?? Directory.GetCurrentDirectory() + "\\BingDownload"; }
            set { }
        }
        private readonly string hostName = "http://cn.bing.com";
        private readonly string imgBaseApi = "http://cn.bing.com/HPImageArchive.aspx?format=js&idx={0}&n=1&nc=1480678894463&pid=hp&scope=web&FORM=HDRSC1&video=1";

        BingImage _bingImage;

        public async Task<BingImage> GetBingImgUri(int day) {

            var json = await GetBingJsonAsync(day);
            if(json == "null")
                throw new Exception("日期超出范围");
            var bingObj = await GetBingObjectDeserialJsonAsync(json);
            if(bingObj == null)
                throw new Exception("反序列化Json时出现异常");
            var imgBytes = await GetBingImgBytesAsync(hostName + bingObj.images[0].url);
            if(imgBytes == null || imgBytes.Length == 0)
                throw new Exception("加载图片失败");

            _bingImage = new BingImage() {
                ImgUrl = bingObj.images[0].url,
                ImgPath = Path.GetFileName(GetImgName(bingObj.images[0].url)),
                ImgFullPath = bingDownloadDir + "\\" + Path.GetFileName(GetImgName(bingObj.images[0].url)),
                Corpyright = bingObj.images[0].copyright,
                Bytes = imgBytes
            };

            return _bingImage;
        }

        /// <summary>
        /// 获取Json数据
        /// </summary>
        /// <param name="day">天</param>
        /// <returns></returns>
        private async Task<string> GetBingJsonAsync(int day) {
            string imgApi = string.Format(imgBaseApi,day);
            var uri = new Uri(imgApi);

            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            string json = string.Empty;
            try {
                json = await webClient.DownloadStringTaskAsync(uri);
            }
            catch(Exception ex) {
                throw ex;
            }
            return json;
        }

        /// <summary>
        /// 获取Bing返回的Json对象
        /// </summary>
        /// <param name="bingJson">Json</param>
        /// <returns></returns>
        private async Task<BingRootobject> GetBingObjectDeserialJsonAsync(string json) {
            if(string.IsNullOrEmpty(json))
                return null;
            return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<BingRootobject>(json));
        }

        /// <summary>
        /// 获取图片字节数组
        /// </summary>
        /// <param name="imgUrl"></param>
        /// <returns></returns>
        private async Task<byte[]> GetBingImgBytesAsync(string imgUrl) {
            if(imgUrl == null)
                throw new ArgumentNullException(nameof(imgUrl));
            using(var webClient=new WebClient()) {
                return await webClient.DownloadDataTaskAsync(imgUrl);
            }
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="imgUrl">图片链接</param>
        /// <param name="imgPath">图片保存路径</param>
        /// <returns></returns>
        private async Task DownloadBingImagAsync(string imgUrl,string imgPath,string downloadDir = null) {
            if(imgUrl == null)
                throw new ArgumentNullException(nameof(imgUrl));
            if(imgPath == null)
                throw new ArgumentNullException(nameof(imgPath));
            using(var webClient = new WebClient()) {
                if(downloadDir != null) {
                    if(!Directory.Exists(downloadDir))
                        await Task.Factory.StartNew(() => Directory.CreateDirectory(bingDownloadDir).Attributes = FileAttributes.Hidden);
                }

                if(!File.Exists(imgPath)) {
                    await webClient.DownloadFileTaskAsync(imgUrl,imgPath);
                }
            }
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="imgUrl">图片链接</param>
        /// <param name="imgPath">图片保存路径</param>
        /// <returns></returns>
        private async Task DownloadBingImagAsync(byte[] bytes,string imgPath,string downloadDir=null) {
            if(bytes == null || bytes.Length == 0)
                throw new ArgumentNullException(nameof(bytes));
            if(imgPath == null)
                throw new ArgumentNullException(nameof(imgPath));
            if(downloadDir != null) {
                if(!Directory.Exists(downloadDir))
                    await Task.Factory.StartNew(() => Directory.CreateDirectory(bingDownloadDir).Attributes = FileAttributes.Hidden);
            }
            if(!File.Exists(imgPath)) {
                using(var fileStream=new FileStream(imgPath,FileMode.Create)) {
                    await fileStream.WriteAsync(bytes,0,bytes.Length);
                }
            }
        }


        /// <summary>
        /// 设置为壁纸
        /// </summary>
        /// <param name="imgPath">图片路径</param>
        /// <returns></returns>
        public async Task SetWallpaper(BingImage image) {
            if(image == null)
                throw new ArgumentNullException(nameof(image));
            if(!File.Exists(image.ImgFullPath)) {
                await DownloadBingImagAsync(image.Bytes,image.ImgFullPath,bingDownloadDir);
            }
            await Task.Factory.StartNew(() => GetImg.SetWallpaperDll(image.ImgFullPath));
        }

        #region 辅助
        [DllImport("user32.dll",EntryPoint = "SystemParametersInfo")]
        private static extern bool SystemParametersInfo(int uAction,int uParam,string lpvParm,int fuWinIni);
        private static void SetWallpaperDll(string path) {
            if(string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            var success = SystemParametersInfo(20,0,path,0x01 | 0x02);
            if(!success)
                throw new Exception("设置失败，请在 Windows 10 系统中使用，也可以尝试打开下载位置！");
        }

        /// <summary>
        /// 获取图片名称
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns></returns>
        public string GetImgName(string url) {
            return url.Split('/').AsParallel().Where(p => p.Contains(".jpg")).FirstOrDefault();
        }
        
        #endregion
    }

    public class BingImage {
        public string ImgPath { get; set; }
        public string ImgFullPath { get; set; }
        public string ImgUrl { get; set; }
        public string Corpyright { get; set; }
        public byte[] Bytes { get; set; }
    }
}
