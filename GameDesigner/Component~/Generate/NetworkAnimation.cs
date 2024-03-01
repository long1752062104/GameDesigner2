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
            fields = new object[40];
            fields[1] = self.clip;
            fields[2] = self.playAutomatically;
            fields[3] = self.wrapMode;
            fields[4] = self.animatePhysics;
            fields[5] = self.cullingType;
        }


        public UnityEngine.AnimationClip clip
        {
            get
            {
                return self.clip;
            }
            set
            {
                if (Equals(value, fields[1]))
                    return;
                fields[1] = value;
                self.clip = value;
                AddOperation(1, value);
            }
        }
        public System.Boolean playAutomatically
        {
            get
            {
                return self.playAutomatically;
            }
            set
            {
                if (Equals(value, fields[2]))
                    return;
                fields[2] = value;
                self.playAutomatically = value;
                AddOperation(2, value);
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
                if (Equals(value, fields[3]))
                    return;
                fields[3] = value;
                self.wrapMode = value;
                AddOperation(3, value);
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
                if (Equals(value, fields[4]))
                    return;
                fields[4] = value;
                self.animatePhysics = value;
                AddOperation(4, value);
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
                if (Equals(value, fields[5]))
                    return;
                fields[5] = value;
                self.cullingType = value;
                AddOperation(5, value);
            }
        }
        public override void OnPropertyAutoCheck()
        {
            if (!autoCheck)
                return;

            clip = clip;
            playAutomatically = playAutomatically;
            wrapMode = wrapMode;
            animatePhysics = animatePhysics;
            cullingType = cullingType;
        }

        public void Stop(System.String name, bool always = false)
        {
            if (Equals(name, fields[7]) & !always) return;
            fields[7] = name;
            AddOperation(6, name);
        }
        public void Rewind(System.String name, bool always = false)
        {
            if (Equals(name, fields[9]) & !always) return;
            fields[9] = name;
            AddOperation(8, name);
        }
        public void Play(bool always = false)
        {
            if (!always) return;

            AddOperation(10, null);
        }
        public void Play(UnityEngine.PlayMode mode, bool always = false)
        {
            if (Equals(mode, fields[12]) & !always) return;
            fields[12] = mode;
            AddOperation(11, mode);
        }
        public void Play(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[14]) & !always) return;
            fields[14] = animation;
            AddOperation(13, animation);
        }
        public void CrossFade(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[16]) & !always) return;
            fields[16] = animation;
            AddOperation(15, animation);
        }
        public void CrossFade(System.String animation, System.Single fadeLength, bool always = false)
        {
            if (Equals(animation, fields[18]) & Equals(fadeLength, fields[19]) & !always) return;
            fields[18] = animation;
            fields[19] = fadeLength;
            AddOperation(17, animation, fadeLength);
        }
        public void Blend(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[21]) & !always) return;
            fields[21] = animation;
            AddOperation(20, animation);
        }
        public void Blend(System.String animation, System.Single targetWeight, bool always = false)
        {
            if (Equals(animation, fields[23]) & Equals(targetWeight, fields[24]) & !always) return;
            fields[23] = animation;
            fields[24] = targetWeight;
            AddOperation(22, animation, targetWeight);
        }
        public void CrossFadeQueued(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[26]) & !always) return;
            fields[26] = animation;
            AddOperation(25, animation);
        }
        public void CrossFadeQueued(System.String animation, System.Single fadeLength, bool always = false)
        {
            if (Equals(animation, fields[28]) & Equals(fadeLength, fields[29]) & !always) return;
            fields[28] = animation;
            fields[29] = fadeLength;
            AddOperation(27, animation, fadeLength);
        }
        public void CrossFadeQueued(System.String animation, System.Single fadeLength, UnityEngine.QueueMode queue, bool always = false)
        {
            if (Equals(animation, fields[31]) & Equals(fadeLength, fields[32]) & Equals(queue, fields[33]) & !always) return;
            fields[31] = animation;
            fields[32] = fadeLength;
            fields[33] = queue;
            AddOperation(30, animation, fadeLength, queue);
        }
        public void PlayQueued(System.String animation, bool always = false)
        {
            if (Equals(animation, fields[35]) & !always) return;
            fields[35] = animation;
            AddOperation(34, animation);
        }
        public void PlayQueued(System.String animation, UnityEngine.QueueMode queue, bool always = false)
        {
            if (Equals(animation, fields[37]) & Equals(queue, fields[38]) & !always) return;
            fields[37] = animation;
            fields[38] = queue;
            AddOperation(36, animation, queue);
        }
        public void RemoveClip(System.String clipName, bool always = false)
        {
            if (Equals(clipName, fields[40]) & !always) return;
            fields[40] = clipName;
            AddOperation(39, clipName);
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
                        var clip = DeserializeObject<UnityEngine.AnimationClip>(new Segment(opt.buffer, false));
                        fields[1] = clip;
                        self.clip = clip;
                    }
                    break;
                case 2:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var playAutomatically = DeserializeObject<System.Boolean>(new Segment(opt.buffer, false));
                        fields[2] = playAutomatically;
                        self.playAutomatically = playAutomatically;
                    }
                    break;
                case 3:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var wrapMode = DeserializeObject<UnityEngine.WrapMode>(new Segment(opt.buffer, false));
                        fields[3] = wrapMode;
                        self.wrapMode = wrapMode;
                    }
                    break;
                case 4:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var animatePhysics = DeserializeObject<System.Boolean>(new Segment(opt.buffer, false));
                        fields[4] = animatePhysics;
                        self.animatePhysics = animatePhysics;
                    }
                    break;
                case 5:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var cullingType = DeserializeObject<UnityEngine.AnimationCullingType>(new Segment(opt.buffer, false));
                        fields[5] = cullingType;
                        self.cullingType = cullingType;
                    }
                    break;
                case 6:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[7] = data.Obj);
                        self.Stop(name);
                    }
                    break;
                case 8:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[9] = data.Obj);
                        self.Rewind(name);
                    }
                    break;
                case 10:
                    {
                        self.Play();
                    }
                    break;
                case 11:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var mode = (UnityEngine.PlayMode)(fields[12] = data.Obj);
                        self.Play(mode);
                    }
                    break;
                case 13:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[14] = data.Obj);
                        self.Play(animation);
                    }
                    break;
                case 15:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[16] = data.Obj);
                        self.CrossFade(animation);
                    }
                    break;
                case 17:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[18] = data.Obj);
                        var fadeLength = (System.Single)(fields[19] = data.Obj);
                        self.CrossFade(animation, fadeLength);
                    }
                    break;
                case 20:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[21] = data.Obj);
                        self.Blend(animation);
                    }
                    break;
                case 22:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[23] = data.Obj);
                        var targetWeight = (System.Single)(fields[24] = data.Obj);
                        self.Blend(animation, targetWeight);
                    }
                    break;
                case 25:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[26] = data.Obj);
                        self.CrossFadeQueued(animation);
                    }
                    break;
                case 27:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[28] = data.Obj);
                        var fadeLength = (System.Single)(fields[29] = data.Obj);
                        self.CrossFadeQueued(animation, fadeLength);
                    }
                    break;
                case 30:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[31] = data.Obj);
                        var fadeLength = (System.Single)(fields[32] = data.Obj);
                        var queue = (UnityEngine.QueueMode)(fields[33] = data.Obj);
                        self.CrossFadeQueued(animation, fadeLength, queue);
                    }
                    break;
                case 34:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[35] = data.Obj);
                        self.PlayQueued(animation);
                    }
                    break;
                case 36:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var animation = (System.String)(fields[37] = data.Obj);
                        var queue = (UnityEngine.QueueMode)(fields[38] = data.Obj);
                        self.PlayQueued(animation, queue);
                    }
                    break;
                case 39:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var clipName = (System.String)(fields[40] = data.Obj);
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
                buffer = buffer
            });
        }
    }
}
#endif