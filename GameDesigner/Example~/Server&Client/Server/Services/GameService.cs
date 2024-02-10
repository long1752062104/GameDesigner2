using Net.Server;
using Net.Share;
using Net.Unity;

public class GameService : TcpServer<GamePlayer, GameScene>
{
    protected override void OnRpcExecute(GamePlayer client, RPCModel model)
    {
        UnityThreadContext.Call(base.OnRpcExecute, client, model);
    }

    public void SceneUpdate()
    {
        foreach (var scene in Scenes.Values)
        {
            scene.UpdateLock(this, NetCmd.OperationSync);
        }
    }

    protected override void SceneUpdateHandle()
    {
        //什么都不做, 不让多线程执行场景更新
    }
}
