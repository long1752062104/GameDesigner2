using Net.Server;
using Net.Share;

public class GameScene : NetScene<GamePlayer>
{
    public override void Update(IServerSendHandle<GamePlayer> handle, byte cmd)
    {
        base.Update(handle, cmd);
    }
}
