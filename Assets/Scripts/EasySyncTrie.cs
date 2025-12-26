using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DefaultNamespace
{
    public class EasySyncTrie
    {
        
        /// <summary>
        /// a.b.c.d:1str.:i:fasd
        /// a.attr.atk:1100
        /// a.hero.100:0
        /// alias.01:a.b.c.d:
        /// .01:str
        /// </summary>
        private readonly Dictionary<string, string> _trie = new();
        private readonly Dictionary<string, Action<NodeSyncOp>> _listeners = new();

        #region Ntf

        public enum NodeSyncOp
        {
            Insert,
            Delete,
            Update,
            
            TrieSync,
        }

        private List<string> NtfNodeDelete(string node)
        {
            List<string> deleteList = new();
            deleteList.Add(node);
            string prefixChild = node + '.';
            foreach (var n in _trie.Keys)
            {
                if (n.StartsWith(prefixChild))
                {
                    deleteList.Add(n);
                }
            }

            foreach (var n in deleteList)
            {
                _trie.Remove(n);
            }
            
            return deleteList;
        }

        private NodeSyncOp NtfNodeSet(string node, string val)
        {
            // bool preNil = true;
            // if (_trie.TryGetValue(node, out string preVal))
            // {
            //     preNil = preVal.StartsWith('0');
            // }
            // bool nil = val.StartsWith('0');
            bool preNil = !_trie.ContainsKey(node);
            
            _trie[node] = val;

            // if (nil)
            // {
            //     return NodeSyncOp.Delete;
            // }
            if (preNil)
            {
                return NodeSyncOp.Insert;
            }
            return NodeSyncOp.Update;
        }
        
        public void NtfSync(string content, bool broadcast=true)
        {
            string[] lines = content.Split("\n");
            List<(string, NodeSyncOp)> syncNodes = new();
            foreach (string line in lines)
            {
                int colonIndex = line.IndexOf(':');
                string node = line.Substring(0, colonIndex);
                string opAndVal = line.Substring(colonIndex + 1);
                
                bool delete = opAndVal.StartsWith('0');
                if (delete)
                {
                    var list = NtfNodeDelete(node);
                    syncNodes.AddRange(list.Select(n => (n, NodeSyncOp.Delete)));
                    continue;
                }

                var val = opAndVal.Substring(1);
                var op = NtfNodeSet(node, val);
                syncNodes.Add((node, op));
            }

            if (broadcast)
            {
                foreach (var (node, op) in syncNodes)
                {
                    if (_listeners.TryGetValue(node, out var listener))
                    {
                        listener?.Invoke(op);
                    }
                }
            }
        }

        #endregion

        public string GetNodeValString(string node)
        {
            return _trie.GetValueOrDefault(node, null);
            // if (!_trie.TryGetValue(node, out var val))
            // {
            //     return null;
            // }
            // if (val.StartsWith('0'))
            // {
            //     return null;
            // }
            // return val.Substring(1);
        }

        public int? GetNodeValInt(string node)
        {
            var str = GetNodeValString(node);
            if (str == null)
            {
                return null;
            }
            return int.Parse(str);
        }

        public float? GetNodeValFloat(string node)
        {
            var str = GetNodeValString(node);
            if (str == null)
            {
                return null;
            }
            return float.Parse(str);
        }
    }
}