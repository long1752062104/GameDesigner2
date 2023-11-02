using System;

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
        public static string GetRelativePath(string root, string fullPath) 
        {
            Uri urlRoot;
            try 
            {
                urlRoot = new Uri(root);
            }
            catch 
            {
                try
                {
                    urlRoot = new Uri(root.Replace('/', '\\'));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            Uri fullPathRoot;
            try
            {
                fullPathRoot = new Uri(fullPath);
            }
            catch
            {
                try
                {
                    fullPathRoot = new Uri(fullPath.Replace('/', '\\'));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            var relativeUri = urlRoot.MakeRelativeUri(fullPathRoot);
            return relativeUri.ToString();
        }
    }
}