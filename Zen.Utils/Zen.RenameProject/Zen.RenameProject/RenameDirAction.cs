using System.IO;

namespace Zen.RenameProject
{
    public class RenameDirAction : ActionBase
    {
        private readonly string _from;
        private readonly string _to;

        public RenameDirAction(string @from, string to)
        {
            _from = @from;
            _to = to;
        }

        public override void Action()
        {
            Directory.Move(_from, _to);
        }
        public override string ToString()
        {
            return string.Format("RenameDir {0} {1}", _from, _to);
        }
    }
}