using AOIExample;
using Net;
using Net.AOI;
using Net.Component;
using Net.Server;
using Net.Share;
using Net.System;
using System;
using System.Data;
using System.Threading;

namespace Example2
{
    /// <summary>
    /// 服务器玩家组件, 这个组件处理 玩家移动操作, 后面增加攻击操作, 碰撞操作....
    /// </summary>
    public class Player : WebPlayer, ISendHandle, IGridBody, IGridActor
    {
        public UserinfoData data;
        internal bool isDead;
        internal Scene scene;

        public int ID { get; set; }
        public int Identity { get; set; }
        public Vector3 Position { get; set; }
        public Grid Grid { get; set; }
        public int Hair { get; set; }
        public int Head { get; set; }
        public int Jacket { get; set; }
        public int Belt { get; set; }
        public int Pants { get; set; }
        public int Shoe { get; set; }
        public int Weapon { get; set; }
        public int ActorID { get; set; }
        public bool MainRole { get; set; }

        internal int health = 100;

        internal ListSafe<Operation> currOpers = new ListSafe<Operation>();//发给其他客户端和自己
        internal ListSafe<Operation> selfOpers = new ListSafe<Operation>();//只发给自己 -- 只有场景线程执行
        internal Operation[] preOpers;
        internal uint frame;

        public void OnEnter(IGridBody body)
        {
            if (body is Player player)
            {
                selfOpers.Add(new Operation(Command.EnterArea, body.Identity, player.Position, Quaternion.identity)
                {
                    index = 1, //区分类型, 1=玩家, 2=怪物
                    index1 = player.ActorID,
                    index2 = player.health,
                });
            }
            else if (body is AIMonster monster) 
            {
                monster.roleInCount++;
                selfOpers.Add(new Operation(Command.EnterArea, body.Identity, monster.Position, monster.Agent.Rotation)
                {
                    index = 2,
                    index1 = monster.ActorID,
                    index2 = monster.health,
                    direction = monster.destination, //怪物下一个位置, 客户端收到后会自动寻路到目的地
                });
            }
        }

        public void OnExit(IGridBody body)
        {
            if (body is Player player)
            {
                selfOpers.Add(new Operation(Command.ExitArea, body.Identity)
                {
                    index = 1, //区分类型, 1=玩家, 2=怪物
                    index1 = player.ActorID,
                });
            }
            else if (body is AIMonster monster)
            {
                monster.roleInCount--;
                selfOpers.Add(new Operation(Command.ExitArea, body.Identity)
                {
                    index = 2,
                    index1 = monster.ActorID,
                });
            }
        }

        public void OnBodyUpdate()
        {
        }

        public override void OnEnter()
        {
            health = 100;
            isDead = false;
            scene = Scene as Scene;
        }

        internal void BeAttacked(int damage)
        {
            if (isDead)
                return;
            health -= damage;
            if (health <= 0)
            {
                isDead = true;
                health = 0;
            }
        }

        public void Resurrection()
        {
            health = 100;
            isDead = false;
        }

        #region 扩展网络请求
        public void Send(byte[] buffer)
        {
            Service.Instance.Send(this, buffer);
        }

        public void Send(byte cmd, byte[] buffer)
        {
            Service.Instance.Send(this, cmd, buffer);
        }

        public void Send(string func, params object[] pars)
        {
            Service.Instance.Send(this, func, pars);
        }

        public void Send(byte cmd, string func, params object[] pars)
        {
            Service.Instance.Send(this, cmd, func, pars);
        }

        public void CallRpc(string func, params object[] pars)
        {
            Service.Instance.Send(this, func, pars);
        }

        public void CallRpc(byte cmd, string func, params object[] pars)
        {
            Service.Instance.Send(this, cmd, func, pars);
        }

        public void Request(string func, params object[] pars)
        {
            Service.Instance.Send(this, func, pars);
        }

        public void Request(byte cmd, string func, params object[] pars)
        {
            Service.Instance.Send(this, cmd, func, pars);
        }

        public void SendRT(string func, params object[] pars)
        {
            Service.Instance.SendRT(this, func, pars);
        }

        public void SendRT(byte cmd, string func, params object[] pars)
        {
            Service.Instance.SendRT(this, cmd, func, pars);
        }

        public void SendRT(byte[] buffer)
        {
            Service.Instance.SendRT(this, buffer);
        }

        public void SendRT(byte cmd, byte[] buffer)
        {
            Service.Instance.SendRT(this, cmd, buffer);
        }

        public void Send(string func, string callbackFunc, Action callback, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(byte cmd, string func, string callbackFunc, Action callback, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(string func, string callbackFunc, Action<object[]> callback, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(byte cmd, string func, string callbackFunc, Action<object[]> callback, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(string func, string callbackFunc, Delegate callback, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(byte cmd, string func, string callbackFunc, Delegate callback, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(byte cmd, object obj)
        {
            Service.Instance.Send(this, cmd, obj);
        }

        public void SendRT(byte cmd, object obj)
        {
            Service.Instance.SendRT(this, cmd, obj);
        }

        public void Send(string func, string funcCB, Delegate callback, int millisecondsDelay, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(string func, string funcCB, Delegate callback, int millisecondsDelay, Action outTimeAct, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(byte cmd, string func, string funcCB, Delegate callback, int millisecondsDelay, Action outTimeAct, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void SendRT(string func, string funcCB, Delegate callback, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void SendRT(string func, string funcCB, Delegate callback, int millisecondsDelay, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void SendRT(string func, string funcCB, Delegate callback, int millisecondsDelay, Action outTimeAct, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void SendRT(byte cmd, string func, string funcCB, Delegate callback, int millisecondsDelay, Action outTimeAct, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void SendRT(byte cmd, string func, string funcCB, Delegate callback, int millisecondsDelay, Action outTimeAct, SynchronizationContext context, params object[] pars)
        {
            throw new NotImplementedException();
        }

        public void Send(byte cmd, string func, string funcCB, Delegate callback, int millisecondsDelay, Action outTimeAct, SynchronizationContext context, params object[] pars)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}