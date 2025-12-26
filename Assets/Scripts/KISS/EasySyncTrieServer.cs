using System.Text;

namespace KISS
{
    public class EasySyncTrieServer
    {
        public class TrieSyncNtf
        {
            private readonly StringBuilder _sb = new();
            
            public TrieSyncNtf AppendNodeDelete(string node)
            {
                _sb.Append($"{node}:0\n");
                return this;
            }

            public TrieSyncNtf AppendNodeSet(string node, string value)
            {
                _sb.Append($"{node}:1{value}\n");
                return this;
            }

            public string PopContent()
            {
                string content = _sb.ToString().TrimEnd('\n');
                _sb.Clear();
                return content;
            }
        }

        public string SyncInit()
        {
            var sync =  new TrieSyncNtf();
            sync.AppendNodeSet("task.main.1001.prog", "100");
            sync.AppendNodeSet("task.main.1001.recv", "false");
            sync.AppendNodeSet("task.main.1002.prog", "50");
            sync.AppendNodeSet("task.main.1002.recv", "true");
            return sync.PopContent();
        }
    }
}