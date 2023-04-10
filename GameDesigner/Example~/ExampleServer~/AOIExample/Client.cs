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

        internal ListSafe<Operation> operations = new ListSafe<Operation>();
        internal bool isSending;

        public void OnEnter(IGridBody body)
        {
            var actor = body as Robot;
            if (actor == null)
                return;
            operations.Add(new Operation(Command.EnterArea, body.Identity, actor.Position, actor.transform.rotation)
            {
                index = actor.ActorID,
            });
        }

        public void OnExit(IGridBody body)
        {
            var actor = body as IGridActor;
            operations.Add(new Operation(Command.ExitArea, body.Identity)
            {
                index = actor.ActorID,
            });
        }

        public void OnBodyUpdate()
        {
        }
    }
}