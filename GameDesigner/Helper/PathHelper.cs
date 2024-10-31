using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Net.Helper
{
    /// <summary>
    /// 文件路径助手
    /// </summary>
    public class PathHelper
    {
        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="root"></param>
        /// <param name="fullPath"></param>
        /// <param name="isRevise"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetRelativePath(string root, string fullPath, bool isRevise = false)
        {
            var rootPathUri = new Uri(PlatformReplace(root));
            var fullPathUri = new Uri(PlatformReplace(fullPath));
            var relativeUri = rootPathUri.MakeRelativeUri(fullPathUri);
            var relativePath = relativeUri.ToString();
            if (isRevise)
                return PlatformReplace(relativePath);
            return relativePath;
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="root"></param>
        /// <param name="fullPath"></param>
        /// <param name="oldSeparator"></param>
        /// <param name="replaceSeparator"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetRelativePath(string root, string fullPath, char oldSeparator, char replaceSeparator)
        {
            var rootPathUri = new Uri(PlatformReplace(root));
            var fullPathUri = new Uri(PlatformReplace(fullPath));
            var relativeUri = rootPathUri.MakeRelativeUri(fullPathUri);
            var relativePath = relativeUri.ToString();
            return PlatformReplace(relativePath, oldSeparator, replaceSeparator);
        }

        public static string Combine(params string[] paths)
        {
            var fullPath = string.Empty;
            foreach (var path in paths)
                fullPath += path + Path.DirectorySeparatorChar;
            fullPath = fullPath.TrimEnd(Path.DirectorySeparatorChar); //解决如果是文件路径, 会导致错误问题
            return PlatformReplace(fullPath);
        }

        public static string PlatformReplace(string path)
        {
            var separator = Path.DirectorySeparatorChar;
            char temp;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                temp = '/';
            else
                temp = '\\';
            path = path.Replace(temp, separator);
            var separators = $"{separator}{separator}";
            while (path.Contains(separators))
                path = path.Replace(separators, separator.ToString());
            return path;
        }

        public static string PlatformReplace(string path, char separator)
        {
            return PlatformReplace(path, Path.DirectorySeparatorChar, separator);
        }

        public static string PlatformReplace(string path, char oldSeparator, char replaceSeparator)
        {
            path = path.Replace(oldSeparator, replaceSeparator);
            var separators = $"{replaceSeparator}{replaceSeparator}";
            while (path.Contains(separators))
                path = path.Replace(separators, replaceSeparator.ToString());
            return path;
        }

        /// <summary>
        /// 查找父文件夹里面的某个格式文件
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string FindParentFile(string searchPattern)
        {
            var files = FindParentFiles(searchPattern);
            return files.Length > 0 ? files[0] : string.Empty;
        }

        /// <summary>
        /// 查找父文件夹里面的某个格式文件
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string[] FindParentFiles(string searchPattern)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                var files = Directory.GetFiles(currentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                    return files;
                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }
            return new string[0];
        }
    }
}