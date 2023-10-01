using Net;
using Net.AI;
using System;

namespace Example2.Model 
{
    [Serializable]
    public class Monster : ActorModel
    {
        public AgentEntity Agent;
        public PlayerModel target;

        public void OnLogicUpdate()
        {
            if (target == null)
                return;
            if (Vector3.Distance(target.transform.Position, transform.Position) < 3)
            {

            }
        }
    }
}