using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fasterflect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TutorGroup.Utilities.Extensions;

namespace CheckCoimcDataList
{
    public class ImageData
    {

        public string Path { get; set; }
        public DateTime Last { get; set; }
        public string MD5 { get; set; }
        public string CRC32 { get; set; }

    }
    class CRC32
    {
        static UInt32[] crcTable =  
        {  
          0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b, 0x1a864db2, 0x1e475005,  
          0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61, 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd,  
          0x4c11db70, 0x48d0c6c7, 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,  
          0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3, 0x709f7b7a, 0x745e66cd,  
          0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039, 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5,  
          0xbe2b5b58, 0xbaea46ef, 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,  
          0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb, 0xceb42022, 0xca753d95,  
          0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1, 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d,  
          0x34867077, 0x30476dc0, 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,  
          0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 0x0808d07d, 0x0cc9cdca,  
          0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde, 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02,  
          0x5e9f46bf, 0x5a5e5b08, 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,  
          0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc, 0xb6238b25, 0xb2e29692,  
          0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6, 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a,  
          0xe0b41de7, 0xe4750050, 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,  
          0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34, 0xdc3abded, 0xd8fba05a,  
          0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637, 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb,  
          0x4f040d56, 0x4bc510e1, 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,  
          0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5, 0x3f9b762c, 0x3b5a6b9b,  
          0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff, 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623,  
          0xf12f560e, 0xf5ee4bb9, 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,  
          0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd, 0xcda1f604, 0xc960ebb3,  
          0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7, 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b,  
          0x9b3660c6, 0x9ff77d71, 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,  
          0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2, 0x470cdd2b, 0x43cdc09c,  
          0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8, 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24,  
          0x119b4be9, 0x155a565e, 0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,  
          0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a, 0x2d15ebe3, 0x29d4f654,  
          0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0, 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c,  
          0xe3a1cbc1, 0xe760d676, 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,  
          0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662, 0x933eb0bb, 0x97ffad0c,  
          0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668, 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4  
        };

        public static uint GetCRC32(byte[] bytes)
        {
            uint iCount = (uint)bytes.Length;
            uint crc = 0xFFFFFFFF;

            for (uint i = 0; i < iCount; i++)
            {
                crc = (crc << 8) ^ crcTable[(crc >> 24) ^ bytes[i]];
            }

            return crc;
        }
        public static DateTime _payDate;
    }
    public static class DirectoryInfoExtension
    {
        public static List<FileInfo> GetCustomFilesInfos(this DirectoryInfo value, string pattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            string[] ps = pattern.Split('|');
            List<FileInfo> list = new List<FileInfo>();
            foreach (var s in ps)
            {
                list.AddRange(value.GetFiles(s, option));
            }
            return list.OrderBy(r => r.Name).ToList();
        }
        public static List<string> GetCustomFilesPaths(this DirectoryInfo value, string pattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return value.GetCustomFilesInfos(pattern, option).Select(r => r.FullName).ToList();
        }
    }

    public enum CType
    {
        MD5,
        CRC32
    }

    class Program
    {

        private static ConcurrentBag<string> fklist = new ConcurrentBag<string>();
        private static ConcurrentBag<string> errorImage = new ConcurrentBag<string>();
        private static ConcurrentDictionary<string, string> fileck = new ConcurrentDictionary<string, string>();
        private static ConcurrentBag<ImageData> idata = new ConcurrentBag<ImageData>();
        private const string comicDir = "D:/yyq";
        private const string fData = "data.txt";
        private const string crckPath = "crckey.txt";
        private const string fkPath = "fkey.txt";
        const int theads = 20;
        static CType ctype = CType.MD5;
        private static List<int> ttt;


        static int NUM_INTS = 500000000;

        static ParallelQuery<int> GenerateInputeData4Parallel()
        {
            return ParallelEnumerable.Range(1, NUM_INTS);
        }

        static void Main2()
        {
            var palTarget = GenerateInputeData4Parallel();

            Console.WriteLine("============================================================");
            Console.WriteLine("TEST PARALLEL LINQ:   Parallelism = 2");
            Console.WriteLine("============================================================");
            var swatchp = Stopwatch.StartNew();
            ConcurrentBag<int> lst = new ConcurrentBag<int>();
            Parallel.ForEach(palTarget, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, intNum =>
             {
                 if (intNum % 5 == 0)
                 {
                     lst.Add(intNum);
                 }
             });
            swatchp.Stop();

            Console.WriteLine("Parallel Result: " + lst.Count + "    LINQ Use Time: {0}", swatchp.Elapsed);

            Console.WriteLine("============================================================");
            Console.WriteLine("TEST PARALLEL LINQ:   Parallelism = 2");
            Console.WriteLine("============================================================");
            var swatchp2 = Stopwatch.StartNew();

            var palQuery = (from intNum in palTarget.AsParallel().WithDegreeOfParallelism(2)
                            where ((intNum % 5) == 0)
                            select (intNum / Math.PI)).Average();
            swatchp2.Stop();

            Console.WriteLine("PLINQ Result: " + palQuery + "    LINQ Use Time: {0}", swatchp2.Elapsed);


            palTarget = GenerateInputeData4Parallel();
            Console.WriteLine("\n\n");
            Console.WriteLine("============================================================");
            Console.WriteLine("TEST PARALLEL LINQ:   Parallelism = 4");
            Console.WriteLine("============================================================");
            var swatchp4 = Stopwatch.StartNew();

            palQuery = (from intNum in palTarget.AsParallel().WithDegreeOfParallelism(4)
                        where ((intNum % 5) == 0)
                        select (intNum / Math.PI)).Average();
            swatchp4.Stop();

            Console.WriteLine("PLINQ Result: " + palQuery + "    LINQ Use Time: {0}", swatchp4.Elapsed);
            Console.ReadLine();
        }

        private const string ext = @"长图漫画";
        static void Main(string[] args)
        {
            try
            {
                //                MergeComic.Program.CombineImages();
                //                var ii = new ImageData();
                //                var jo = JObject.FromObject(ii, new JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                //Main2();
                //return;
                //                var a = new[] { 0, 1, 0, 1, 0, 2, 0, 0, 2, 1 }.Select((val, index) => new { k = index, v = val });
                //                var b = new[] { 0, 1, 0, 2, 0, 0, 1, 0, 2, 1 }.Select((val, index) => new { k = index, v = val });
                //                var c = a.Intersect(b);
                var datas = JsonConvert.DeserializeObject<ImageData[]>(File.ReadAllText(fData)).ToList();
                if (datas.Any())
                    datas.AsParallel().WithDegreeOfParallelism(theads).ForAll((i) => idata.Add(i));
            }
            catch (Exception) { idata = new ConcurrentBag<ImageData>(); }
            finally { }
            CkComicDir();
            //TestBigImage();
        }

        private static void TestBigImage()
        {
            string tpath = @"E:\yyq\我的邻座是魔王\[48]我的邻座是魔王_36.5胡子先生的二三事[u17][23P][2017-04-06]";
            GetBigImage(new DirectoryInfo(tpath));
        }

        private const int page = 10;
        private static void GetBigImage(DirectoryInfo info)
        {
            return;
            try
            {
                var list = info.GetCustomFilesPaths("*.jpg|*.png|*.gif");
                var dir = string.Format("{0}/comic/{1}", comicDir, info.Parent.Name);
                Directory.CreateDirectory(dir);
                if (list.Count > 0)
                {
                    var images = list.OrderBy(r => r).Select(r => Bitmap.FromFile(r)).ToList();
                    int step = images.Count / page + 1;
                    for (int i = 0; i <= step; i++)
                    {
                        var t = images.Skip(i * page).Take(page).ToList();
                        if (t.Any())
                        {
                            string fname = string.Format("{0}/{3}{1}_{2}.jpg", dir, info.Name, (i + 1).ToString().PadLeft(2, '0'), "", info.Parent.Name + "_");
                            if (File.Exists(fname)) continue;
                            Bitmap img = JoinImage2(t);
                            EncoderParameters ep = new EncoderParameters();
                            long[] qy = new long[1];
                            qy[0] = 20;//设置压缩的比例1-100
                            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                            ep.Param[0] = eParam;
                            try
                            {
                                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                                ImageCodecInfo jpegICIinfo = null;
                                for (int x = 0; x < arrayICI.Length; x++)
                                {
                                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                                    {
                                        jpegICIinfo = arrayICI[x];
                                        break;
                                    }
                                }
                                if (jpegICIinfo != null)
                                {
                                    img.Save(fname, jpegICIinfo, ep);//dFile是压缩后的新路径
                                }
                                else
                                {
                                    img.Save(fname, ImageFormat.Jpeg);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("{2}\r\n:{0}\r\n{1}", ex.Message, ex.StackTrace, info.Name);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{2}\r\n:{0}\r\n{1}", ex.Message, ex.StackTrace, info.Name);
            }
        }
        private static Bitmap JoinImage2(List<Image> imageList)//实现左右拼接图片
        {
            int imgHeight = 0, imgWidth = 0;
            int dimgHeight = 0, dimgWidth = 0;
            try
            {
                imgHeight = imageList.Sum(r => r.Height);
                imgWidth = imageList.Max(r => r.Width);
                Bitmap joinedBitmap = new Bitmap(imgWidth, imgHeight);
                Graphics graph = Graphics.FromImage(joinedBitmap);
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                for (int i = 0; i < imageList.Count; i++)
                {
                    graph.DrawImage(imageList[i], dimgWidth, dimgHeight, imageList[i].Width, imageList[i].Height);
                    dimgHeight += imageList[i].Height;
                }
                return joinedBitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine("imgWidth:{0},imgHeight:{1}", imgWidth, imgHeight);
                return new Bitmap(1, 1);
            }
        }

        private static Image JoinImage(Image Img1, Image Img2)//实现左右拼接图片
        {
            int imgHeight = 0, imgWidth = 0;
            imgWidth = Img1.Width + Img2.Width;
            imgHeight = Math.Max(Img1.Height, Img2.Height);
            Bitmap joinedBitmap = new Bitmap(imgWidth, imgHeight);
            Graphics graph = Graphics.FromImage(joinedBitmap);
            graph.DrawImage(Img1, 0, 0, Img1.Width, Img1.Height);
            graph.DrawImage(Img2, Img1.Width, 0, Img2.Width, Img2.Height);
            return joinedBitmap;
        }
        public static void GetFileMd5(string filePath)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            try
            {
                using (Pipeline pipe = runspace.CreatePipeline())
                {
                    //Get-VM -Name vm001
                    Command cmd = new Command("Get-FileHash");
                    cmd.Parameters.Add("Path", "filePath");
                    cmd.Parameters.Add("Algorithm", "MD5");
                    pipe.Commands.Add(cmd);
                    var result = pipe.Invoke();
                    Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        static void GetMd5(List<string> images)
        {
            images.ForEach((i) => GetFileMd5(i));

            Environment.Exit(0);
        }

        private static bool lockToken = false;
        private static bool errorToken = false;

        static void GetCk(List<string> images)
        {
            //GetMd5(images);
            Console.WriteLine("开始获取文件校验码...");
            int step = images.Count / theads + 1;
            Console.WriteLine("step:{0}", step);
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            //ConcurrentBag<List<string>> tbag = new ConcurrentBag<List<string>>();
            List<List<string>> tbag = new List<List<string>>();

            var ts = Enumerable.Range(0, theads)
                .Select(i => Task.Factory.StartNew(() =>
                {
                    tbag.Add(images.Skip(step * i).Take(step).ToList());
                })).ToArray();
            Task.WaitAll(ts);
            int c = 1;
            /*
            List<Task> tasks = new List<Task>();            
            var e = tbag.GetEnumerator();
            //while (tbag.Count > 0)
            while (e.MoveNext())
            {
                List<string> s;
                //                if (tbag.TryTake(out s))
                s = e.Current;
                {
                    if (s != null)
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            s.ForEach((x) =>
                            {
                                string ck = GetFileCK(x);
                                //fileck.AddOrUpdate(x, ck, ((key, ovalue) => ck));
                            });
                            Console.WriteLine("线程{0}:执行完毕", c++);
                        }));
                }
            }
            Task.WaitAll(tasks.ToArray());
            */

            Parallel.ForEach(tbag, new ParallelOptions() { MaxDegreeOfParallelism = theads }, (list, state) =>
            {
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = theads * 10 }, (s, loopState) =>
                {
                    string ck = GetFileCK(s);
                });
                Console.WriteLine("线程{0}:执行完毕", c++);
            });
            sw.Stop();
            Console.WriteLine("\r\n取文件校验码 总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
            Console.WriteLine("===============================================");
            File.WriteAllText(fData, JsonConvert.SerializeObject(idata.ToArray()));
        }

        static string GetFileCK(string filePath)
        {
            string ret = "";
            ImageData i = idata.FirstOrDefault(r => r.Path == filePath);
            if (i == null)
            {
                i = new ImageData() { Path = filePath };
                FileInfo f = new FileInfo(filePath);
                i.Last = f.LastWriteTime;
                idata.Add(i);
                switch (ctype)
                {
                    case CType.CRC32:
                        i.CRC32 = CRC32.GetCRC32(File.ReadAllBytes(filePath)).ToGetValue("");
                        ret = i.CRC32; break;
                    case CType.MD5:
                        i.MD5 = GetMD5HashFromFile(filePath);
                        ret = i.MD5; break;
                }
            }
            else
            {
                if (i.Last == new FileInfo(filePath).LastWriteTime)
                {
                    switch (ctype)
                    {
                        case CType.CRC32:
                            if (i.CRC32 == null) i.CRC32 = CRC32.GetCRC32(File.ReadAllBytes(filePath)).ToGetValue("");
                            ret = i.CRC32; break;
                        case CType.MD5:
                            if (i.MD5 == null) i.MD5 = GetMD5HashFromFile(filePath);
                            ret = i.MD5; break;
                    }
                }
                else
                {
                    i.Last = new FileInfo(filePath).LastWriteTime;
                    switch (ctype)
                    {
                        case CType.CRC32:
                            i.CRC32 = CRC32.GetCRC32(File.ReadAllBytes(filePath)).ToGetValue("");
                            ret = i.CRC32; break;
                        case CType.MD5:
                            i.MD5 = GetMD5HashFromFile(filePath);
                            ret = i.MD5; break;
                    }
                }
            }
            /*
            if (idata.Exists(r => r.Path == i.Path))
            {
                i = idata.SingleOrDefault(r => r.Path == i.Path);
                FileInfo f = new FileInfo(filePath);
                if (f.LastWriteTime == i.Last)
                {

                }
            }
            switch (ctype)
            {
                case CType.CRC32:
                    ret = CRC32.GetCRC32(File.ReadAllBytes(filePath)).ToGetValue("");
                    break;
                case CType.MD5:
                default:
                    ret = GetMD5HashFromFile(filePath);
                    break;
            }
            */
            return ret;

        }
        static void CkImages(List<string> images)
        {
            Console.WriteLine("选择校验方式：1.MD5 2.CRC32 默认MD5");
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2)
            {
                ctype = CType.CRC32;
            }
            Console.WriteLine("校验方式：" + ctype.ToString());
            var path = "";
            switch (ctype)
            {
                case CType.CRC32:
                    path = crckPath;
                    break;
                case CType.MD5:
                    path = fkPath;
                    break;
            }
            if (fklist.Count == 0 && File.Exists(path))
            {
                var flst = File.ReadAllLines(path).Where(r => r.Trim().Length > 0).Select(r => r.Trim()).Distinct().ToList();
                flst.AsParallel().ForAll((item) => { fklist.Add(item); });
            }
            GetCk(images);

            TextWriter tw = Console.Out;

            int currentT = Console.CursorTop;
            int step = images.Count / theads + 1;
            Stopwatch sw = new Stopwatch();
            TextWriter twe = Console.Out;

            #region 新方法
            sw.Restart();
            ConcurrentBag<List<string>> tbag = new ConcurrentBag<List<string>>();
            var ts = Enumerable.Range(0, theads)
                .Select(index => Task.Factory.StartNew(() =>
                {
                    tbag.Add(images.Skip(step * index).Take(step).ToList());
                })).ToArray();
            Task.WaitAll(ts);

            List<Task> tasks = new List<Task>();
            int c = 0;
            while (tbag.Count > 0)
            {
                List<string> s;
                if (tbag.TryTake(out s))
                {
                    if (s != null)
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            s.ForEach((image) =>
                            {
                                c++;
                                if (Monitor.TryEnter(tw))
                                {
                                    if (Monitor.IsEntered(tw))
                                    {
                                        Console.SetCursorPosition(0, currentT);
                                        Console.ResetColor();
                                        tw.Write("处理进度：{0}/{1}", c, images.Count);
                                    }
                                    if (Monitor.IsEntered(tw))
                                        Monitor.Exit(tw);
                                }
                                if (!TestCompletedImage(image))
                                {
                                    lock (twe)
                                    {
                                        Console.SetCursorPosition(0, currentT);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        twe.WriteLine(image);
                                        currentT = Console.CursorTop;
                                    }
                                    errorImage.Add(image);
                                }
                            });
                        }));
                }
            }
            Task.WaitAll(tasks.ToArray());
            Console.SetCursorPosition(0, currentT);
            Console.ResetColor();
            Console.Write("处理进度：{0}/{1}", c, images.Count);
            sw.Stop();
            Console.WriteLine("\r\n ConcurrentBag Task.Factory.StartNew 总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
            Console.WriteLine("===============================================");
            currentT = Console.CursorTop;
            #endregion

            int i = 0;
            #region 方法1
            //            i = 0;
            //            sw.Restart();
            //            List<Task> tlst = new List<Task>();
            //            for (int x = 0; x <= theads; x++)
            //            {
            //                var items = images.Skip(step * x).Take(step).ToList();
            //                tlst.Add(Task.Factory.StartNew((o) =>
            //                {
            //                    List<string> lst = (List<string>)o;
            //                    //lst.ForEach((image) =>
            //                    Parallel.ForEach(lst, (image) =>
            //                    {
            //                        i++;
            //                        if (Monitor.TryEnter(tw))
            //                        {
            //                            if (Monitor.IsEntered(tw))
            //                            {
            //                                Console.SetCursorPosition(0, currentT);
            //                                Console.ResetColor();
            //                                tw.Write("处理进度：{0}/{1}", i, images.Count);
            //                            }
            //                            if (Monitor.IsEntered(tw))
            //                                Monitor.Exit(tw);
            //                        }
            //                        if (!TestCompletedImage(image))
            //                        {
            //                            lock (twe)
            //                            {
            //                                Console.SetCursorPosition(0, currentT);
            //                                Console.ForegroundColor = ConsoleColor.Red;
            //                                twe.WriteLine(image);
            //                                currentT = Console.CursorTop;
            //                            }
            //                            errorImage.Add(image);
            //                        }
            //                    });
            //                }, items));
            //            }
            //            Task.WaitAll(tlst.ToArray());
            //
            //            Console.SetCursorPosition(0, currentT);
            //            Console.ResetColor();
            //            Console.Write("处理进度：{0}/{1}", i, images.Count);
            //            sw.Stop();
            //            Console.WriteLine("\r\nTask.Factory.StartNew 总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
            //            Console.WriteLine("===============================================");
            //            currentT = Console.CursorTop;
            #endregion

            #region 方法2
            //            i = 0;
            //            sw.Restart();
            //            Parallel.For(0, theads + 1, (x) =>
            //            {
            //                var items = images.Skip(step * x).Take(step).ToList();
            //                items.AsParallel().ForAll((image) =>
            //                {
            //                    i++;
            //                    if (Monitor.TryEnter(tw))
            //                    {
            //                        if (Monitor.IsEntered(tw))
            //                        {
            //                            Console.SetCursorPosition(0, currentT);
            //                            Console.ResetColor();
            //                            tw.Write("处理进度：{0}/{1}", i, images.Count);
            //                        }
            //                        if (Monitor.IsEntered(tw))
            //                            Monitor.Exit(tw);
            //                    }
            //                    if (!TestCompletedImage(image))
            //                    {
            //                        lock (twe)
            //                        {
            //                            Console.SetCursorPosition(0, currentT);
            //                            Console.ForegroundColor = ConsoleColor.Red;
            //                            twe.WriteLine(image);
            //                            currentT = Console.CursorTop;
            //                        }
            //                        errorImage.Add(image);
            //                    }
            //                });
            //            });
            //            Console.SetCursorPosition(0, currentT);
            //            Console.ResetColor();
            //            Console.Write("处理进度：{0}/{1}", i, images.Count);
            //            sw.Stop();
            //
            //            currentT = Console.CursorTop;
            //            Console.ResetColor();
            //            Console.WriteLine("\r\nParallel.ForALL 1 总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
            //            currentT = Console.CursorTop;
            #endregion

            #region 方法3
            //            i = 0;
            //            sw.Restart();
            //            Action<List<string>> ccc = (lst) =>
            //                {
            //                    lst.AsParallel().ForAll((image) =>
            //                    {
            //                        i++;
            //                        if (Monitor.TryEnter(tw))
            //                        {
            //                            if (Monitor.IsEntered(tw))
            //                            {
            //                                Console.SetCursorPosition(0, currentT);
            //                                Console.ResetColor();
            //                                tw.Write("处理进度：{0}/{1}", i, images.Count);
            //                            }
            //                            if (Monitor.IsEntered(tw))
            //                                Monitor.Exit(tw);
            //                        }
            //                        if (!TestCompletedImage(image))
            //                        {
            //                            lock (twe)
            //                            {
            //                                Console.SetCursorPosition(0, currentT);
            //                                Console.ForegroundColor = ConsoleColor.Red;
            //                                twe.WriteLine(image);
            //                                currentT = Console.CursorTop;
            //                            }
            //                            errorImage.Add(image);
            //                        }
            //                    });
            //                };
            //            Parallel.For(0, theads + 1, (x) =>
            //            {
            //                var items = images.Skip(step * x).Take(step).ToList();
            //                ccc(items);
            //            });
            //            Console.SetCursorPosition(0, currentT);
            //            Console.ResetColor();
            //            Console.Write("处理进度：{0}/{1}", i, images.Count);
            //            sw.Stop();
            //
            //            currentT = Console.CursorTop;
            //            Console.ResetColor();
            //            Console.WriteLine("\r\nParallel.ForALL 2 总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
            //            currentT = Console.CursorTop;
            #endregion

            #region aaa

            /*
            i = 0;
            sw.Restart();
            images.AsParallel().ForAll((image) =>
            {
                i++;
                if (Monitor.TryEnter(tw))
                {
                    if (Monitor.IsEntered(tw))
                    {
                        Console.SetCursorPosition(0, currentT);
                        Console.ResetColor();
                        tw.Write("处理进度：{0}/{1}", i, images.Count);
                    }
                    if (Monitor.IsEntered(tw))
                        Monitor.Exit(tw);
                }
                if (!TestCompletedImage(image))
                {
                    
//                    Monitor.TryEnter(twe,1000);
//                    if (Monitor.IsEntered(twe))
//                    {
//                        Console.SetCursorPosition(0, currentT);
//                        Console.ForegroundColor = ConsoleColor.Red;
//                        tw.WriteLine(image);
//                        currentT = Console.CursorTop;
//                    }
//                    if (Monitor.IsEntered(twe))
//                        Monitor.Exit(twe);
                    
                    lock (twe)
                    {
                        Console.SetCursorPosition(0, currentT);
                        Console.ForegroundColor = ConsoleColor.Red;
                        tw.WriteLine(image);
                        currentT = Console.CursorTop;
                    }
                    errorImage.Add(image);
                }
            });
            Console.SetCursorPosition(0, currentT);
            Console.ResetColor();
            Console.Write("处理进度：{0}/{1}", i, images.Count);
            sw.Stop();            
            currentT = Console.CursorTop;
            Console.ResetColor();
            Console.WriteLine("\r\nimages.AsParallel().ForAll 总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
            currentT = Console.CursorTop;
            */

            //Console.ReadKey(true);
            /*
            images.AsParallel().ForAll((image) =>
            {
                i++;
                lock (image)
                {
                    Console.SetCursorPosition(0, currentT);
                    Console.ResetColor();
                    Console.Write("处理进度：{0}/{1}", i, images.Count);
                    if (!IsCompletedImage(image))
                    {
                        Console.SetCursorPosition(0, currentT);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(image);
                        currentT++;
                        Console.SetCursorPosition(0, currentT);
                        errorImage.Add(image);
                    }
                }
            });            
            */
            /*
            Parallel.For(0,images.Count, (x) =>
            {
                i++;
                Console.SetCursorPosition(0, currentT);
                Console.ResetColor();
                Console.Write("处理进度：{0}/{1}", i, images.Count);
                if (!IsCompletedImage(images[x]))
                {
                    Console.SetCursorPosition(0, currentT);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(images[x]);
                    currentT++;
                    Console.SetCursorPosition(0, currentT);
                    errorImage.Add(images[x]);
                }
            });
             */
            #endregion

            #region 常用
            //            i = 0;
            //            sw.Restart();
            //            images.ForEach((image) =>
            //            {
            //                i++;
            //                Console.SetCursorPosition(0, currentT);
            //                Console.ResetColor();
            //                Console.Write("处理进度：{0}/{1}", i, images.Count);
            //                if (!TestCompletedImage(image))
            //                {
            //                    Console.SetCursorPosition(0, currentT);
            //                    Console.ForegroundColor = ConsoleColor.Red;
            //                    Console.Write(image);
            //                    currentT++;
            //                    Console.SetCursorPosition(0, currentT);
            //                    errorImage.Add(image);
            //                }
            //            });
            //            Console.SetCursorPosition(0, currentT);
            //            Console.ResetColor();
            //            Console.Write("处理进度：{0}/{1}", i, images.Count);
            //            sw.Stop();
            //            currentT = Console.CursorTop;
            //            Console.ResetColor();
            //            Console.WriteLine("\r\nParallel.ForEach 总共花费{0}ms.", sw.Elapsed.TotalMilliseconds);
            //            currentT = Console.CursorTop;
            #endregion

            Console.ResetColor();
            Console.WriteLine();

            File.WriteAllLines(path, fklist.ToArray());
            if (errorImage.Count > 0)
            {
                Console.WriteLine("是否删除错误的图片？ Y：删除 任意键继续");
                var inputkey = Console.ReadKey(true);
                if (inputkey.Key == ConsoleKey.Y)
                {
                    errorImage.Distinct().AsParallel().ForAll(File.Delete);
                }
            }
        }

        static void ExistsDir(List<DirectoryInfo> dirs)
        {
            dirs.RemoveAll(info => !Directory.Exists(info.FullName) || ext.Contains(info.Name));
        }
        static void CkComicDir()
        {
            var regex = new Regex(@"^\[(\d*)\]");
            var regexCn = new Regex(@"^[^\x00-\xff]");
            DirectoryInfo dirInfo = new DirectoryInfo(comicDir);
            Console.WriteLine("是否检查错误的图片？ Y：检查 任意键继续");
            var inputkey = Console.ReadKey(true);
            if (inputkey.Key == ConsoleKey.Y)
            {
                List<string> images = dirInfo.GetCustomFilesPaths("*.jpg|*.gif|*.png", SearchOption.AllDirectories);
                CkImages(images);
            }
            Console.WriteLine("===============================================================");
            List<DirectoryInfo> childInfos = dirInfo.GetDirectories().Where(r => !r.Name.Contains("长图漫画")).Select((info, i) =>
            {
                if (regexCn.Match(info.Name).Success)
                    return info;
                return null;
            }).Where(r => r != null).ToList();

            List<DirectoryInfo> muchList = new List<DirectoryInfo>();

            foreach (DirectoryInfo childInfo in childInfos)
            {
                var t = childInfo.GetDirectories().OrderByDescending(d => d.Name).FirstOrDefault();
                //Console.WriteLine(t.Name);
                if (t != null && t.LastWriteTimeUtc.Date <= DateTime.Now.Date.AddDays(-10))
                {
                    Console.WriteLine("{0}:{1}", childInfo.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"), childInfo.Name);
                    muchList.Add(childInfo);
                }
            }
            if (muchList.Count > 0)
            {
                Console.WriteLine("是否删除10天前的目录？ Y：删除 任意键退出");
                inputkey = Console.ReadKey(true);
                if (inputkey.Key == ConsoleKey.Y)
                {
                    foreach (DirectoryInfo childInfo in muchList)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("清理长时间目录：{0}", childInfo.Name);
                        childInfo.GetDirectories().AsParallel().ForAll(DeleteSelf);
                        childInfo.Delete(true);
                        Console.ResetColor();
                    }
                }
            }
            muchList.Clear();
            Console.WriteLine("===============================================================");
            ExistsDir(childInfos);
            foreach (DirectoryInfo childInfo in childInfos)
            {
                var chapterList = childInfo.GetDirectories().ToList();
                chapterList = chapterList.OrderByDescending(d => d.LastWriteTime).ToList();
                if (chapterList.Count > 3 || childInfo.Name.Contains(" - 副本"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0}:{1}", childInfo.Name, chapterList.Count);
                    muchList.Add(childInfo);
                }
                else
                {
                    Console.WriteLine("{0}:{1}", childInfo.Name, chapterList.Count);
                    childInfo.GetDirectories().ToList().AsParallel().ForAll((dir) => GetBigImage(dir));
                }
                Console.ResetColor();
            }
            Console.WriteLine("是否删除多余的目录？ Y：删除 任意键退出");
            inputkey = Console.ReadKey(true);
            if (inputkey.Key == ConsoleKey.Y)
            {
                Console.WriteLine("开始删除！......");

                foreach (DirectoryInfo info in muchList)
                {
                    Dictionary<int, DirectoryInfo> dlist = new Dictionary<int, DirectoryInfo>();
                    if (info.Name.Contains(" - 副本"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        DeleteSelf(info);
                        continue;
                    }
                    info.GetDirectories().OrderByDescending(d => d.LastWriteTime).AsParallel().ForAll(
                        dir =>
                        {
                            Console.ResetColor();
                            var match = regex.Match(dir.Name);
                            if (match.Success)
                            {
                                lock (dlist)
                                {
                                    int id;
                                    if (int.TryParse(match.Groups[1].Value, out id) && !dir.Name.Contains(" - 副本"))
                                    {
                                        if (!dlist.ContainsKey(id))
                                        {
                                            dlist.Add(id, dir);
                                            return;
                                        }
                                    }
                                }
                            }
                            Console.ForegroundColor = ConsoleColor.Red;
                            if (dir != null)
                            {
                                lock (dir)
                                {
                                    DeleteSelf(dir);
                                }
                            }
                        });
                    Console.ResetColor();
                    dlist.OrderByDescending(k => k.Key).Skip(3).ToList().AsParallel().ForAll(dir =>
                    {
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            if (dir.Value != null)
                                lock (dir.Value)
                                {
                                    DeleteSelf(dir.Value);
                                }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    });
                }
                Console.WriteLine("清理完毕！任意键退出");
                Console.ReadKey(true);
            }
        }

        private static void DeleteSelf(DirectoryInfo dir)
        {
            var baseDir = dir.Parent;
            DateTime baseDateTime = baseDir.LastWriteTime;
            Console.WriteLine(@"{0}/{1}", dir.Parent.ToGetValue(dir).Name, dir.Name);
            dir.Delete(true);
            baseDir.LastWriteTime = baseDateTime;
        }
        private static bool TestCompletedImage(string strFileName)
        {
            string ckdata = "";
            /*
            if (fileck.TryGetValue(strFileName, out ckdata))
                if (fklist.Contains(ckdata))
                {
                    return true;
                }*/
            ImageData i = idata.FirstOrDefault(r => r.Path == strFileName);

            switch (ctype)
            {
                case CType.CRC32:
                    ckdata = i.CRC32; break;
                case CType.MD5:
                    ckdata = i.MD5; break;
            }
            if (fklist.Contains(ckdata)) return true;

            if (IsCompletedImage(strFileName))
            {
                fklist.Add(ckdata);
                return true;
            }
            return false;
        }
        private static bool IsCompletedImage(string strFileName)
        {
            try
            {
                FileStream fs = new FileStream(strFileName, FileMode.Open);
                BinaryReader reader = new BinaryReader(fs);
                try
                {
                    byte[] szBuffer = reader.ReadBytes((int)fs.Length);
                    //jpg png图是根据最前面和最后面特殊字节确定. bmp根据文件长度确定  
                    //png检查  
                    if (szBuffer[0] == 137 && szBuffer[1] == 80 && szBuffer[2] == 78 && szBuffer[3] == 71 && szBuffer[4] == 13
                        && szBuffer[5] == 10 && szBuffer[6] == 26 && szBuffer[7] == 10)
                    {
                        //&& szBuffer[szBuffer.Length - 8] == 73 && szBuffer[szBuffer.Length - 7] == 69 && szBuffer[szBuffer.Length - 6] == 78  
                        if (szBuffer[szBuffer.Length - 5] == 68 && szBuffer[szBuffer.Length - 4] == 174 && szBuffer[szBuffer.Length - 3] == 66
                            && szBuffer[szBuffer.Length - 2] == 96 && szBuffer[szBuffer.Length - 1] == 130)
                            return true;
                        //有些情况最后多了些没用的字节  
                        for (int i = szBuffer.Length - 1; i > szBuffer.Length / 2; --i)
                        {
                            if (szBuffer[i - 5] == 68 && szBuffer[i - 4] == 174 && szBuffer[i - 3] == 66
                             && szBuffer[i - 2] == 96 && szBuffer[i - 1] == 130)
                                return true;
                        }


                    }
                    else if (szBuffer[0] == 66 && szBuffer[1] == 77)//bmp  
                    {
                        //bmp长度  
                        //整数转成字符串拼接  
                        string str = Convert.ToString(szBuffer[5], 16) + Convert.ToString(szBuffer[4], 16)
                            + Convert.ToString(szBuffer[3], 16) + Convert.ToString(szBuffer[2], 16);
                        int iLength = Convert.ToInt32("0x" + str, 16); //16进制数转成整数  
                        if (iLength <= szBuffer.Length) //有些图比实际要长  
                            return true;
                    }
                    else if (szBuffer[0] == 71 && szBuffer[1] == 73 && szBuffer[2] == 70 && szBuffer[3] == 56)//gif  
                    {
                        //标准gif 检查00 3B  
                        if (szBuffer[szBuffer.Length - 2] == 0 && szBuffer[szBuffer.Length - 1] == 59)
                            return true;
                        //检查含00 3B  
                        for (int i = szBuffer.Length - 1; i > szBuffer.Length / 2; --i)
                        {
                            if (szBuffer[i] != 0)
                            {
                                if (szBuffer[i] == 59 && szBuffer[i - 1] == 0)
                                    return true;
                            }
                        }
                    }
                    else if (szBuffer[0] == 255 && szBuffer[1] == 216) //jpg  
                    {
                        //标准jpeg最后出现ff d9  
                        if (szBuffer[szBuffer.Length - 2] == 255 && szBuffer[szBuffer.Length - 1] == 217)
                            return true;
                        else
                        {
                            //有好多jpg最后被人为补了些字符也能打得开, 算作完整jpg, ffd9出现在近末端  
                            //jpeg开始几个是特殊字节, 所以最后大于10就行了 从最后字符遍历  
                            //有些文件会出现两个ffd9 后半部分ffd9才行  
                            /*
                            for (int i = szBuffer.Length - 2; i > szBuffer.Length / 2; --i)
                            {
                                //检查有没有ffd9连在一起的  
                                if (szBuffer[i] == 255 && szBuffer[i + 1] == 217)
                                    return true;
                            }
                            */
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                    if (reader != null)
                        reader.Close();
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        public static string GetMD5HashFromFile(string filePath)
        {
            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString().ToUpper();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}
