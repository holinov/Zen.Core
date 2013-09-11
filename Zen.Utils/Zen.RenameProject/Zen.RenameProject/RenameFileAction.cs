using System.IO;

namespace Zen.RenameProject
{
    public class RenameFileAction:ActionBase
    {
        private readonly string _from;
        private readonly string _to;

        public RenameFileAction(string from, string to)
        {
            _from = @from;
            _to = to;
        }

        public override void Action()
        {
            File.Move(_from,_to);
        }
        public override string ToString()
        {
            return string.Format("RenameFile {0} {1}", _from, _to);
        }
    }
}