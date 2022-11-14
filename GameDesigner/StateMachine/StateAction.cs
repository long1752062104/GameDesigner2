using System.Collections.Generic;

namespace GameDesigner
{
    /// <summary>
    /// ARPG状态动作
    /// </summary>
	[System.Serializable]
    public sealed class StateAction
    {
        /// <summary>
        /// 动画剪辑名称
        /// </summary>
		public string clipName = "";
        /// <summary>
        /// 动画剪辑索引
        /// </summary>
		public int clipIndex = 0;
        /// <summary>
        /// 当前动画时间
        /// </summary>
		public float animTime = 0;
        /// <summary>
        /// 动画结束时间
        /// </summary>
		public float animTimeMax = 100;

        /// <summary>
        /// ARPG动作行为
        /// </summary>
		public List<ActionBehaviour> behaviours = new List<ActionBehaviour>();

#if UNITY_EDITOR || DEBUG
        /// <summary>
        /// 编辑器脚本是否展开 编辑器音效是否展开 查找行为组件
        /// </summary>
        public bool foldout = true, audiofoldout = false, findBehaviours = false;
        /// <summary>
        /// 删除音效索引 行为脚本菜单索引
        /// </summary>
		public int desAudioIndex = 0, behaviourMenuIndex = 0;
        /// <summary>
        /// 创建脚本名称
        /// </summary>
        public string createScriptName = "NewStateAction";

        public int acSize = 1;
#endif
        public StateMachine stateMachine;
       
        /// <summary>
        /// 动作是否完成?, 当动画播放结束后为True, 否则为false
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return animTime >= animTimeMax - 1;
            }
        }

        /// <summary>
        /// 构造状态动作
        /// </summary>
        public StateAction()
        {
        }
    }
}