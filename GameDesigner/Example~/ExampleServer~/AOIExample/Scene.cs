using Net.Server;
using Net.AOI;
using Net.Share;
using System.Collections.Generic;
using Net.Component;
using Net.Event;
using Net.System;
using ECS;
using Example1;
using Net;

namespace AOIExample
{
    internal class Scene : NetScene<Client>
    {
        internal GridWorld gridWorld = new GridWorld();
        internal GSystem gSystem = new GSystem();

        public int spawnAmount = 10000;
        public float interleave = 5;

        public Scene() 
        {
            gridWorld.Init(-500, -500, 50, 50, 20, 20);

            //float sqrt = Mathf.Sqrt(spawnAmount);
            //float offset = -sqrt / 2 * interleave;
            //int spawned = 0;
            //for (int spawnX = 0; spawnX < sqrt; ++spawnX)
            //{
            //    for (int spawnZ = 0; spawnZ < sqrt; ++spawnZ)
            //    {
            //        if (spawned < spawnAmount)
            //        {
            //            var entity = gSystem.Create<Entity>();
            //            var robot = entity.AddComponent<Robot>();

            //            float x = offset + spawnX * interleave;
            //            float z = offset + spawnZ * interleave;
            //            robot.transform.position = new Vector3(x, 0, z);
            //            ++spawned;

            //            robot.Identity = spawned;
            //            robot.Start();
            //            gridWorld.Insert(robot);
            //        }
            //    }
            //}
        }
        public override void OnEnter(Client client)
        {
            client.Identity = client.UserID;
            client.MainRole = true;
            gridWorld.Insert(client);
        }
        public override void OnExit(Client client)
        {
            if (Count <= 0) //如果没人时要清除操作数据，不然下次进来会直接发送Command.OnPlayerExit指令给客户端，导致客户端的对象被销毁
                operations.Clear();
            else
                AddOperation(new Operation(Command.OnPlayerExit, client.UserID));
            gridWorld.Remove(client);
        }
        public override void OnOperationSync(Client client, OperationList list)
        {
            foreach (var item in list.operations)
            {
                switch (item.cmd) 
                {
                    case Command.Transform:
                        client.Position = item.position;
                        client.operations.Add(item);
                        break;
                    default:
                        client.operations.Add(item);
                        break;
                }
            }
        }
        public override void Update(IServerSendHandle<Client> handle, byte cmd = 19)
        {
            var players = Clients;
            int playerCount = players.Count;
            if (playerCount <= 0)
                return;
            frame++;
            int count;
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null)
                    continue;
                if (player.isSending)
                {
                    player.isSending = false;
                    continue;
                }
                player.OnUpdate();
                var operations = new FastList<Operation>();
                var grid = players[i].Grid;
                if (grid == null)
                    continue;
                var gridBodies = grid.GetGridBodiesAll();
                var players1 = new FastList<Client>();
                foreach (var item1 in gridBodies)
                {
                    if (item1 is Client player1)
                    {
                        var opts1 = player1.operations.GetRemoveRange(0, player1.operations.Count);
                        operations.AddRange(opts1);
                        player1.isSending = true;
                        players1.Add(player1);
                    }
                    else if (item1 is Robot robot)
                    {
                        operations.Add(new Operation(Command.RobotUpdate, robot.Identity, robot.Position, robot.transform.rotation)
                        {
                            index = robot.ActorID,
                        });
                    }
                }
                count = operations.Count;
                while (count > Split)
                {
                    OnPacket(handle, cmd, Split, players1, operations);
                    count -= Split;
                }
                if (count > 0)
                    OnPacket(handle, cmd, count, players1, operations);
            }
            count = operations.Count;//不管aoi, 整个场景的同步在这里, 如玩家退出操作
            while (count > Split)
            {
                OnPacket(handle, cmd, Split);
                count -= Split;
            }
            if (count > 0)
                OnPacket(handle, cmd, count);
            gridWorld.UpdateHandler();
            gSystem.Update();
        }
    }
}