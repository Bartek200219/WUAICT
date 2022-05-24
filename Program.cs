using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json.Linq;
using Tommy;

namespace WUAICT
{
    class Program
    {
        static void Main(string[] args)
        {
            string defaultPrograms;
            string otherProgramToDisplay;
            string XMLsavePath;
            string ventoyConfigPath;

            //Check if config exist
            if (!File.Exists("config.toml"))
            {
                Console.WriteLine("Nie znaleziono pliku konfiguracyjnego!\nGeneruje nowy...");
                TomlTable toml = new TomlTable
                {
                    ["WUAICT"]=
                    {
                        ["defaultPrograms"] = "firefox google-chrome notepad++ vlc onlyoffice-desktopeditors anydesk 7zip",
                        ["otherProgramToDisplay"] = "microsoft-teams teamspeak discord obs-studio steam origin opera epic-games-launcher",
                    },
                    ["ventoy"] =
                    {
                        ["XMLsavePath"] = "../",
                        ["ConfigPath"] = "/../ventoy/ventoy.json"
                    }
                };
                using (StreamWriter writer = File.CreateText("config.toml"))
                {
                    toml.WriteTo(writer);
                    writer.Flush();
                }
                PrintAndWait("Pomyślnie wygenerowano plik konfiguracyjny, zedytuj go!");
                return;
            }

            //Config get
            using (StreamReader reader = File.OpenText("config.toml"))
            {
                TomlTable table = TOML.Parse(reader);
                defaultPrograms = table["WUAICT"]["defaultPrograms"];
                otherProgramToDisplay = table["WUAICT"]["otherProgramToDisplay"];
                ventoyConfigPath = table["ventoy"]["ConfigPath"];
                XMLsavePath = table["ventoy"]["XMLsavePath"];
            }

            //instalation config
            string[] arguments = Environment.GetCommandLineArgs();
            string filePath;
            string WindowsKey;
            string programsToInstall;
            if ((arguments.Length > 1) && arguments[1].EndsWith(".toml") && File.Exists(arguments[1]))
            {
                using (StreamReader reader = File.OpenText(arguments[1]))
                {
                    TomlTable table = TOML.Parse(reader);
                    filePath = table["filePath"];
                    WindowsKey = GetKey(table["options"]["keyGetMethod"]);
                    programsToInstall = table["options"]["programsToInstall"];
                }
            }
            else if ((arguments.Length > 1) && arguments[1].EndsWith(".xml") && File.Exists(arguments[1]))
            {
                filePath = arguments[1];
                WindowsKey = GetKey();
                programsToInstall = GetProgramsToInstall(defaultPrograms, otherProgramToDisplay);
            }
            else
            {
                filePath = getXMLfile();
                WindowsKey = GetKey();
                programsToInstall = GetProgramsToInstall(defaultPrograms, otherProgramToDisplay);
            }

            //Set output file path
            DateTime dt = DateTime.Now;
            string creationdate = dt.Year + "-" + dt.Month + "-" + dt.Day + "_" + dt.Hour + "-" + dt.Minute + "-" + dt.Second;
            string outputfilename = creationdate + "-" + WindowsKey.Substring(Math.Max(0, WindowsKey.Length - 5)) + ".xml";
            Console.WriteLine(outputfilename);
            string outputFilePath = Path.Combine(XMLsavePath, outputfilename);

            //Open and load xml
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(filePath);

            //Change productkey
            XmlNode n = doc.GetElementsByTagName("ProductKey")[1];
            n.InnerText = WindowsKey;

            //Change programs to install
            var index = 0;
            XmlNodeList commandlist = doc.GetElementsByTagName("CommandLine");
            foreach (XmlNode item in commandlist)
            {
                if (item.InnerText.Contains("just-install "))
                {
                    break;
                }
                index++;
            }
            XmlNode programNode = commandlist[index];
            programNode.InnerText = "just-install " + programsToInstall;

            //Save xml
            doc.Save(outputFilePath);

            //Ventoy edit config
            var outputFilePathforVentoy = Path.GetFullPath(outputFilePath).Substring(2, Path.GetFullPath(outputFilePath).Length-2);
            outputFilePathforVentoy = outputFilePathforVentoy.Replace('\\', '/');
            if (!File.Exists(ventoyConfigPath))
            {
                PrintAndWait("Nie znaleziono pliku " + ventoyConfigPath);
                return;
            }
            var configv = File.ReadAllText(ventoyConfigPath);
            JObject ventoyconf = JObject.Parse(configv);
            int WindowsIsoLocation =0;
            foreach (var element in ventoyconf["auto_install"])
            {
                if (element["image"].ToString().ToLower().Contains("windows"))
                {
                    break;
                }
                WindowsIsoLocation++;
            }
            var xd = ventoyconf["auto_install"][WindowsIsoLocation].SelectToken("template");
            xd.Last.AddAfterSelf(outputFilePathforVentoy);
            File.WriteAllText(ventoyConfigPath, ventoyconf.ToString());
        }

        static void PrintAndWait(string komunikat="")
        {
            Console.WriteLine(komunikat);
            Console.ReadLine();
        }

        static string getXMLfile(){
            string file =null;
            while (file == null) {
                Console.WriteLine("Podaj ścieżkę do pliku xml");
                file = Console.ReadLine();
                if (!File.Exists(file))
                {
                    Console.WriteLine("ścieżka nie istnieje");
                    file = null;
                }
            }
            return file;
        }
        static string GetProgramsToInstall(string defaultPrograms, string otherProgramToDisplay)
        {
            Console.WriteLine("Wybierz programy do instalacji. Aby wybrać domyślne wybierz enter");
            Console.WriteLine("Domyślne programy: " + defaultPrograms);
            Console.WriteLine("Inne dostępne porgramy:" + otherProgramToDisplay);
            var choose = Console.ReadLine();
            if (choose == "")
            {
                choose = defaultPrograms;
            }
            Console.WriteLine("Wybrane programy: " + choose);
            return choose;
        }
        static string GetKey(string keyGetMethod=null)
        {
            if (keyGetMethod==null || keyGetMethod=="" || keyGetMethod == "ask") { 
                Console.WriteLine("Podaj klucz do Windowsa lub kliknij enter aby poszukać keyfinderem");
                var choose = Console.ReadLine();
                if (choose == "")
                {
                    keyGetMethod = KeyDecoder.GetWindowsProductKeyFromRegistry();
                }
                else 
                {
                    keyGetMethod = choose;
                }
            }else if (keyGetMethod == "KeyFind")
            {
                keyGetMethod = KeyDecoder.GetWindowsProductKeyFromRegistry();
            }
            Console.WriteLine("Klucz:" + keyGetMethod);
            return keyGetMethod;
        }
    }
}
