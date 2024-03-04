namespace Net.Share
{
    public delegate void RPCModelEvent(RPCModel model);
    public delegate void RPCModelEvent<Player>(Player client, RPCModel model);
}
