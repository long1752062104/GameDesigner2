using GameDesigner;
using System.Collections.Generic;
using UnityEngine;

namespace StateExample
{
    public class Player : MonoBehaviour
    {
        public float moveSpeed;
        public StateManager sm;

        public AudioClip[] audioClips;

        // Start is called before the first frame update
        void Start()
        {
            sm.stateMachine.animMode = AnimationMode.Animation; //使用旧版本动画模式

            AddState("idle", true, AnimPlayMode.Sequence, "idle", new StateBehaviour[] { new IdleState() }, null); //添加idle状态 和 状态行为
            AddState("move", true, AnimPlayMode.Sequence, "run", new StateBehaviour[] { new MoveState() }, null); //添加run状态 和 状态行为

            var attack = AddState("attack", false, AnimPlayMode.Sequence, "default_attack", null, new ActionBehaviour[] { new AttackState() }); //添加attack状态, 并添加动作组件
            attack.AddTransition(0); //添加连线到idle状态
            attack.ActionEndTransfer(0); //动作结束后跳到idle状态

            var skill_1 = AddState("skill_1", false, AnimPlayMode.Sequence, "frenzied_slash", null, new ActionBehaviour[] { //添加skill_1状态, 并添加动作组件
                new AttackState(),
                new ActionAudio() //音效组件
                {
                    audioClips = new List<AudioClip>(audioClips)
                }
            });
            skill_1.AddTransition(0);
            skill_1.ActionEndTransfer(0);

            var skill_2 = AddState("skill_2", false, AnimPlayMode.Sequence, "gethit_from_behind", null, new ActionBehaviour[] { 
                new AttackState(), 
                new ActionAudio() 
                {
                    audioClips = new List<AudioClip>(audioClips)
                }
            });
            skill_2.AddTransition(0);
            skill_2.ActionEndTransfer(0);

            var skill_3 = AddState("skill_3", false, AnimPlayMode.Sequence, "shield_bash", null, new ActionBehaviour[] { 
                new AttackState(),
                new ActionAudio()
                {
                    audioClips = new List<AudioClip>(audioClips)
                }
            });
            skill_3.AddTransition(0);
            skill_3.ActionEndTransfer(0);

            var jump = AddState("jump", false, AnimPlayMode.Sequence, "jump", null, new ActionBehaviour[] { new JumpState() });
            jump.AddTransition(0);
            jump.ActionEndTransfer(0);

            var damage = AddState("damage", false, AnimPlayMode.Sequence, "gethit_from_left", null, null);
            damage.AddTransition(0);
            damage.ActionEndTransfer(0);

            AddState("death", false, AnimPlayMode.Sequence, "die", null, null);
        }

        private State AddState(string name, bool animLoop, AnimPlayMode animPlayMode, string clipName, StateBehaviour[] stateBehaviours, ActionBehaviour[] actionBehaviours)
        {
            var state = sm.stateMachine.AddState(name, stateBehaviours);
            state.animLoop = animLoop;
            state.animPlayMode = animPlayMode;
            state.actionSystem = true;
            state.isCrossFade = true;
            state.duration = 0.1f;
            state.animSpeed = 1f;
            var action = state.GetAction(0);
            action.SetAnimClip(clipName);
            action.AddComponent(actionBehaviours);
            return state;
        }

        public Vector3 Direction => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        public Vector3 Transform3Dir(Transform t, Vector3 dir)
        {
            var f = Mathf.Deg2Rad * (-t.rotation.eulerAngles.y);
            dir.Normalize();
            var ret = new Vector3(dir.x * Mathf.Cos(f) - dir.z * Mathf.Sin(f), 0, dir.x * Mathf.Sin(f) + dir.z * Mathf.Cos(f));
            return ret;
        }

        internal void CheckKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                sm.ChangeState(2, 0, true);
                return;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                sm.ChangeState(3, 0, true);
                return;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                sm.ChangeState(4, 0, true);
                return;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                sm.ChangeState(5, 0, true);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                sm.ChangeState(6, 0, true);
                return;
            }
        }
    }

    public class IdleState : StateBehaviour
    {
        private Player self;

        public override void OnInit()
        {
            self = transform.GetComponent<Player>();
        }

        public override void OnUpdate()
        {
            var dir = self.Transform3Dir(Camera.main.transform, self.Direction);
            if (dir != Vector3.zero)
            {
                ChangeState(1);
                return;
            }
            self.CheckKeyDown();
        }
    }

    public class MoveState : StateBehaviour
    {
        private Player self;

        public override void OnInit()
        {
            self = transform.GetComponent<Player>();
        }

        public override void OnUpdate()
        {
            var dir = self.Transform3Dir(Camera.main.transform, self.Direction);
            if (dir == Vector3.zero)
            {
                ChangeState(0);
                return;
            }
            self.CheckKeyDown();
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), 0.5f);
            transform.Translate(0, 0, self.moveSpeed * Time.deltaTime);
        }
    }

    public class JumpState : ActionCoreBase
    {
        private Player self;

        public override void OnInit()
        {
            self = transform.GetComponent<Player>();
        }

        public override void OnAnimationEvent(StateAction action)
        {
            self.GetComponent<Rigidbody>().AddForce(self.transform.forward * 5f, ForceMode.Impulse);
        }
    }

    public class AttackState : ActionCore
    {
        private Player self;

        public override void OnInit()
        {
            self = transform.GetComponent<Player>();
        }

        public override void OnUpdate(StateAction action)
        {
            base.OnUpdate(action);
            if (Input.GetKeyDown(KeyCode.E))
            {
                ChangeState(3, 0, true);
                return;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ChangeState(4, 0, true);
                return;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                ChangeState(5, 0, true);
                return;
            }
        }
    }

    public class SkillState : ActionCore
    {
        private Player self;

        public override void OnInit()
        {
            self = transform.GetComponent<Player>();
        }

        public override void OnUpdate(StateAction action)
        {
            base.OnUpdate(action);
        }
    }
}