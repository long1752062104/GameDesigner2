using Net.Server;

namespace AOIExample
{
    class Service : TcpServer<Client,Scene>
    {
        internal Scene MainScene 
        {
            get 
            {
                Scenes.TryGetValue(MainSceneName, out var scene);
                return scene;
            }
        }
    }
}
