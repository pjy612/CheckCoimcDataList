using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageMagick;

namespace MergeComic
{
    public static class ObjectExtension
    {
        public static bool IsNotNull(this object value)
        {
            return value != null;
        }
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
    public enum ImageMergeOrientation
    {
        Horizontal,
        Vertical
    }

    public static class Program
    {
        private const string searchPatten = "*.jpg|*.png|*.gif";

        public static void SearchImage(string basePath)
        {
            if (Directory.Exists(basePath))
            {
                var info = new DirectoryInfo(basePath);
                var images = info.GetCustomFilesInfos(searchPatten).OrderBy(r => r.Name).ToArray();
                if (images.Any())
                {
                    string dirName = string.Format("{0}/{1}", savefolderPath, string.IsNullOrWhiteSpace(ism) ? info.Parent.Name : "mobile");
                    Directory.CreateDirectory(dirName);
                    string infoName = info.Name;
                    if (!infoName.Contains(info.Parent.Name))
                        infoName = info.Name.Insert(info.Name.IndexOf("]") + 1, info.Parent.Name + "_");
                    string fname = String.Format("{0}/{1}{2}.jpg", dirName, infoName, ism);
                    //CombineImages(images, fname, deWidth: dwidth);
                    CombineImagesMagick(images, fname, deWidth: dwidth);
                    //CombineImagesByEmguCV(images, fname, deWidth: dwidth);
                    RestDirLastWriteTime(dirName);
                    RestDirLastWriteTime(savefolderPath);
                    Console.WriteLine("{0}生成完毕！", infoName);
                    doFile = true;
                    return;
                }
                info.GetDirectories().ToList().ForEach(child =>
                    SearchImage(child.FullName)
                    );
            }
        }

        private static void RestDirLastWriteTime(string dir)
        {
            try
            {
                new DirectoryInfo(dir).LastWriteTime = DateTime.Now;
            }
            catch (Exception)
            {
                // ignored
            }
        }
        const string folderPath = @"D:\yyq\";
        const string savefolderPath = @"D:\yyq\长图漫画";
        static string ism = "";
        static bool doFile = false;
        private static int? dwidth = null;

        public static void ReDirNames(string basePath)
        {
            var regex = new Regex(@"^\[(\d*)\]");
            var cDirList = new DirectoryInfo(basePath).GetDirectories().OrderBy(c => c.Name).ToList();
            int index = 0;
            for (int i = 0; i < cDirList.Count(); i++)
            {
                index++;
                var cinfo = cDirList[i];
                var match = regex.Match(cinfo.Name);
                if (match.Length > 0)
                {
                    int x;
                    if (int.TryParse(match.Groups[1].Value, out x))
                    {
                        if (index == x) continue;
                        string newName = regex.Replace(cinfo.Name, string.Format("[{0}]", index.ToString().PadLeft(4, '0')));
                        var flist = cinfo.GetFiles().OrderBy(f => f.Name);
                        foreach (FileInfo f in flist)
                        {
                            var nfname = f.Name.Replace(x + "_", index + "_");
                            if (nfname == f.Name) continue;
                            doFile = true;
                            f.MoveTo(cinfo.FullName + "/" + nfname);
                            Console.WriteLine(cinfo.FullName + "/" + nfname);
                        }
                        Console.WriteLine(cinfo.Parent.FullName + "/" + newName);
                        cinfo.MoveTo(cinfo.Parent.FullName + "/" + newName);
                    }
                }
            }
        }
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                List<string> param = args.ToList();
                string basePath = param[0];
                if (basePath == "1")
                {
                    basePath = Directory.GetCurrentDirectory();
                    param.Insert(1, "1");
                }
                if (param.Count > 1 && param[1] == "2")
                {
                    ReDirNames(basePath);
                    return;
                }
                if (param.Count > 1 && param[1] == "1")
                {
                    ism = "_mobile";
                    dwidth = 640;
                }
                SearchImage(basePath);
            }
            else
            {
                var mergeList = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/Merge.txt").Where(r => !string.IsNullOrWhiteSpace(r)).Distinct();
                var parent = new DirectoryInfo(folderPath).GetDirectories().Where(r => mergeList.Contains(r.Name)).ToList();
                foreach (DirectoryInfo p in parent)
                {
                    var childs = p.GetDirectories().OrderBy(r => r.Name).ToList();
                    string dirName = string.Format("{0}/{1}", savefolderPath, p.Name);
                    Directory.CreateDirectory(dirName);
                    //childs.AsParallel().ForAll(info =>
                    //{
                    //    string fname = String.Format("{0}/{1}.tiff", dirName, info.Name);
                    //    if (!File.Exists(fname))
                    //    {
                    //        var images = info.GetFiles("*.*", SearchOption.TopDirectoryOnly).OrderBy(r => r.Name).ToArray();
                    //        CombineImages(images, fname);
                    //        Console.WriteLine("{0}生成完毕！", info.Name);
                    //    }
                    //});
                    foreach (DirectoryInfo info in childs)
                    {
                        string infoName = info.Name;
                        if (!infoName.Contains(p.Name))
                            infoName = info.Name.Insert(info.Name.IndexOf("]") + 1, p.Name + "_");
                        string fname = String.Format("{0}/{1}.jpg", dirName, infoName);
                        if (!File.Exists(fname))
                        {
                            var images = info.GetCustomFilesInfos(searchPatten).OrderBy(r => r.Name).ToArray();
                            //CombineImages(images, fname);
                            CombineImagesMagick(images, fname, deWidth: dwidth);
                            RestDirLastWriteTime(dirName);
                            RestDirLastWriteTime(savefolderPath);
                            Console.WriteLine("{0}生成完毕！", infoName);
                            doFile = true;
                        }
                    }
                    var dir = new DirectoryInfo(dirName);
                    DateTime lastTime = dir.LastWriteTime;
                    var outFiles = dir.GetCustomFilesInfos(searchPatten).Where(file => !childs.Any(info =>
                    {
                        string infoName = info.Name;
                        if (!infoName.Contains(p.Name))
                            infoName = info.Name.Insert(info.Name.IndexOf("]") + 1, p.Name + "_");
                        return file.Name.Remove(file.Name.LastIndexOf(".")) == infoName;
                    }));
                    if (outFiles.Any())
                    {
                        Console.WriteLine("清理多余相关文件：");
                        Console.ForegroundColor = ConsoleColor.Red;
                        outFiles.AsParallel().ForAll(i =>
                        {
                            try
                            {
                                i.Delete();
                            }
                            finally
                            {
                                Console.WriteLine(i.Name);
                            }
                        });
                    }
                    Console.ResetColor();
                    dir.LastWriteTime = lastTime;
                }
            }
            if (doFile)
            {
                Console.WriteLine("全部处理完毕！");
                Console.ReadKey(true);
            }
        }
        public static void CombineImagesMagick(FileInfo[] files, string toPath,
            ImageMergeOrientation mergeType = ImageMergeOrientation.Vertical, int? deWidth = null, int? deHeight = null)
        {
            var finalImage = toPath;
            List<MagickImage> imgs;
        reloadImg:
            try
            {
                imgs = files.Select(f => new MagickImage(f.FullName)).ToList();
            }
            catch (Exception)
            {
                goto reloadImg;
            }
            if (!imgs.Any()) return;
            var destWidth = deWidth ?? imgs.Max(img => img.Width);
            var destHeight = deHeight ?? imgs.Max(img => img.Height);

        baseMethod:
            MagickImageCollection images = new MagickImageCollection(imgs);
            IMagickImage finalImg = new MagickImage();
            try
            {
                foreach (IMagickImage img in images)
                {
                    MagickGeometry mg = (mergeType == ImageMergeOrientation.Horizontal ? new MagickGeometry() { Height = destHeight } : new MagickGeometry() { Width = destWidth });
                    img.Resize(mg);
                }
                finalImg = (mergeType == ImageMergeOrientation.Horizontal ? images.AppendHorizontally() : images.AppendVertically());
                finalImg.Strip();
                GC.Collect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{2}图片生成异常！{0}x{1},Error:{3}", finalImg.Width, finalImg.Height, finalImage, ex.Message);
                destWidth = (int)(destWidth * 0.9);
                destHeight = (int)(destHeight * 0.9);
                goto baseMethod;
            }
        reSave:
            int baseSize = 65500;
            try
            {
                finalImg.Quality = 80;
                finalImg.CompressionMethod = CompressionMethod.LZW;
                finalImg.Format = MagickFormat.Jpeg;
                if (mergeType == ImageMergeOrientation.Horizontal)
                {
                    if (finalImg.Width > 65500 && finalImg.Format == MagickFormat.Jpeg)
                    {
                        finalImg.Quality = 100;
                        finalImg.Resize(new MagickGeometry() { Width = baseSize });
                    }
                }
                else if (mergeType == ImageMergeOrientation.Vertical)
                {
                    if (finalImg.Height > 65500 && finalImg.Format == MagickFormat.Jpeg)
                    {
                        finalImg.Quality = 100;
                        finalImg.Resize(new MagickGeometry() { Height = baseSize });
                    }
                }
                //using (FileStream fs = new FileStream(finalImage, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    finalImg.Write(finalImage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{2}Save出错！{0}x{1},Error:{3}", finalImg.Width, finalImg.Height, finalImage, ex.Message);
                if (finalImg.Format != MagickFormat.Jpeg)
                    finalImg.Resize(new Percentage(90));
                goto reSave;
            }
        }
        public static void CombineImages(FileInfo[] files, string toPath, ImageMergeOrientation mergeType = ImageMergeOrientation.Vertical, int? deWidth = null, int? deHeight = null)
        {
            //change the location to store the final image.
            // URL：http://www.bianceng.cn/Programming/csharp/201410/45751.htm

            EncoderParameter p1;
            EncoderParameter p2;
            EncoderParameters ps;

            ps = new EncoderParameters(2);
            p1 = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
            p2 = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);
            ps.Param[0] = p1;
            ps.Param[1] = p2;

            var finalImage = toPath;
            List<Image> imgs;
        reloadImg:
            try
            {
                imgs = files.Select(f => Image.FromFile(f.FullName)).ToList();
            }
            catch (Exception)
            {
                goto reloadImg;
            }
            if (!imgs.Any()) return;
            int finalWidth = 0, finalHeight = 0;
            var destWidth = deWidth ?? imgs.Max(img => img.Width);
            var destHeight = deHeight ?? imgs.Max(img => img.Height);
            Bitmap finalImg;
            ImageFormat thisFormat = null;
        baseMethod:
            try
            {
                switch (mergeType)
                {
                    case ImageMergeOrientation.Horizontal:
                        finalHeight = destHeight;
                        finalWidth = imgs.Sum(img => (int)(img.Width / ((decimal)img.Height / destHeight)));
                        break;
                    case ImageMergeOrientation.Vertical:
                        finalHeight = imgs.Sum(img => (int)(img.Height / ((decimal)img.Width / destWidth)));
                        finalWidth = destWidth;
                        break;
                }
            }
            catch (Exception)
            {
                goto reloadImg;
            }
            //baseNewImg:
            try
            {
                //Console.WriteLine("{0}x{1}", finalWidth, finalHeight);
                finalImg = new Bitmap(finalWidth, finalHeight, PixelFormat.Format32bppArgb);
            }
            catch (ArgumentException)
            {
                destWidth = (int)(destWidth * 0.9);
                destHeight = (int)(destHeight * 0.9);
                goto baseMethod;
            }
            catch (Exception)
            {
                goto reloadImg;
            }

            try
            {
                Graphics g = Graphics.FromImage(finalImg);
                // 设置画布的描绘质量  
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.Clear(SystemColors.AppWorkspace);
                var width = finalWidth;
                var height = finalHeight;
                var nIndex = 0;
                foreach (FileInfo file in files)
                {
                    Image img = Image.FromFile(file.FullName);

                    int setp = 0;
                    switch (mergeType)
                    {
                        case ImageMergeOrientation.Horizontal:
                            setp = (int)(img.Width / ((decimal)img.Height / destHeight));
                            break;
                        case ImageMergeOrientation.Vertical:
                            setp = (int)(img.Height / ((decimal)img.Width / destWidth));
                            break;
                    }

                    if (nIndex == 0)
                    {
                        thisFormat = img.RawFormat;
                        switch (mergeType)
                        {
                            case ImageMergeOrientation.Horizontal:
                                g.DrawImage(img, new Rectangle(new Point(0, 0), new Size(setp, destHeight)));
                                width = setp;
                                height = destHeight;
                                break;
                            case ImageMergeOrientation.Vertical:
                                g.DrawImage(img, new Rectangle(new Point(0, 0), new Size(destWidth, setp)));
                                width = destWidth;
                                height = setp;
                                break;
                        }
                        //g.DrawImage(img, new Point(0, 0));
                        nIndex++;
                    }
                    else
                    {
                        switch (mergeType)
                        {
                            case ImageMergeOrientation.Horizontal:
                                //g.DrawImage(img, new Point(width, 0));
                                g.DrawImage(img, new Rectangle(new Point(width, 0), new Size(setp, destHeight)));
                                width += setp;
                                break;
                            case ImageMergeOrientation.Vertical:
                                //g.DrawImage(img, new Point(0, height));
                                g.DrawImage(img, new Rectangle(new Point(0, height), new Size(destWidth, setp)));
                                height += setp;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("mergeType");
                        }
                    }
                    img.Dispose();
                }
                g.Dispose();
                GC.Collect();
                //                //保存图象     
                //                ImageCodecInfo imgCodecInfo = GetCodecInfo(finalImage);
                //            
                //                using (FileStream fs = new FileStream(finalImage, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                //                {
                //                    if (imgCodecInfo == null)
                //                    {
                //                        finalImg.Save(fs, thisFormat);
                //                    }
                //                    else
                //                    {
                //                        finalImg.Save(fs, imgCodecInfo, ps);
                //                    }
                //                }
                //                finalImg.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{2}图片生成异常！{0}x{1},Error:{3}", finalWidth, finalHeight, finalImage, ex.Message);
                destWidth = (int)(destWidth * 0.9);
                destHeight = (int)(destHeight * 0.9);
                goto baseMethod;
            }
        reSave:
            try
            {
                using (Bitmap f = new Bitmap(finalWidth, finalHeight, PixelFormat.Format32bppArgb))
                {
                    Graphics g = Graphics.FromImage(f);
                    // 设置画布的描绘质量  
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(finalImg, new Rectangle(new Point(0, 0), new Size(finalWidth, finalHeight)));
                    g.Dispose();
                    GC.Collect();
                    //保存图象     
                    ImageCodecInfo imgCodecInfo = GetCodecInfo(finalImage);
                    using (FileStream fs = new FileStream(finalImage, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        if (imgCodecInfo == null)
                        {
                            f.Save(fs, thisFormat);
                        }
                        else
                        {
                            f.Save(fs, imgCodecInfo, ps);
                        }
                    }
                    finalImg.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{2}Save出错！{0}x{1},Error:{3}", finalWidth, finalHeight, finalImage, ex.Message);
                finalWidth = (int)(finalWidth * 0.9);
                finalHeight = (int)(finalHeight * 0.9);
                goto reSave;
            }
        }
        private static byte[] GetBytes(Image image)
        {
            try
            {
                if (image == null) return null;
                using (Bitmap bitmap = new Bitmap(image))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Jpeg);
                        return stream.GetBuffer();
                    }
                }
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
            }
        }
        //返回图片解码器信息用于jpg图片  
        private static ImageCodecInfo GetCodecInfo(string str)
        {
            string ext = str.Substring(str.LastIndexOf(".") + 1);
            string mimeType = "";
            switch (ext.ToLower())
            {
                case "jpe":
                case "jpg":
                case "jpeg":
                    mimeType = "image/jpeg";
                    break;
                case "bmp":
                    mimeType = "image/bmp";
                    break;
                case "png":
                    mimeType = "image/png";
                    break;
                case "tif":
                case "tiff":
                    mimeType = "image/tiff";
                    break;
                default:
                    mimeType = "image/jpeg";
                    break;
            }
            ImageCodecInfo[] CodecInfo = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in CodecInfo)
            {
                if (ici.MimeType == mimeType) return ici;
            }
            return null;
        }
    }
}
