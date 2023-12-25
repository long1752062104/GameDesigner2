using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Net.Helper
{
    public class PathHelper
    {
        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="root"></param>
        /// <param name="fullPath"></param>
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
    }
}