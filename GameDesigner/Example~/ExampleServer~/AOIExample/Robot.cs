using ECS;
using Net;
using Net.AOI;
using Net.Component;
using Net.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOIExample
{
    public class Robot : Component, IGridBody, IGridActor, IUpdate
    {
        public int ID { get; set; }
        public int Identity { get; set; }
        public Vector3 Position { get; set; }
        public Grid Grid { get; set; }
        public int ActorID { get ; set ; }
        public int Hair { get ; set ; }
        public int Head { get ; set ; }
        public int Jacket { get ; set ; }
        public int Belt { get ; set ; }
        public int Pants { get ; set ; }
        public int Shoe { get ; set ; }
        public int Weapon { get ; set ; }
        public bool MainRole { get; set; }

        public NTransform transform = new NTransform();

        public float speed = 6;
        public float movementDistance = 20;
        private bool moving;
        private Vector3 start;
        private Vector3 destination;

        public void OnBodyUpdate()
        {
            Position = transform.position;
        }

        public void OnEnter(IGridBody body)
        {
            
        }

        public void OnExit(IGridBody body)
        {
            
        }

        internal void Start()
        {
            start = transform.position;
            OnBodyUpdate();
        }

        public void OnUpdate()
        {
            if (moving)
            {
                if (Vector3.Distance(transform.position, destination) <= 0.01f)
                {
                    moving = false;
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, destination, speed * 0.033f);
                }
            }
            else
            {
                Vector2 circlePos = new Vector3(RandomHelper.Range(-1f, 1f), 0f, RandomHelper.Range(-1f, 1f));
                Vector3 dir = new Vector3(circlePos.x, 0, circlePos.y);
                Vector3 dest = transform.position + dir * movementDistance;
                if (Vector3.Distance(start, dest) <= movementDistance)
                {
                    destination = dest;
                    moving = true;
                }
            }
        }
    }
}
