using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using log4net;

namespace Zen.NugetPacker
{
    class NugetPackerProgram
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            var ser = new XmlSerializer(typeof(PackagesConfig));
            PackagesConfig cfg;
            List<NugetPackage> packageList;

            string configFile = "packagesList.xml";

            if (!File.Exists(configFile))
            {
                cfg=new PackagesConfig()
                    {
                        BuildType = "Release",
                        SolutionPath = "C:\\src\\Zen.Core",
                        VersionString = "1.1.0.18"
                    };
                packageList = new List<NugetPackage>()
                    {
                        new NugetPackage()
                            {
                                Name = "Zen.Core",
                                Projects = new List<Project>()
                                    {
                                        new Project() {Name = "Zen"},
                                        new Project() {Name = "Zen.DataStore"},
                                    }
                            }
                    };
                cfg.Packages = packageList;
                
                using (var wr = File.CreateText(configFile))
                {
                    ser.Serialize(wr, cfg);
                }
            }
            else
            {
                using (var rdr = File.OpenText(configFile))
                {
                    cfg = (PackagesConfig)ser.Deserialize(rdr);
                }
            }

            //Console.WriteLine("1");
            var nuget = new NugetRunner(cfg.SolutionPath, cfg.BuildType, cfg.VersionString,cfg.Publish,cfg.PublishKey);
            //nuget.Update();

            foreach (var nugetPackage in cfg.Packages)
            {
                nuget.CopyPackageFiles(nugetPackage);                
                nuget.Pack(nugetPackage);
            }
            //Console.WriteLine("2");
            Console.WriteLine("Процесс сборки пакетов завершен. Нажмите любую клавишу для продолжения.");
            Console.ReadKey();
        }
    }

    public class PackagesConfig
    {
        [XmlAttribute]
        public string SolutionPath { get; set; }
        
        [XmlAttribute]
        public string BuildType { get; set; }
        
        [XmlAttribute]
        public string VersionString { get; set; }
        
        [XmlAttribute]
        public string PublishKey { get; set; }
        
        [XmlAttribute]
        public bool Publish { get; set; }

        [XmlArray]
        public List<NugetPackage> Packages { get; set; }
    }

    internal class NugetRunner
    {
        private const string VersionPlaceholder = "{{VERSION}}";
        private string _solutionPath;
        private string _buildType;
        private readonly string _versionString;
        private readonly bool _publish;
        private readonly string _publishKey;
        private readonly string _nugetExe ;
        private static readonly ILog Log = LogManager.GetLogger(typeof (NugetRunner));

        public NugetRunner(string solutionPath, string buildType, string versionString, bool publish, string publishKey, string nugetExe = "NuGet.exe")
        {
            _solutionPath = solutionPath;
            _nugetExe = nugetExe;
            _buildType = buildType;
            _versionString = versionString;
            _publish = publish;
            _publishKey = publishKey;
        }

        public void Update()
        {
            Log.Info("Запуск обновления");
            var process = Process.Start(_nugetExe, "Update -self");
            if (process != null)
            {
                process.WaitForExit();
                Log.Info("Обновление закончено");
            }
        }

        public void CopyPackageFiles(NugetPackage package)
        {
            var packageDir = GetPackageDir(package);

            Log.Info("Начало копирования файлов пакета "+package.Name);
            foreach (var project in package.Projects)
            {
                var from = Path.Combine(_solutionPath, project.Name, "bin", _buildType, project.Name);
                var to = Path.Combine(packageDir, "lib","net45", project.Name);
                var ext = "";
                if (File.Exists(from + ".dll"))
                {
                    ext = ".dll";
                }
                else
                {
                    ext = ".exe";
                }

                from += ext;
                to += ext;
                var path = Path.GetDirectoryName(to);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                Log.InfoFormat("Копирование файла {0} в {1}", from.Replace(_solutionPath, ""),
                               to.Replace(_solutionPath, ""));
                File.Copy(from, to, true);
            }
        }

        private string GetPackageDir(NugetPackage package)
        {
            return Path.Combine(_solutionPath, "nuget", "Templates", package.Name);
        }

        public void Pack(NugetPackage package)
        {
            Log.Info("Начало упаковки " + package.Name);
            var realNuspec = PrepareNuspec(package);

            var startInfo = new ProcessStartInfo(_nugetExe, "pack " + realNuspec)
                {
                    WorkingDirectory = Path.GetDirectoryName(realNuspec),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,                    
                };
            var process = Process.Start(startInfo);
            //var process = Process.Start(_nugetExe, "pack " + realNuspec);
            if (process != null)
            {
                process.WaitForExit();
                
                var sb = new StringBuilder("Запуск NuGet.exe pack для файла ").Append(realNuspec)
                    .AppendLine()
                    .AppendLine(process.StandardOutput.ReadToEnd());

                if(process.ExitCode==0)
                Log.Info(sb.ToString());
                else Log.Error(sb.ToString());
            }
            File.Delete(realNuspec);
            if (_publish)
            {
                Log.Info("Установка ключа публикации " + _publishKey);
                process = Process.Start(_nugetExe, "SetApiKey " + _publishKey);
                if (process != null)
                {
                    process.WaitForExit();
                }

                startInfo = new ProcessStartInfo(_nugetExe, "Push " + realNuspec.Replace(".nuspec",".nupkg"))
                {
                    WorkingDirectory = Path.GetDirectoryName(realNuspec),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,                    
                };
                process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();

                    var sb = new StringBuilder("Запуск публикации пакета")
                        .Append(realNuspec)
                        .AppendLine()
                        .AppendLine(process.StandardOutput.ReadToEnd());

                    if (process.ExitCode == 0)
                        Log.Info(sb.ToString());
                    else Log.Error(sb.ToString());
                }
            }
        }

        private string PrepareNuspec(NugetPackage package)
        {
            var fileName = package.Name + ".nuspec";
            var targetFileName = string.Format("{0}.{1}.nuspec", package.Name, _versionString);
            var from = Path.Combine(GetPackageDir(package), fileName);
            var to = GetRealNuspecPath(package, targetFileName);
            var log = new StringBuilder();
            log.AppendLine("Подготовка метаданных")
               .AppendFormat("Файл {0} {1}найден", from, File.Exists(from) ? "" : "не")
               .AppendLine();
            bool foundPlaceholder = false;
            using (var rdr = File.OpenText(from))
            using (var writer = File.CreateText(to))
            {
                var nuspec = rdr.ReadToEnd();
                if (nuspec.Contains(VersionPlaceholder))
                {
                    foundPlaceholder = true;
                    log.AppendLine("Найдено место для вставки версии");
                    nuspec = nuspec.Replace(VersionPlaceholder, _versionString);
                }
                else
                {
                    log.AppendLine("Не найдено место вставки версии в файле " + from);
                }
                writer.Write(nuspec);
            }
            log.AppendLine("Записан файл реальной конфигурации " + targetFileName);

            if (!File.Exists(from) || !foundPlaceholder)
                Log.Warn(log.ToString());
            else
                Log.Info(log.ToString());
            return to;
        }

        private string GetRealNuspecPath(NugetPackage package, string targetFileName)
        {
            return Path.Combine(GetPackageDir(package), targetFileName);
        }
    }

    public class NugetPackage
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlArray]
        public List<Project> Projects { get; set; }
    }

    public class Project
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}
