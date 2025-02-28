using System.IO;

namespace Lustie.UnityDocfx
{
    public class DocfxTemplatesPath
    {
        /// <summary>
        /// Get current path
        /// </summary>
        /// <returns></returns>
        public static string CurrentPath()
        {
            string scriptPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            return Path.GetDirectoryName(scriptPath);
        }

        public static bool LoadTemplates(string fileName, out string filePath)
        {
            filePath = string.Join("/", CurrentPath(), fileName);

            return File.Exists(filePath);
        }
    }
}
