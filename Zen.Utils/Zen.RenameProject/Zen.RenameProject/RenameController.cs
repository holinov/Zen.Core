using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zen.RenameProject
{
    public class RenameController
    {
        private readonly string _fileName;
        private readonly string _renameFrom;
        private readonly string _renameTo;
        private readonly bool _renameFiles;
        private readonly bool _renameDirs;
        private List<ActionBase> _actions;

        public RenameController(string fileName, string renameFrom, string renameTo, bool renameFiles = true,
                                bool renameDirs = true)
        {
            _fileName = fileName;
            _renameFrom = renameFrom;
            _renameTo = renameTo;
            _renameFiles = renameFiles;
            _renameDirs = renameDirs;
        }

        public void DetectWork()
        {
            var rootPath = System.IO.Path.GetDirectoryName(_fileName);
            Actions=ScanDir(rootPath);
        }

        public List<ActionBase> Actions
        {
            get { return _actions; }
            private set { _actions = value; }
        }

        private string[] _filter = new[]
            {
                ".dll", ".obj",".suo",".exe",".pdb"
            };
        private List<ActionBase> ScanDir(string rootPath)
        {
            var res = new List<ActionBase>();
            if (rootPath.EndsWith(".git")) return res;

            //Найти все файлы для переименования и поиска и замены
            foreach (string fileName in Directory.EnumerateFiles(rootPath).Where(fn=>!_filter.Contains(System.IO.Path.GetExtension(fn))))
            {
                var fileNameShort = System.IO.Path.GetFileName(fileName);
                var fileDir = System.IO.Path.GetDirectoryName(fileName);
                if (_renameFiles && fileNameShort.Contains(_renameFrom))
                    res.Add(new RenameFileAction(fileName,
                                                 System.IO.Path.Combine(fileDir,
                                                                        fileNameShort.Replace(_renameFrom, _renameTo))));

                res.Add(new FindAndReplaceAction(fileName, _renameFrom, _renameTo));
            }

            //Сканировать вложенные папки
            foreach (string directory in Directory.EnumerateDirectories(rootPath))
            {
                var dirName = directory.Split('\\').Last();
                var dirPath = System.IO.Path.GetDirectoryName(directory);

                if (_renameDirs && dirName.Contains(_renameFrom))
                    res.Add(new RenameDirAction(directory,
                                                System.IO.Path.Combine(dirPath, dirName.Replace(_renameFrom, _renameTo))));
                res.AddRange(ScanDir(directory));
            }

            return res;
        }
    }
}