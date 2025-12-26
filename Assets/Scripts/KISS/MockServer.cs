using System;

namespace KISS
{
    public class MockServer: MonoSingleton<MockServer>
    {
        private EasySyncTrieServer _trieServer = new EasySyncTrieServer();

        private void Start()
        {
            MockClient.Instance.Sync(_trieServer.SyncInit(), true);
        }
    }
}