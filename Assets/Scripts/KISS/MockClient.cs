namespace KISS
{
    public class MockClient: MonoSingleton<MockClient>
    {
        private EasySyncTrie _trieClient = new EasySyncTrie();

        public void Sync(string data, bool init)
        {
            _trieClient.NtfSync(data, !init);
        }
    }
}