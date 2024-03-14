using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace GameDesigner
{
    /// <summary>
    /// 音效模式
    /// </summary>
	public enum AudioMode
    {
        /// <summary>
        /// 进入状态播放音效
        /// </summary>
		EnterPlay,
        /// <summary>
        /// 动画事件播放音效
        /// </summary>
		AnimEvent,
        /// <summary>
        /// 退出状态播放音效
        /// </summary>
		ExitPlay
    }

    /// <summary>
    /// 技能生成模式
    /// </summary>
	public enum ActiveMode
    {
        /// <summary>
        /// 实例化
        /// </summary>
		Instantiate,
        /// <summary>
        /// 对象池
        /// </summary>
		ObjectPool,
        /// <summary>
        /// 激活模式
        /// </summary>
        Active,
    }

    /// <summary>
    /// 技能物体设置模式
    /// </summary>
	public enum SpwanMode
    {
        None,
        /// <summary>
        /// 设置技能物体在自身位置
        /// </summary>
		localPosition,
        /// <summary>
        /// 设置技能物体在父对象位置和成为父对象的子物体
        /// </summary>
		SetParent,
        /// <summary>
        /// 设置技能物体在父对象位置
        /// </summary>
		SetInTargetPosition,
        /// <summary>
        /// 激活模式的自身位置
        /// </summary>
        ActiveLocalPosition,
    }

    public class ActionCoreBase : ActionBehaviour
    {
        /// <summary>
        /// 动画事件时间
        /// </summary>
		public float animEventTime = 50f;
        /// <summary>
        /// 是否已到达事件时间或超过事件时间，到为true，没到为flase
        /// </summary>
		[HideField]
        protected bool eventEnter;

        public override void OnEnter(StateAction action)
        {
            eventEnter = false;
        }

        public override void OnUpdate(StateAction action)
        {
            if (action.animTime >= animEventTime & !eventEnter)
            {
                eventEnter = true;
                OnAnimationEvent(action);
            }
        }

        /// <summary>
        /// 当动画事件触发
        /// </summary>
        public virtual void OnAnimationEvent(StateAction action) { }

        public override void OnExit(StateAction action)
        {
            eventEnter = false;
        }
    }

    public class ActionAudio : ActionCoreBase
    {
        /// <summary>
        /// 音效触发模式
        /// </summary>
		public AudioMode audioModel = AudioMode.AnimEvent;
        /// <summary>
        /// 音效剪辑
        /// </summary>
		public List<AudioClip> audioClips = new List<AudioClip>();

        public override void OnEnter(StateAction action)
        {
            base.OnEnter(action);
            if (audioModel == AudioMode.EnterPlay & audioClips.Count > 0)
            {
                var audioIndex = Random.Range(0, audioClips.Count);
                AudioManager.Play(audioClips[audioIndex]);
            }
        }

        public override void OnAnimationEvent(StateAction action)
        {
            if (audioModel == AudioMode.AnimEvent & audioClips.Count > 0)
            {
                var audioIndex = Random.Range(0, audioClips.Count);
                AudioManager.Play(audioClips[audioIndex]);
            }
        }

        public override void OnExit(StateAction action)
        {
            base.OnExit(action);
            if (audioModel == AudioMode.ExitPlay & audioClips.Count > 0)
            {
                var audioIndex = Random.Range(0, audioClips.Count);
                AudioManager.Play(audioClips[audioIndex]);
            }
        }
    }

    /// <summary>
    /// 内置的技能动作行为组件, 此组件包含播放音效, 处理技能特效
    /// </summary>
    public class ActionCore : ActionCoreBase
    {
        /// <summary>
        /// 技能粒子物体
        /// </summary>
		public GameObject effect = null;
        /// <summary>
        /// 粒子物体生成模式
        /// </summary>
		public ActiveMode activeMode = ActiveMode.Instantiate;
        /// <summary>
        /// 粒子物体销毁或关闭时间
        /// </summary>
		public float hideTime = 1f;
        /// <summary>
        /// 粒子物体对象池
        /// </summary>
		public List<GameObject> objectPool = new List<GameObject>();
        /// <summary>
        /// 粒子位置设置
        /// </summary>
		public SpwanMode spwanMode = SpwanMode.localPosition;
        /// <summary>
        /// 作为粒子挂载的父对象 或 作为粒子生成在此parent对象的位置
        /// </summary>
		public Transform parent = null;
        /// <summary>
        /// 粒子出生位置
        /// </summary>
		public Vector3 postion = new Vector3(0, 1.5f, 2f);
        /// <summary>
        /// 粒子角度
        /// </summary>
        public Vector3 rotation;

        public override void OnAnimationEvent(StateAction action)
        {
            if (effect == null)
                return;
            if (activeMode == ActiveMode.Instantiate)
                Object.Destroy(InstantiateSpwan(), hideTime);
            else if (activeMode == ActiveMode.ObjectPool)
            {
                bool active = false;
                for (int i = 0; i < objectPool.Count; i++)
                {
                    var obj = objectPool[i];
                    if (obj == null)
                    {
                        objectPool.RemoveAt(i);
                        break;
                    }
                    if (!obj.activeSelf)
                    {
                        obj.SetActive(true);
                        obj.transform.SetParent(null);
                        SetPosition(obj);
                        active = true;
                        _ = HideSpwanTime(obj);
                        break;
                    }
                }
                if (!active)
                {
                    var go = InstantiateSpwan();
                    objectPool.Add(go);
                    _ = HideSpwanTime(go);
                }
            }
            else
            {
                effect.SetActive(true);
                SetPosition(effect);
            }
        }

        private async UniTaskVoid HideSpwanTime(GameObject gameObject)
        {
            await UniTask.Delay((int)(hideTime * 1000));
            if (gameObject != null)
                gameObject.SetActive(false);
        }

        /// <summary>
        /// 设置技能位置
        /// </summary>
		private void SetPosition(GameObject go)
        {
            switch (spwanMode)
            {
                case SpwanMode.localPosition:
                    go.transform.localPosition = stateManager.transform.TransformPoint(postion);
                    go.transform.eulerAngles = stateManager.transform.eulerAngles + rotation;
                    break;
                case SpwanMode.SetParent:
                    parent = parent ? parent : stateManager.transform;
                    go.transform.SetParent(parent);
                    go.transform.position = parent.TransformPoint(postion);
                    go.transform.eulerAngles = parent.eulerAngles + rotation;
                    break;
                case SpwanMode.SetInTargetPosition:
                    parent = parent ? parent : stateManager.transform;
                    go.transform.SetParent(parent);
                    go.transform.position = parent.TransformPoint(postion);
                    go.transform.eulerAngles = parent.eulerAngles + rotation;
                    go.transform.SetParent(null);
                    break;
                case SpwanMode.ActiveLocalPosition:
                    go.transform.localPosition = postion;
                    go.transform.localEulerAngles = rotation;
                    break;
            }
        }

        /// <summary>
        /// 技能实例化
        /// </summary>
		private GameObject InstantiateSpwan()
        {
            var go = Object.Instantiate(effect);
            OnSpwanEffect(go);
            SetPosition(go);
            return go;
        }

        /// <summary>
        /// 当技能物体实例化
        /// </summary>
        /// <param name="effect"></param>
        public virtual void OnSpwanEffect(GameObject effect) { }

        public override void OnDestroyComponent()
        {
            foreach (var obj in objectPool)
            {
                Object.Destroy(obj);
            }
        }
    }
}