#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Client;
using Net.Share;
using Net.Component;
using Net.UnityComponent;
using UnityEngine;
using Net.System;
using static Net.Serialize.NetConvertFast2;

namespace BuildComponent
{
    /// <summary>
    /// Animation同步组件, 此代码由BuildComponentTools工具生成, 如果同步发生相互影响的字段或属性, 请自行检查处理一下!
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Animation))]
    public class NetworkAnimation : NetworkBehaviour
    {
        private UnityEngine.Animation self;
        public bool autoCheck;
        private object[] fields;

        public void Awake()
        {
            self = GetComponent<UnityEngine.Animation>();
            fields = new object[39];
            fields[1] = self.playAutomatically;
            fields[2] = self.wrapMode;
            fields[3] = self.animatePhysics;
            fields[4] = self.cullingType;
        }


        public System.Boolean playAutomatically
        {
            get
            {
                return self.playAutomatically;
            }
            set
            {
                if (Equals(value, fields[1]))
                    return;
                fields[1] = value;
                self.playAutomatically = value;
                AddOperation(1, value);
            }
        }
        public UnityEngine.WrapMode wrapMode
        {
            get
            {
                return self.wrapMode;
            }
            set
            {
                if (Equals(value, fields[2]))
                    return;
                fields[2] = value;
                self.wrapMode = value;
                AddOperation(2, (int)value);
            }
        }
        public System.Boolean animatePhysics
        {
            get
            {
                return self.animatePhysics;
            }
            set
            {
                if (Equals(value, fields[3]))
                    return;
                fields[3] = value;
                self.animatePhysics = value;
                AddOperation(3, value);
            }
        }
        public UnityEngine.AnimationCullingType cullingType
        {
            get
            {
                return self.cullingType;
            }
            set
            {
                if (Equals(value, fields[4]))
                    return;
                fields[4] = value;
                self.cullingType = value;
                AddOperation(4, (int)value);
            }
        }
        public override void OnPropertyAutoCheck()
        {
            if (!autoCheck)
                return;

            playAutomatically = playAutomatically;
            wrapMode = wrapMode;
            animatePhysics = animatePhysics;
            cullingType = cullingType;
        }

        public void Stop(System.String name, bool always = false)
        {
            if (Equals(name, fields[6]) & !always) return;
            fields[6] = name;
            AddOperation(5, name);
        }
        public void Rewind(System.String name, bool always = false)
        {
            if (Equals(name, fields[8]) & !always) return;
            fields[8] = name;
            AddOperation(7, name);
        }
        public void Play(bool always = false)
        {
            if (!always) return;

            AddOperation(9, null);
        }
        public void Play(UnityEngine.PlayMode mode, bool always = false)
        {
            if (Equals(mode, fields[11]) & !always) return;
            fields[11] = mode;
            AddOperation(10, mode);
        }
        public void Play(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[13]) & !always) return;
            fields[13] = animation;
            AddOperation(12, animation);
        }
        public void CrossFade(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[15]) & !always) return;
            fields[15] = animation;
            AddOperation(14, animation);
        }
        public void CrossFade(System.String animation, System.Single fadeLength, bool always = false)
        {
            if (Equals(animation, fields[17]) & Equals(fadeLength, fields[18]) & !always) return;
            fields[17] = animation;
            fields[18] = fadeLength;
            AddOperation(16, animation, fadeLength);
        }
        public void Blend(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[20]) & !always) return;
            fields[20] = animation;
            AddOperation(19, animation);
        }
        public void Blend(System.String animation, System.Single targetWeight, bool always = false)
        {
            if (Equals(animation, fields[22]) & Equals(targetWeight, fields[23]) & !always) return;
            fields[22] = animation;
            fields[23] = targetWeight;
            AddOperation(21, animation, targetWeight);
        }
        public void CrossFadeQueued(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[25]) & !always) return;
            fields[25] = animation;
            AddOperation(24, animation);
        }
        public void CrossFadeQueued(System.String animation, System.Single fadeLength, bool always = false)
        {
            if (Equals(animation, fields[27]) & Equals(fadeLength, fields[28]) & !always) return;
            fields[27] = animation;
            fields[28] = fadeLength;
            AddOperation(26, animation, fadeLength);
        }
        public void CrossFadeQueued(System.String animation, System.Single fadeLength, UnityEngine.QueueMode queue, bool always = false)
        {
            if (Equals(animation, fields[30]) & Equals(fadeLength, fields[31]) & Equals(queue, fields[32]) & !always) return;
            fields[30] = animation;
            fields[31] = fadeLength;
            fields[32] = queue;
            AddOperation(29, animation, fadeLength, queue);
        }
        public void PlayQueued(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[34]) & !always) return;
            fields[34] = animation;
            AddOperation(33, animation);
        }
        public void PlayQueued(System.String animation, UnityEngine.QueueMode queue, bool always = false)
        {
            if (Equals(animation, fields[36]) & Equals(queue, fields[37]) & !always) return;
            fields[36] = animation;
            fields[37] = queue;
            AddOperation(35, animation, queue);
        }
        public void RemoveClip(System.String clipName, bool always = false)
        {
            if (Equals(clipName, fields[39]) & !always) return;
            fields[39] = clipName;
            AddOperation(38, clipName);
        }
        public System.Collections.IEnumerator GetEnumerator()
        {
            return self.GetEnumerator();
        }
        public UnityEngine.AnimationClip GetClip(System.String name)
        {
            return self.GetClip(name);
        }
        public override void OnNetworkOperationHandler(in Operation opt)
        {
            switch (opt.index2)
            {

                case 1:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var playAutomatically = (System.Boolean)data.Obj;
                        fields[1] = playAutomatically;
                        self.playAutomatically = playAutomatically;
                    }
                    break;
                case 2:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var wrapMode = (UnityEngine.WrapMode)data.Obj;
                        fields[2] = wrapMode;
                        self.wrapMode = wrapMode;
                    }
                    break;
                case 3:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animatePhysics = (System.Boolean)data.Obj;
                        fields[3] = animatePhysics;
                        self.animatePhysics = animatePhysics;
                    }
                    break;
                case 4:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var cullingType = (UnityEngine.AnimationCullingType)data.Obj;
                        fields[4] = cullingType;
                        self.cullingType = cullingType;
                    }
                    break;
                case 5:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[6] = data.Obj);
                        self.Stop(name);
                    }
                    break;
                case 7:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[8] = data.Obj);
                        self.Rewind(name);
                    }
                    break;
                case 9:
                    {
                        self.Play();
                    }
                    break;
                case 10:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var mode = (UnityEngine.PlayMode)(fields[11] = data.Obj);
                        self.Play(mode);
                    }
                    break;
                case 12:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[13] = data.Obj);
                        self.Play(animation);
                    }
                    break;
                case 14:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[15] = data.Obj);
                        self.CrossFade(animation);
                    }
                    break;
                case 16:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[17] = data.Obj);
                        var fadeLength = (System.Single)(fields[18] = data.Obj);
                        self.CrossFade(animation, fadeLength);
                    }
                    break;
                case 19:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[20] = data.Obj);
                        self.Blend(animation);
                    }
                    break;
                case 21:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[22] = data.Obj);
                        var targetWeight = (System.Single)(fields[23] = data.Obj);
                        self.Blend(animation, targetWeight);
                    }
                    break;
                case 24:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[25] = data.Obj);
                        self.CrossFadeQueued(animation);
                    }
                    break;
                case 26:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[27] = data.Obj);
                        var fadeLength = (System.Single)(fields[28] = data.Obj);
                        self.CrossFadeQueued(animation, fadeLength);
                    }
                    break;
                case 29:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[30] = data.Obj);
                        var fadeLength = (System.Single)(fields[31] = data.Obj);
                        var queue = (UnityEngine.QueueMode)(fields[32] = data.Obj);
                        self.CrossFadeQueued(animation, fadeLength, queue);
                    }
                    break;
                case 33:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[34] = data.Obj);
                        self.PlayQueued(animation);
                    }
                    break;
                case 35:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[36] = data.Obj);
                        var queue = (UnityEngine.QueueMode)(fields[37] = data.Obj);
                        self.PlayQueued(animation, queue);
                    }
                    break;
                case 38:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var clipName = (System.String)(fields[39] = data.Obj);
                        self.RemoveClip(clipName);
                    }
                    break;
            }
        }

        private void AddOperation(int invokeId, params object[] args)
        {
            if (!IsLocal)
                return;
            if (args == null)
                args = new object[0];
            var buffer = SerializeModel(new RPCModel() { pars = args });
            NetworkSceneManager.Instance.AddOperation(new Operation(Command.BuildComponent, netObj.Identity)
            {
                index = netObj.registerObjectIndex,
                index1 = NetComponentID,
                index2 = invokeId,
                buffer = buffer,
                uid = ClientBase.Instance.UID
            });
        }
    }
}
#endif