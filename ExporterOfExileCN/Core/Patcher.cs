using System.IO;
using System.Text.RegularExpressions;

namespace ExporterOfExileCN.Core
{
    class Patcher
    {
        private static readonly string _hostNameOfTencent = "https://poe.game.qq.com/";
        private static readonly string _hostNameOfLocalPattern = @"http://localhost:\d{1,5}/";

        public static bool IsNeededPatch(string path, string hostName)
        {
            var content = File.ReadAllText(path);
            if (content.Contains(hostName))
            {
                return false;
            }
            return true;
        }

        public static void Patch(string path, string hostName)
        {
            string content = File.ReadAllText(path);
            if (content.Contains(_hostNameOfTencent))
            {
                File.WriteAllText(path, content.Replace(_hostNameOfTencent, hostName));
            }
            else
            {
                var rx = new Regex(_hostNameOfLocalPattern);
                if (rx.IsMatch(content))
                {
                    File.WriteAllText(path, rx.Replace(content, hostName));
                }
            }
        }
    }
}
