﻿using Net.Component;
using Net.Event;
using Net.Server;
using Net.Share;

namespace LockStep.Server
{
    public class Service : UdxServer<Player, Scene>
    {
        protected override void OnStartupCompleted()
        {
            RemoveScene(MainSceneName, false);
            SyncSceneTime = 16;
        }

        protected override void OnOperationSync(Player client, OperationList list)
        {
            foreach (Operation opt in list.operations)
            {
                switch (opt.cmd)
                {
                    case Command.Input:
                        client.Scene?.AddOperation(opt);
                        break;
                }
            }
        }

        [Rpc(NetCmd.SafeCall)]
        void CreateRoom(Player client, string name)
        {
            Scene scene = CreateScene(client, name);
            NDebug.Log($"创建房间:{scene != null}");
            if (scene == null)
            {
                Call(client, "CreateRoomCallback", "创建失败, 房间已经存在!");
                return;
            }
            scene.sceneCapacity = 1;
            Call(client, "CreateRoomCallback", "创建成功!");
        }

        [Rpc(NetCmd.SafeCall)]
        void JoinRoom(Player client, string name)
        {
            Scene scene = JoinScene(client, name);
            NDebug.Log($"加入房间:{scene != null}");
            if (scene == null)
            {
                Call(client, "JoinRoomCallback", "加入失败, 房间不存在!");
                return;
            }
            scene.AddPlayer(client);
            Call(client, "JoinRoomCallback", "加入成功!");
        }

        [Rpc(NetCmd.SafeCall)]
        void ExitRoom(Player client)
        {
            if (client.Scene.Count == 1)
                RemoveScene(client.SceneName, false);
            else
                ExitScene(client, false);
            Call(client, "ExitRoomCallback", "退出房间!");
            NDebug.Log($"退出房间!");
        }

        [Rpc(NetCmd.SafeCall)]
        void StartBattle(Player client)
        {
            Scene scene = client.Scene;
            if (scene == null)
            {
                NDebug.Log("空场景:" + client.PlayerID);
                return;
            }
            if (scene.battle)
                return;
            client.readyBattle = true;
            int readyCount = 0;
            foreach (var p in scene.Clients)
            {
                if (p.readyBattle)
                    readyCount++;
            }
            NDebug.Log($"准备:{readyCount}/{scene.Count}");
            if (readyCount == scene.Count)
            {
                NDebug.Log("开始同步!");
                Multicast(scene.Clients, "StartGameSync");
                client.Scene.battle = true;
                if (scene.check)
                    Net.Event.EventSystem.RemoveEvent(scene.actionId);
                return;
            }
            if (!scene.check)
            {
                scene.check = true;
                scene.actionId = Net.Event.EventSystem.AddEvent(60000, () =>
                {//如果在60秒内, 其他玩家还没加载完成, 那就不等了, 直接游戏开始
                    if (client.Scene == null)
                        return;
                    Multicast(scene.Clients, "StartGameSync");
                    client.Scene.battle = true;
                    NDebug.Log("不等待!");
                });
            }
        }

        [Rpc(NetCmd.SafeCall)]
        void ExitBattle(Player client)
        {
            client.readyBattle = false;
            Scene scene = client.Scene;
            if (scene == null)
            {
                NDebug.Log("空场景:" + client.PlayerID);
                return;
            }
            scene.Remove(client);
            if (scene.Count == 0)
                RemoveScene(scene.Name, false);
            Call(client, "ExitBattle", pars: client.UserID);
            NDebug.Log("退出战斗");
        }
    }
}
