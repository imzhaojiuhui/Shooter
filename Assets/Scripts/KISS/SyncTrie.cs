using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace KISS
{
    public class SyncTrie
    {
        /// <summary>
        /// a.b.c.d:str.:fasd
        /// </summary>
        private readonly Dictionary<string, byte[]> _trie = new();

        public void NtfNodeAdd(string node, byte[] val)
        {
            _trie[node] = val;
        }

        public void NtfNodeRemove(string node)
        {
            _trie.Remove(node);
        }

        public void NtfNodeUpdate(string node, byte[] val)
        {
            _trie[node] = val;
        }

        public string GetNodeValString(string node)
        {
            return Encoding.UTF8.GetString(_trie[node]);
        }

        public int GetNodeValInt(string node)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(_trie[node]);
        }

        public float GetNodeValFloat(string node)
        {
            int val = GetNodeValInt(node);
            return BitConverter.Int32BitsToSingle(val);
        }
    }
}