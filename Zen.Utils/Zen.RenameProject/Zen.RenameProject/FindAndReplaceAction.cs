using System;
using System.IO;
using System.Text;

namespace Zen.RenameProject
{
    public class FindAndReplaceAction:ActionBase
    {
        private readonly string _from;
        private readonly string _to;
        private readonly string _path;
        private Func<string, string> _rep;

        public FindAndReplaceAction(string path, string @from, string to, Func<string,string>  rep = null)
        {
            _path = path;
            _from = @from;
            _to = to;

            if (rep == null)
                rep = s => s.Replace(_from, _to);
            _rep = rep;
        }

        public override void Action()
        {
            var enc = Encoding.Default;
            string contents = "";
            using (var rdr=File.OpenRead(_path))
            {
                var buffer=new byte[(int)rdr.Length];
                rdr.Read(buffer, 0, buffer.Length);
                contents = enc.GetString(buffer);
            }

            contents = _rep(contents);
            using (var writer=File.Create(_path))
            {
                var buffer = enc.GetBytes(contents);
                writer.Write(buffer,0,buffer.Length);
            }
        }

        public override string ToString()
        {
            return string.Format("FindAndReplace {0} {1} {2}", _from, _to, _path);
        }
    }
}