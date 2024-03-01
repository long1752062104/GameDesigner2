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
    /// Animator同步组件, 此代码由BuildComponentTools工具生成, 如果同步发生相互影响的字段或属性, 请自行检查处理一下!
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Animator))]
    public class NetworkAnimator : NetworkBehaviour
    {
        private UnityEngine.Animator self;
        public bool autoCheck;
        private object[] fields;

        public void Awake()
        {
            self = GetComponent<UnityEngine.Animator>();
            fields = new object[173];
            fields[1] = self.applyRootMotion;
            fields[2] = self.updateMode;
            fields[3] = self.speed;
            fields[4] = self.cullingMode;
        }


        public System.Boolean applyRootMotion
        {
            get
            {
                return self.applyRootMotion;
            }
            set
            {
                if (Equals(value, fields[1]))
                    return;
                fields[1] = value;
                self.applyRootMotion = value;
                AddOperation(1, value);
            }
        }
        public UnityEngine.AnimatorUpdateMode updateMode
        {
            get
            {
                return self.updateMode;
            }
            set
            {
                if (Equals(value, fields[2]))
                    return;
                fields[2] = value;
                self.updateMode = value;
                AddOperation(2, value);
            }
        }
        public System.Single speed
        {
            get
            {
                return self.speed;
            }
            set
            {
                if (Equals(value, fields[3]))
                    return;
                fields[3] = value;
                self.speed = value;
                AddOperation(3, value);
            }
        }
        public UnityEngine.AnimatorCullingMode cullingMode
        {
            get
            {
                return self.cullingMode;
            }
            set
            {
                if (Equals(value, fields[4]))
                    return;
                fields[4] = value;
                self.cullingMode = value;
                AddOperation(4, value);
            }
        }
        public override void OnPropertyAutoCheck()
        {
            if (!autoCheck)
                return;

            applyRootMotion = applyRootMotion;
            updateMode = updateMode;
            speed = speed;
            cullingMode = cullingMode;
        }

        public System.Single GetFloat(System.String name)
        {
            return self.GetFloat(name);
        }
        public System.Single GetFloat(System.Int32 id)
        {
            return self.GetFloat(id);
        }
        public void SetFloat(System.String name, System.Single value, bool always = false)
        {
            if (Equals(name, fields[6]) & Equals(value, fields[7]) & !always) return;
            fields[6] = name;
            fields[7] = value;
            AddOperation(5, name, value);
        }
        public void SetFloat(System.String name, System.Single value, System.Single dampTime, System.Single deltaTime, bool always = false)
        {
            if (Equals(name, fields[9]) & Equals(value, fields[10]) & Equals(dampTime, fields[11]) & Equals(deltaTime, fields[12]) & !always) return;
            fields[9] = name;
            fields[10] = value;
            fields[11] = dampTime;
            fields[12] = deltaTime;
            AddOperation(8, name, value, dampTime, deltaTime);
        }
        public void SetFloat(System.Int32 id, System.Single value, bool always = false)
        {
            if (Equals(id, fields[14]) & Equals(value, fields[15]) & !always) return;
            fields[14] = id;
            fields[15] = value;
            AddOperation(13, id, value);
        }
        public void SetFloat(System.Int32 id, System.Single value, System.Single dampTime, System.Single deltaTime, bool always = false)
        {
            if (Equals(id, fields[17]) & Equals(value, fields[18]) & Equals(dampTime, fields[19]) & Equals(deltaTime, fields[20]) & !always) return;
            fields[17] = id;
            fields[18] = value;
            fields[19] = dampTime;
            fields[20] = deltaTime;
            AddOperation(16, id, value, dampTime, deltaTime);
        }
        public System.Boolean GetBool(System.String name)
        {
            return self.GetBool(name);
        }
        public System.Boolean GetBool(System.Int32 id)
        {
            return self.GetBool(id);
        }
        public void SetBool(System.String name, System.Boolean value, bool always = false)
        {
            if (Equals(name, fields[22]) & Equals(value, fields[23]) & !always) return;
            fields[22] = name;
            fields[23] = value;
            AddOperation(21, name, value);
        }
        public void SetBool(System.Int32 id, System.Boolean value, bool always = false)
        {
            if (Equals(id, fields[25]) & Equals(value, fields[26]) & !always) return;
            fields[25] = id;
            fields[26] = value;
            AddOperation(24, id, value);
        }
        public System.Int32 GetInteger(System.String name)
        {
            return self.GetInteger(name);
        }
        public System.Int32 GetInteger(System.Int32 id)
        {
            return self.GetInteger(id);
        }
        public void SetInteger(System.String name, System.Int32 value, bool always = false)
        {
            if (Equals(name, fields[28]) & Equals(value, fields[29]) & !always) return;
            fields[28] = name;
            fields[29] = value;
            AddOperation(27, name, value);
        }
        public void SetInteger(System.Int32 id, System.Int32 value, bool always = false)
        {
            if (Equals(id, fields[31]) & Equals(value, fields[32]) & !always) return;
            fields[31] = id;
            fields[32] = value;
            AddOperation(30, id, value);
        }
        public void SetTrigger(System.String name, bool always = false)
        {
            if (Equals(name, fields[34]) & !always) return;
            fields[34] = name;
            AddOperation(33, name);
        }
        public void SetTrigger(System.Int32 id, bool always = false)
        {
            if (Equals(id, fields[36]) & !always) return;
            fields[36] = id;
            AddOperation(35, id);
        }
        public void ResetTrigger(System.String name, bool always = false)
        {
            if (Equals(name, fields[38]) & !always) return;
            fields[38] = name;
            AddOperation(37, name);
        }
        public void ResetTrigger(System.Int32 id, bool always = false)
        {
            if (Equals(id, fields[40]) & !always) return;
            fields[40] = id;
            AddOperation(39, id);
        }
        public System.Boolean IsParameterControlledByCurve(System.String name)
        {
            return self.IsParameterControlledByCurve(name);
        }
        public System.Boolean IsParameterControlledByCurve(System.Int32 id)
        {
            return self.IsParameterControlledByCurve(id);
        }
        public UnityEngine.Vector3 GetIKPosition(UnityEngine.AvatarIKGoal goal)
        {
            return self.GetIKPosition(goal);
        }
        public void SetIKPosition(UnityEngine.AvatarIKGoal goal, UnityEngine.Vector3 goalPosition, bool always = false)
        {
            if (Equals(goal, fields[42]) & Equals(goalPosition, fields[43]) & !always) return;
            fields[42] = goal;
            fields[43] = goalPosition;
            AddOperation(41, goal, goalPosition);
        }
        public UnityEngine.Quaternion GetIKRotation(UnityEngine.AvatarIKGoal goal)
        {
            return self.GetIKRotation(goal);
        }
        public void SetIKRotation(UnityEngine.AvatarIKGoal goal, UnityEngine.Quaternion goalRotation, bool always = false)
        {
            if (Equals(goal, fields[45]) & Equals(goalRotation, fields[46]) & !always) return;
            fields[45] = goal;
            fields[46] = goalRotation;
            AddOperation(44, goal, goalRotation);
        }
        public System.Single GetIKPositionWeight(UnityEngine.AvatarIKGoal goal)
        {
            return self.GetIKPositionWeight(goal);
        }
        public void SetIKPositionWeight(UnityEngine.AvatarIKGoal goal, System.Single value, bool always = false)
        {
            if (Equals(goal, fields[48]) & Equals(value, fields[49]) & !always) return;
            fields[48] = goal;
            fields[49] = value;
            AddOperation(47, goal, value);
        }
        public System.Single GetIKRotationWeight(UnityEngine.AvatarIKGoal goal)
        {
            return self.GetIKRotationWeight(goal);
        }
        public void SetIKRotationWeight(UnityEngine.AvatarIKGoal goal, System.Single value, bool always = false)
        {
            if (Equals(goal, fields[51]) & Equals(value, fields[52]) & !always) return;
            fields[51] = goal;
            fields[52] = value;
            AddOperation(50, goal, value);
        }
        public UnityEngine.Vector3 GetIKHintPosition(UnityEngine.AvatarIKHint hint)
        {
            return self.GetIKHintPosition(hint);
        }
        public void SetIKHintPosition(UnityEngine.AvatarIKHint hint, UnityEngine.Vector3 hintPosition, bool always = false)
        {
            if (Equals(hint, fields[54]) & Equals(hintPosition, fields[55]) & !always) return;
            fields[54] = hint;
            fields[55] = hintPosition;
            AddOperation(53, hint, hintPosition);
        }
        public System.Single GetIKHintPositionWeight(UnityEngine.AvatarIKHint hint)
        {
            return self.GetIKHintPositionWeight(hint);
        }
        public void SetIKHintPositionWeight(UnityEngine.AvatarIKHint hint, System.Single value, bool always = false)
        {
            if (Equals(hint, fields[57]) & Equals(value, fields[58]) & !always) return;
            fields[57] = hint;
            fields[58] = value;
            AddOperation(56, hint, value);
        }
        public void SetLookAtPosition(UnityEngine.Vector3 lookAtPosition, bool always = false)
        {
            if (Equals(lookAtPosition, fields[60]) & !always) return;
            fields[60] = lookAtPosition;
            AddOperation(59, lookAtPosition);
        }
        public void SetLookAtWeight(System.Single weight, bool always = false)
        {
            if (Equals(weight, fields[62]) & !always) return;
            fields[62] = weight;
            AddOperation(61, weight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, bool always = false)
        {
            if (Equals(weight, fields[64]) & Equals(bodyWeight, fields[65]) & !always) return;
            fields[64] = weight;
            fields[65] = bodyWeight;
            AddOperation(63, weight, bodyWeight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, System.Single headWeight, bool always = false)
        {
            if (Equals(weight, fields[67]) & Equals(bodyWeight, fields[68]) & Equals(headWeight, fields[69]) & !always) return;
            fields[67] = weight;
            fields[68] = bodyWeight;
            fields[69] = headWeight;
            AddOperation(66, weight, bodyWeight, headWeight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, System.Single headWeight, System.Single eyesWeight, bool always = false)
        {
            if (Equals(weight, fields[71]) & Equals(bodyWeight, fields[72]) & Equals(headWeight, fields[73]) & Equals(eyesWeight, fields[74]) & !always) return;
            fields[71] = weight;
            fields[72] = bodyWeight;
            fields[73] = headWeight;
            fields[74] = eyesWeight;
            AddOperation(70, weight, bodyWeight, headWeight, eyesWeight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, System.Single headWeight, System.Single eyesWeight, System.Single clampWeight, bool always = false)
        {
            if (Equals(weight, fields[76]) & Equals(bodyWeight, fields[77]) & Equals(headWeight, fields[78]) & Equals(eyesWeight, fields[79]) & Equals(clampWeight, fields[80]) & !always) return;
            fields[76] = weight;
            fields[77] = bodyWeight;
            fields[78] = headWeight;
            fields[79] = eyesWeight;
            fields[80] = clampWeight;
            AddOperation(75, weight, bodyWeight, headWeight, eyesWeight, clampWeight);
        }
        public void SetBoneLocalRotation(UnityEngine.HumanBodyBones humanBoneId, UnityEngine.Quaternion rotation, bool always = false)
        {
            if (Equals(humanBoneId, fields[82]) & Equals(rotation, fields[83]) & !always) return;
            fields[82] = humanBoneId;
            fields[83] = rotation;
            AddOperation(81, humanBoneId, rotation);
        }
        public UnityEngine.StateMachineBehaviour[] GetBehaviours(System.Int32 fullPathHash, System.Int32 layerIndex)
        {
            return self.GetBehaviours(fullPathHash, layerIndex);
        }
        public UnityEngine.AnimatorStateInfo GetCurrentAnimatorStateInfo(System.Int32 layerIndex)
        {
            return self.GetCurrentAnimatorStateInfo(layerIndex);
        }
        public UnityEngine.AnimatorStateInfo GetNextAnimatorStateInfo(System.Int32 layerIndex)
        {
            return self.GetNextAnimatorStateInfo(layerIndex);
        }
        public UnityEngine.AnimatorTransitionInfo GetAnimatorTransitionInfo(System.Int32 layerIndex)
        {
            return self.GetAnimatorTransitionInfo(layerIndex);
        }
        public System.Int32 GetCurrentAnimatorClipInfoCount(System.Int32 layerIndex)
        {
            return self.GetCurrentAnimatorClipInfoCount(layerIndex);
        }
        public System.Int32 GetNextAnimatorClipInfoCount(System.Int32 layerIndex)
        {
            return self.GetNextAnimatorClipInfoCount(layerIndex);
        }
        public UnityEngine.AnimatorControllerParameter GetParameter(System.Int32 index)
        {
            return self.GetParameter(index);
        }
        public void InterruptMatchTarget(bool always = false)
        {
            if (!always) return;

            AddOperation(84, null);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, bool always = false)
        {
            if (Equals(stateName, fields[86]) & Equals(fixedTransitionDuration, fields[87]) & !always) return;
            fields[86] = stateName;
            fields[87] = fixedTransitionDuration;
            AddOperation(85, stateName, fixedTransitionDuration);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[89]) & Equals(fixedTransitionDuration, fields[90]) & Equals(layer, fields[91]) & !always) return;
            fields[89] = stateName;
            fields[90] = fixedTransitionDuration;
            fields[91] = layer;
            AddOperation(88, stateName, fixedTransitionDuration, layer);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, System.Int32 layer, System.Single fixedTimeOffset, bool always = false)
        {
            if (Equals(stateName, fields[93]) & Equals(fixedTransitionDuration, fields[94]) & Equals(layer, fields[95]) & Equals(fixedTimeOffset, fields[96]) & !always) return;
            fields[93] = stateName;
            fields[94] = fixedTransitionDuration;
            fields[95] = layer;
            fields[96] = fixedTimeOffset;
            AddOperation(92, stateName, fixedTransitionDuration, layer, fixedTimeOffset);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, System.Int32 layer, System.Single fixedTimeOffset, System.Single normalizedTransitionTime, bool always = false)
        {
            if (Equals(stateName, fields[98]) & Equals(fixedTransitionDuration, fields[99]) & Equals(layer, fields[100]) & Equals(fixedTimeOffset, fields[101]) & Equals(normalizedTransitionTime, fields[102]) & !always) return;
            fields[98] = stateName;
            fields[99] = fixedTransitionDuration;
            fields[100] = layer;
            fields[101] = fixedTimeOffset;
            fields[102] = normalizedTransitionTime;
            AddOperation(97, stateName, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
        }
        public void CrossFadeInFixedTime(System.Int32 stateHashName, System.Single fixedTransitionDuration, System.Int32 layer, System.Single fixedTimeOffset, bool always = false)
        {
            if (Equals(stateHashName, fields[104]) & Equals(fixedTransitionDuration, fields[105]) & Equals(layer, fields[106]) & Equals(fixedTimeOffset, fields[107]) & !always) return;
            fields[104] = stateHashName;
            fields[105] = fixedTransitionDuration;
            fields[106] = layer;
            fields[107] = fixedTimeOffset;
            AddOperation(103, stateHashName, fixedTransitionDuration, layer, fixedTimeOffset);
        }
        public void CrossFadeInFixedTime(System.Int32 stateHashName, System.Single fixedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateHashName, fields[109]) & Equals(fixedTransitionDuration, fields[110]) & Equals(layer, fields[111]) & !always) return;
            fields[109] = stateHashName;
            fields[110] = fixedTransitionDuration;
            fields[111] = layer;
            AddOperation(108, stateHashName, fixedTransitionDuration, layer);
        }
        public void CrossFadeInFixedTime(System.Int32 stateHashName, System.Single fixedTransitionDuration, bool always = false)
        {
            if (Equals(stateHashName, fields[113]) & Equals(fixedTransitionDuration, fields[114]) & !always) return;
            fields[113] = stateHashName;
            fields[114] = fixedTransitionDuration;
            AddOperation(112, stateHashName, fixedTransitionDuration);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, System.Int32 layer, System.Single normalizedTimeOffset, bool always = false)
        {
            if (Equals(stateName, fields[116]) & Equals(normalizedTransitionDuration, fields[117]) & Equals(layer, fields[118]) & Equals(normalizedTimeOffset, fields[119]) & !always) return;
            fields[116] = stateName;
            fields[117] = normalizedTransitionDuration;
            fields[118] = layer;
            fields[119] = normalizedTimeOffset;
            AddOperation(115, stateName, normalizedTransitionDuration, layer, normalizedTimeOffset);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[121]) & Equals(normalizedTransitionDuration, fields[122]) & Equals(layer, fields[123]) & !always) return;
            fields[121] = stateName;
            fields[122] = normalizedTransitionDuration;
            fields[123] = layer;
            AddOperation(120, stateName, normalizedTransitionDuration, layer);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, bool always = false)
        {
            if (Equals(stateName, fields[125]) & Equals(normalizedTransitionDuration, fields[126]) & !always) return;
            fields[125] = stateName;
            fields[126] = normalizedTransitionDuration;
            AddOperation(124, stateName, normalizedTransitionDuration);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, System.Int32 layer, System.Single normalizedTimeOffset, System.Single normalizedTransitionTime, bool always = false)
        {
            if (Equals(stateName, fields[128]) & Equals(normalizedTransitionDuration, fields[129]) & Equals(layer, fields[130]) & Equals(normalizedTimeOffset, fields[131]) & Equals(normalizedTransitionTime, fields[132]) & !always) return;
            fields[128] = stateName;
            fields[129] = normalizedTransitionDuration;
            fields[130] = layer;
            fields[131] = normalizedTimeOffset;
            fields[132] = normalizedTransitionTime;
            AddOperation(127, stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
        }
        public void CrossFade(System.Int32 stateHashName, System.Single normalizedTransitionDuration, System.Int32 layer, System.Single normalizedTimeOffset, bool always = false)
        {
            if (Equals(stateHashName, fields[134]) & Equals(normalizedTransitionDuration, fields[135]) & Equals(layer, fields[136]) & Equals(normalizedTimeOffset, fields[137]) & !always) return;
            fields[134] = stateHashName;
            fields[135] = normalizedTransitionDuration;
            fields[136] = layer;
            fields[137] = normalizedTimeOffset;
            AddOperation(133, stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset);
        }
        public void CrossFade(System.Int32 stateHashName, System.Single normalizedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateHashName, fields[139]) & Equals(normalizedTransitionDuration, fields[140]) & Equals(layer, fields[141]) & !always) return;
            fields[139] = stateHashName;
            fields[140] = normalizedTransitionDuration;
            fields[141] = layer;
            AddOperation(138, stateHashName, normalizedTransitionDuration, layer);
        }
        public void CrossFade(System.Int32 stateHashName, System.Single normalizedTransitionDuration, bool always = false)
        {
            if (Equals(stateHashName, fields[143]) & Equals(normalizedTransitionDuration, fields[144]) & !always) return;
            fields[143] = stateHashName;
            fields[144] = normalizedTransitionDuration;
            AddOperation(142, stateHashName, normalizedTransitionDuration);
        }
        public void PlayInFixedTime(System.String stateName, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[146]) & Equals(layer, fields[147]) & !always) return;
            fields[146] = stateName;
            fields[147] = layer;
            AddOperation(145, stateName, layer);
        }
        public void PlayInFixedTime(System.String stateName, bool always = false)
        {
            if (Equals(stateName, fields[149]) & !always) return;
            fields[149] = stateName;
            AddOperation(148, stateName);
        }
        public void PlayInFixedTime(System.String stateName, System.Int32 layer, System.Single fixedTime, bool always = false)
        {
            if (Equals(stateName, fields[151]) & Equals(layer, fields[152]) & Equals(fixedTime, fields[153]) & !always) return;
            fields[151] = stateName;
            fields[152] = layer;
            fields[153] = fixedTime;
            AddOperation(150, stateName, layer, fixedTime);
        }
        public void PlayInFixedTime(System.Int32 stateNameHash, System.Int32 layer, bool always = false)
        {
            if (Equals(stateNameHash, fields[155]) & Equals(layer, fields[156]) & !always) return;
            fields[155] = stateNameHash;
            fields[156] = layer;
            AddOperation(154, stateNameHash, layer);
        }
        public void PlayInFixedTime(System.Int32 stateNameHash, bool always = false)
        {
            if (Equals(stateNameHash, fields[158]) & !always) return;
            fields[158] = stateNameHash;
            AddOperation(157, stateNameHash);
        }
        public void Play(System.String stateName, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[160]) & Equals(layer, fields[161]) & !always) return;
            fields[160] = stateName;
            fields[161] = layer;
            AddOperation(159, stateName, layer);
        }
        public void Play(System.String stateName, bool always = false)
        {
            if (Equals(stateName, fields[163]) & !always) return;
            fields[163] = stateName;
            AddOperation(162, stateName);
        }
        public void Play(System.String stateName, System.Int32 layer, System.Single normalizedTime, bool always = false)
        {
            if (Equals(stateName, fields[165]) & Equals(layer, fields[166]) & Equals(normalizedTime, fields[167]) & !always) return;
            fields[165] = stateName;
            fields[166] = layer;
            fields[167] = normalizedTime;
            AddOperation(164, stateName, layer, normalizedTime);
        }
        public void Play(System.Int32 stateNameHash, System.Int32 layer, bool always = false)
        {
            if (Equals(stateNameHash, fields[169]) & Equals(layer, fields[170]) & !always) return;
            fields[169] = stateNameHash;
            fields[170] = layer;
            AddOperation(168, stateNameHash, layer);
        }
        public void Play(System.Int32 stateNameHash, bool always = false)
        {
            if (Equals(stateNameHash, fields[172]) & !always) return;
            fields[172] = stateNameHash;
            AddOperation(171, stateNameHash);
        }
        public UnityEngine.Transform GetBoneTransform(UnityEngine.HumanBodyBones humanBoneId)
        {
            return self.GetBoneTransform(humanBoneId);
        }
        public void Rebind(bool always = false)
        {
            if (!always) return;

            AddOperation(173, null);
        }
        public override void OnNetworkOperationHandler(in Operation opt)
        {
            switch (opt.index2)
            {

                case 1:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var applyRootMotion = DeserializeObject<System.Boolean>(new Segment(opt.buffer, false));
                        fields[1] = applyRootMotion;
                        self.applyRootMotion = applyRootMotion;
                    }
                    break;
                case 2:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var updateMode = DeserializeObject<UnityEngine.AnimatorUpdateMode>(new Segment(opt.buffer, false));
                        fields[2] = updateMode;
                        self.updateMode = updateMode;
                    }
                    break;
                case 3:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var speed = DeserializeObject<System.Single>(new Segment(opt.buffer, false));
                        fields[3] = speed;
                        self.speed = speed;
                    }
                    break;
                case 4:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var cullingMode = DeserializeObject<UnityEngine.AnimatorCullingMode>(new Segment(opt.buffer, false));
                        fields[4] = cullingMode;
                        self.cullingMode = cullingMode;
                    }
                    break;
                case 5:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[6] = data.Obj);
                        var value = (System.Single)(fields[7] = data.Obj);
                        self.SetFloat(name, value);
                    }
                    break;
                case 8:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[9] = data.Obj);
                        var value = (System.Single)(fields[10] = data.Obj);
                        var dampTime = (System.Single)(fields[11] = data.Obj);
                        var deltaTime = (System.Single)(fields[12] = data.Obj);
                        self.SetFloat(name, value, dampTime, deltaTime);
                    }
                    break;
                case 13:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[14] = data.Obj);
                        var value = (System.Single)(fields[15] = data.Obj);
                        self.SetFloat(id, value);
                    }
                    break;
                case 16:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[17] = data.Obj);
                        var value = (System.Single)(fields[18] = data.Obj);
                        var dampTime = (System.Single)(fields[19] = data.Obj);
                        var deltaTime = (System.Single)(fields[20] = data.Obj);
                        self.SetFloat(id, value, dampTime, deltaTime);
                    }
                    break;
                case 21:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[22] = data.Obj);
                        var value = (System.Boolean)(fields[23] = data.Obj);
                        self.SetBool(name, value);
                    }
                    break;
                case 24:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[25] = data.Obj);
                        var value = (System.Boolean)(fields[26] = data.Obj);
                        self.SetBool(id, value);
                    }
                    break;
                case 27:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[28] = data.Obj);
                        var value = (System.Int32)(fields[29] = data.Obj);
                        self.SetInteger(name, value);
                    }
                    break;
                case 30:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[31] = data.Obj);
                        var value = (System.Int32)(fields[32] = data.Obj);
                        self.SetInteger(id, value);
                    }
                    break;
                case 33:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[34] = data.Obj);
                        self.SetTrigger(name);
                    }
                    break;
                case 35:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[36] = data.Obj);
                        self.SetTrigger(id);
                    }
                    break;
                case 37:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[38] = data.Obj);
                        self.ResetTrigger(name);
                    }
                    break;
                case 39:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[40] = data.Obj);
                        self.ResetTrigger(id);
                    }
                    break;
                case 41:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[42] = data.Obj);
                        var goalPosition = (UnityEngine.Vector3)(fields[43] = data.Obj);
                        self.SetIKPosition(goal, goalPosition);
                    }
                    break;
                case 44:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[45] = data.Obj);
                        var goalRotation = (UnityEngine.Quaternion)(fields[46] = data.Obj);
                        self.SetIKRotation(goal, goalRotation);
                    }
                    break;
                case 47:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[48] = data.Obj);
                        var value = (System.Single)(fields[49] = data.Obj);
                        self.SetIKPositionWeight(goal, value);
                    }
                    break;
                case 50:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[51] = data.Obj);
                        var value = (System.Single)(fields[52] = data.Obj);
                        self.SetIKRotationWeight(goal, value);
                    }
                    break;
                case 53:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var hint = (UnityEngine.AvatarIKHint)(fields[54] = data.Obj);
                        var hintPosition = (UnityEngine.Vector3)(fields[55] = data.Obj);
                        self.SetIKHintPosition(hint, hintPosition);
                    }
                    break;
                case 56:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var hint = (UnityEngine.AvatarIKHint)(fields[57] = data.Obj);
                        var value = (System.Single)(fields[58] = data.Obj);
                        self.SetIKHintPositionWeight(hint, value);
                    }
                    break;
                case 59:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var lookAtPosition = (UnityEngine.Vector3)(fields[60] = data.Obj);
                        self.SetLookAtPosition(lookAtPosition);
                    }
                    break;
                case 61:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[62] = data.Obj);
                        self.SetLookAtWeight(weight);
                    }
                    break;
                case 63:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[64] = data.Obj);
                        var bodyWeight = (System.Single)(fields[65] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight);
                    }
                    break;
                case 66:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[67] = data.Obj);
                        var bodyWeight = (System.Single)(fields[68] = data.Obj);
                        var headWeight = (System.Single)(fields[69] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight, headWeight);
                    }
                    break;
                case 70:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[71] = data.Obj);
                        var bodyWeight = (System.Single)(fields[72] = data.Obj);
                        var headWeight = (System.Single)(fields[73] = data.Obj);
                        var eyesWeight = (System.Single)(fields[74] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight);
                    }
                    break;
                case 75:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[76] = data.Obj);
                        var bodyWeight = (System.Single)(fields[77] = data.Obj);
                        var headWeight = (System.Single)(fields[78] = data.Obj);
                        var eyesWeight = (System.Single)(fields[79] = data.Obj);
                        var clampWeight = (System.Single)(fields[80] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
                    }
                    break;
                case 81:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var humanBoneId = (UnityEngine.HumanBodyBones)(fields[82] = data.Obj);
                        var rotation = (UnityEngine.Quaternion)(fields[83] = data.Obj);
                        self.SetBoneLocalRotation(humanBoneId, rotation);
                    }
                    break;
                case 84:
                    {
                        self.InterruptMatchTarget();
                    }
                    break;
                case 85:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[86] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[87] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
                    }
                    break;
                case 88:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[89] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[90] = data.Obj);
                        var layer = (System.Int32)(fields[91] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration, layer);
                    }
                    break;
                case 92:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[93] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[94] = data.Obj);
                        var layer = (System.Int32)(fields[95] = data.Obj);
                        var fixedTimeOffset = (System.Single)(fields[96] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration, layer, fixedTimeOffset);
                    }
                    break;
                case 97:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[98] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[99] = data.Obj);
                        var layer = (System.Int32)(fields[100] = data.Obj);
                        var fixedTimeOffset = (System.Single)(fields[101] = data.Obj);
                        var normalizedTransitionTime = (System.Single)(fields[102] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
                    }
                    break;
                case 103:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[104] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[105] = data.Obj);
                        var layer = (System.Int32)(fields[106] = data.Obj);
                        var fixedTimeOffset = (System.Single)(fields[107] = data.Obj);
                        self.CrossFadeInFixedTime(stateHashName, fixedTransitionDuration, layer, fixedTimeOffset);
                    }
                    break;
                case 108:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[109] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[110] = data.Obj);
                        var layer = (System.Int32)(fields[111] = data.Obj);
                        self.CrossFadeInFixedTime(stateHashName, fixedTransitionDuration, layer);
                    }
                    break;
                case 112:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[113] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[114] = data.Obj);
                        self.CrossFadeInFixedTime(stateHashName, fixedTransitionDuration);
                    }
                    break;
                case 115:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[116] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[117] = data.Obj);
                        var layer = (System.Int32)(fields[118] = data.Obj);
                        var normalizedTimeOffset = (System.Single)(fields[119] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset);
                    }
                    break;
                case 120:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[121] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[122] = data.Obj);
                        var layer = (System.Int32)(fields[123] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration, layer);
                    }
                    break;
                case 124:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[125] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[126] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration);
                    }
                    break;
                case 127:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[128] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[129] = data.Obj);
                        var layer = (System.Int32)(fields[130] = data.Obj);
                        var normalizedTimeOffset = (System.Single)(fields[131] = data.Obj);
                        var normalizedTransitionTime = (System.Single)(fields[132] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
                    }
                    break;
                case 133:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[134] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[135] = data.Obj);
                        var layer = (System.Int32)(fields[136] = data.Obj);
                        var normalizedTimeOffset = (System.Single)(fields[137] = data.Obj);
                        self.CrossFade(stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset);
                    }
                    break;
                case 138:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[139] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[140] = data.Obj);
                        var layer = (System.Int32)(fields[141] = data.Obj);
                        self.CrossFade(stateHashName, normalizedTransitionDuration, layer);
                    }
                    break;
                case 142:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[143] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[144] = data.Obj);
                        self.CrossFade(stateHashName, normalizedTransitionDuration);
                    }
                    break;
                case 145:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[146] = data.Obj);
                        var layer = (System.Int32)(fields[147] = data.Obj);
                        self.PlayInFixedTime(stateName, layer);
                    }
                    break;
                case 148:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[149] = data.Obj);
                        self.PlayInFixedTime(stateName);
                    }
                    break;
                case 150:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[151] = data.Obj);
                        var layer = (System.Int32)(fields[152] = data.Obj);
                        var fixedTime = (System.Single)(fields[153] = data.Obj);
                        self.PlayInFixedTime(stateName, layer, fixedTime);
                    }
                    break;
                case 154:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[155] = data.Obj);
                        var layer = (System.Int32)(fields[156] = data.Obj);
                        self.PlayInFixedTime(stateNameHash, layer);
                    }
                    break;
                case 157:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[158] = data.Obj);
                        self.PlayInFixedTime(stateNameHash);
                    }
                    break;
                case 159:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[160] = data.Obj);
                        var layer = (System.Int32)(fields[161] = data.Obj);
                        self.Play(stateName, layer);
                    }
                    break;
                case 162:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[163] = data.Obj);
                        self.Play(stateName);
                    }
                    break;
                case 164:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[165] = data.Obj);
                        var layer = (System.Int32)(fields[166] = data.Obj);
                        var normalizedTime = (System.Single)(fields[167] = data.Obj);
                        self.Play(stateName, layer, normalizedTime);
                    }
                    break;
                case 168:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[169] = data.Obj);
                        var layer = (System.Int32)(fields[170] = data.Obj);
                        self.Play(stateNameHash, layer);
                    }
                    break;
                case 171:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[172] = data.Obj);
                        self.Play(stateNameHash);
                    }
                    break;
                case 173:
                    {
                        self.Rebind();
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