namespace Net.Share
{
    public delegate void RPCModelEvent(RPCModel model);
    public delegate void RPCModelEvent<Player>(Player client, RPCModel model);
    public delegate void OnOperationSyncEvent(in OperationList operList);
    public delegate void OnOperationEvent(in Operation opt);
}
