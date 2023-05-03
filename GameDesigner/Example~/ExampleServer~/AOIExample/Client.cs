using Net.Server;
using Net.AOI;
using Net;
using Net.Share;
using Net.System;

namespace AOIExample
{
    internal class Client : NetPlayer, IGridBody, IGridActor
    {
        public int ID { get ; set ; }
        public int Identity { get; set; }
        public Vector3 Position { get ; set ; }
        public Grid Grid { get ; set ; }
        public int Hair { get ; set ; }
        public int Head { get ; set ; }
        public int Jacket { get ; set ; }
        public int Belt { get ; set ; }
        public int Pants { get ; set ; }
        public int Shoe { get ; set ; }
        public int Weapon { get ; set ; }
        public int ActorID { get; set; }
        public bool MainRole { get; set; }

        internal ListSafe<Operation> currOpers = new ListSafe<Operation>();//发给其他客户端
        internal FastList<Operation> selfOpers = new FastList<Operation>();//只能发给自己 -- 只有场景线程执行
        internal Operation[] preOpers;
        internal uint frame;

        public void OnEnter(IGridBody body)
        {
            var actor = body as IGridActor;
            selfOpers.Add(new Operation(Command.EnterArea, body.Identity, body.Position, Quaternion.identity)
            {
                index = actor.ActorID,
            });
            if (body is Robot robot)
                robot.roleCount++;
        }

        public void OnExit(IGridBody body)
        {
            var actor = body as IGridActor;
            selfOpers.Add(new Operation(Command.ExitArea, body.Identity)
            {
                index = actor.ActorID,
            });
            if (body is Robot robot)
                robot.roleCount--;
        }

        public void OnBodyUpdate()
        {
        }
    }
}