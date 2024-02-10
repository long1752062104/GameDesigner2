using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using Cysharp.Threading.Tasks;
using Net.Event;
using System.Linq;

namespace Net.Helper
{
    /// <summary>
    /// 压缩数据传输
    /// </summary>
    public static class UnZipHelper
    {
        #region 返回压缩后的字节数组
        /// <summary>
        /// 返回压缩后的字节数组
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(4096))
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(buffer, 0, buffer.Length);
                }
                buffer = ms.ToArray();
                return buffer;
            }
        }
        #endregion

        #region 返回解压后的字节数组
        /// <summary>
        /// 返回解压后的字节数组
        /// </summary>
        /// <param name="data">原始字节数组</param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data)
        {
            return Decompress(data, 0, data.Length);
        }

        /// <summary>
        /// 返回解压后的字节数组
        /// </summary>
        /// <param name="data">原始字节数组</param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] data, int index, int count)
        {
            using (MemoryStream stream = new MemoryStream(data, index, count))
            {
                using (MemoryStream stream1 = new MemoryStream(4096))
                {
                    using (GZipStream decompress = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        decompress.CopyTo(stream1);
                        byte[] result = stream1.ToArray();
                        return result;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="sourceDirectoryName">要压缩的文件夹路径</param>
        /// <param name="destinationArchiveFileName">压缩文件路径</param>
        /// <param name="compressionLevel">压缩层</param>
        /// <param name="includeBaseDirectory">压缩包含当前目录</param>
        /// <param name="entryNameEncoding">压缩编码</param>
        public static void CompressFiles(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel = CompressionLevel.Fastest, bool includeBaseDirectory = false, Encoding entryNameEncoding = null)
        {
            _ = CompressFiles(sourceDirectoryName, destinationArchiveFileName, compressionLevel, includeBaseDirectory, entryNameEncoding, null, false);
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="sourceDirectoryName">要压缩的文件夹路径</param>
        /// <param name="destinationArchiveFileName">压缩文件路径</param>
        /// <param name="compressionLevel">压缩层</param>
        /// <param name="includeBaseDirectory">压缩包含当前目录</param>
        /// <param name="entryNameEncoding">压缩编码</param>
        public static async UniTask CompressFiles(string sourceDirectoryName, string destinationArchiveFileName, CompressionLevel compressionLevel, bool includeBaseDirectory, Encoding entryNameEncoding, Action<string, float> progress = null, bool isAsync = true)
        {
            sourceDirectoryName = Path.GetFullPath(sourceDirectoryName);
            destinationArchiveFileName = Path.GetFullPath(destinationArchiveFileName);
            FileStream fileStream;
            if (File.Exists(destinationArchiveFileName))
                fileStream = File.Open(destinationArchiveFileName, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite);
            else
                fileStream = File.Open(destinationArchiveFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: false, entryNameEncoding))
            {
                bool flag = true;
                var directoryInfo = new DirectoryInfo(sourceDirectoryName);
                string fullName = directoryInfo.FullName;
                if (includeBaseDirectory && directoryInfo.Parent != null)
                    fullName = directoryInfo.Parent.FullName;
                var fileSystemInfos = directoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToArray();
                var count = fileSystemInfos.Length;
                for (int i = 0; i < count; i++)
                {
                    var item = fileSystemInfos[i];
                    int length = item.FullName.Length - fullName.Length;
                    string text;
                    text = item.FullName.Substring(fullName.Length, length);
                    text = text.Replace('\\', '/');
                    if (item is FileInfo)
                    {
                        using Stream stream = File.Open(item.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var entry = zipArchive.CreateEntry(text, compressionLevel);
                        var dateTime = File.GetLastWriteTime(item.FullName);
                        if (dateTime.Year < 1980 || dateTime.Year > 2107)
                            dateTime = new DateTime(1980, 1, 1, 0, 0, 0);
                        entry.LastWriteTime = dateTime;
                        using (Stream destination2 = entry.Open())
                        {
                            stream.CopyTo(destination2);
                        }
                        progress?.Invoke(entry.Name, i / (float)count);
                        if (isAsync) await UniTask.Yield();
                        continue;
                    }
                    if (item is DirectoryInfo directoryInfo2)
                    {
                        using (var enumerator = directoryInfo2.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).GetEnumerator())
                        {
                            if (enumerator.MoveNext())
                                zipArchive.CreateEntry(text + '/');
                        }
                    }
                }
                if (includeBaseDirectory && flag)
                    zipArchive.CreateEntry(directoryInfo.Name + '/');
            }
        }

        /// <summary>
        /// 解压文件夹
        /// </summary>
        /// <param name="sourceArchiveFileName">压缩文件路径</param>
        /// <param name="destinationDirectoryName">要解压到的文件夹路径</param>
        /// <param name="entryNameEncoding">解压编码</param>
        /// <exception cref="IOException"></exception>
        public static void DecompressFile(string sourceArchiveFileName, string destinationDirectoryName, Encoding entryNameEncoding = null)
        {
            _ = DecompressFile(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding, null, false);
        }

        /// <summary>
        /// 解压文件夹
        /// </summary>
        /// <param name="sourceArchiveFileName">压缩文件路径</param>
        /// <param name="destinationDirectoryName">要解压到的文件夹路径</param>
        /// <param name="entryNameEncoding">解压编码</param>
        /// <param name="progress">解压进度</param>
        /// <param name="isAsync">是否异步调用</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public static async UniTask<bool> DecompressFile(string sourceArchiveFileName, string destinationDirectoryName, Encoding entryNameEncoding, Action<string, float> progress = null, bool isAsync = true)
        {
            try
            {
                var fileStream = File.Open(sourceArchiveFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (ZipArchive source = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: false, entryNameEncoding))
                {
                    var directoryInfo = Directory.CreateDirectory(destinationDirectoryName);
                    string text = directoryInfo.FullName;
                    var count = source.Entries.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var entry = source.Entries[i];
                        progress?.Invoke(entry.Name, i / (float)count);
                        if (isAsync) await UniTask.Yield();
                        string fullPath = Path.GetFullPath(text + entry.FullName);
                        if (Path.GetFileName(fullPath).Length == 0)
                        {
                            if (entry.Length != 0L)
                                throw new IOException("把数据当作文件夹识别了!");
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                            using (Stream destination = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                            {
                                using Stream stream = entry.Open();
                                stream.CopyTo(destination);
                            }
                            File.SetLastWriteTime(fullPath, entry.LastWriteTime.DateTime);
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                NDebug.LogError(ex);
            }
            return false;
        }
    }
}