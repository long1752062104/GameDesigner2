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
            fields = new object[176];
            fields[1] = self.applyRootMotion;
            fields[2] = self.updateMode;
            fields[3] = self.stabilizeFeet;
            fields[4] = self.speed;
            fields[5] = self.cullingMode;
            fields[6] = self.keepAnimatorStateOnDisable;
            fields[7] = self.writeDefaultValuesOnDisable;
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
        public System.Boolean stabilizeFeet
        {
            get
            {
                return self.stabilizeFeet;
            }
            set
            {
                if (Equals(value, fields[3]))
                    return;
                fields[3] = value;
                self.stabilizeFeet = value;
                AddOperation(3, value);
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
                if (Equals(value, fields[4]))
                    return;
                fields[4] = value;
                self.speed = value;
                AddOperation(4, value);
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
                if (Equals(value, fields[5]))
                    return;
                fields[5] = value;
                self.cullingMode = value;
                AddOperation(5, value);
            }
        }
        public System.Boolean keepAnimatorStateOnDisable
        {
            get
            {
                return self.keepAnimatorStateOnDisable;
            }
            set
            {
                if (Equals(value, fields[6]))
                    return;
                fields[6] = value;
                self.keepAnimatorStateOnDisable = value;
                AddOperation(6, value);
            }
        }
        public System.Boolean writeDefaultValuesOnDisable
        {
            get
            {
                return self.writeDefaultValuesOnDisable;
            }
            set
            {
                if (Equals(value, fields[7]))
                    return;
                fields[7] = value;
                self.writeDefaultValuesOnDisable = value;
                AddOperation(7, value);
            }
        }
        public override void OnPropertyAutoCheck()
        {
            if (!autoCheck)
                return;

            applyRootMotion = applyRootMotion;
            updateMode = updateMode;
            stabilizeFeet = stabilizeFeet;
            speed = speed;
            cullingMode = cullingMode;
            keepAnimatorStateOnDisable = keepAnimatorStateOnDisable;
            writeDefaultValuesOnDisable = writeDefaultValuesOnDisable;
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
            if (Equals(name, fields[9]) & Equals(value, fields[10]) & !always) return;
            fields[9] = name;
            fields[10] = value;
            AddOperation(8, name, value);
        }
        public void SetFloat(System.String name, System.Single value, System.Single dampTime, System.Single deltaTime, bool always = false)
        {
            if (Equals(name, fields[12]) & Equals(value, fields[13]) & Equals(dampTime, fields[14]) & Equals(deltaTime, fields[15]) & !always) return;
            fields[12] = name;
            fields[13] = value;
            fields[14] = dampTime;
            fields[15] = deltaTime;
            AddOperation(11, name, value, dampTime, deltaTime);
        }
        public void SetFloat(System.Int32 id, System.Single value, bool always = false)
        {
            if (Equals(id, fields[17]) & Equals(value, fields[18]) & !always) return;
            fields[17] = id;
            fields[18] = value;
            AddOperation(16, id, value);
        }
        public void SetFloat(System.Int32 id, System.Single value, System.Single dampTime, System.Single deltaTime, bool always = false)
        {
            if (Equals(id, fields[20]) & Equals(value, fields[21]) & Equals(dampTime, fields[22]) & Equals(deltaTime, fields[23]) & !always) return;
            fields[20] = id;
            fields[21] = value;
            fields[22] = dampTime;
            fields[23] = deltaTime;
            AddOperation(19, id, value, dampTime, deltaTime);
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
            if (Equals(name, fields[25]) & Equals(value, fields[26]) & !always) return;
            fields[25] = name;
            fields[26] = value;
            AddOperation(24, name, value);
        }
        public void SetBool(System.Int32 id, System.Boolean value, bool always = false)
        {
            if (Equals(id, fields[28]) & Equals(value, fields[29]) & !always) return;
            fields[28] = id;
            fields[29] = value;
            AddOperation(27, id, value);
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
            if (Equals(name, fields[31]) & Equals(value, fields[32]) & !always) return;
            fields[31] = name;
            fields[32] = value;
            AddOperation(30, name, value);
        }
        public void SetInteger(System.Int32 id, System.Int32 value, bool always = false)
        {
            if (Equals(id, fields[34]) & Equals(value, fields[35]) & !always) return;
            fields[34] = id;
            fields[35] = value;
            AddOperation(33, id, value);
        }
        public void SetTrigger(System.String name, bool always = false)
        {
            if (Equals(name, fields[37]) & !always) return;
            fields[37] = name;
            AddOperation(36, name);
        }
        public void SetTrigger(System.Int32 id, bool always = false)
        {
            if (Equals(id, fields[39]) & !always) return;
            fields[39] = id;
            AddOperation(38, id);
        }
        public void ResetTrigger(System.String name, bool always = false)
        {
            if (Equals(name, fields[41]) & !always) return;
            fields[41] = name;
            AddOperation(40, name);
        }
        public void ResetTrigger(System.Int32 id, bool always = false)
        {
            if (Equals(id, fields[43]) & !always) return;
            fields[43] = id;
            AddOperation(42, id);
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
            if (Equals(goal, fields[45]) & Equals(goalPosition, fields[46]) & !always) return;
            fields[45] = goal;
            fields[46] = goalPosition;
            AddOperation(44, goal, goalPosition);
        }
        public UnityEngine.Quaternion GetIKRotation(UnityEngine.AvatarIKGoal goal)
        {
            return self.GetIKRotation(goal);
        }
        public void SetIKRotation(UnityEngine.AvatarIKGoal goal, UnityEngine.Quaternion goalRotation, bool always = false)
        {
            if (Equals(goal, fields[48]) & Equals(goalRotation, fields[49]) & !always) return;
            fields[48] = goal;
            fields[49] = goalRotation;
            AddOperation(47, goal, goalRotation);
        }
        public System.Single GetIKPositionWeight(UnityEngine.AvatarIKGoal goal)
        {
            return self.GetIKPositionWeight(goal);
        }
        public void SetIKPositionWeight(UnityEngine.AvatarIKGoal goal, System.Single value, bool always = false)
        {
            if (Equals(goal, fields[51]) & Equals(value, fields[52]) & !always) return;
            fields[51] = goal;
            fields[52] = value;
            AddOperation(50, goal, value);
        }
        public System.Single GetIKRotationWeight(UnityEngine.AvatarIKGoal goal)
        {
            return self.GetIKRotationWeight(goal);
        }
        public void SetIKRotationWeight(UnityEngine.AvatarIKGoal goal, System.Single value, bool always = false)
        {
            if (Equals(goal, fields[54]) & Equals(value, fields[55]) & !always) return;
            fields[54] = goal;
            fields[55] = value;
            AddOperation(53, goal, value);
        }
        public UnityEngine.Vector3 GetIKHintPosition(UnityEngine.AvatarIKHint hint)
        {
            return self.GetIKHintPosition(hint);
        }
        public void SetIKHintPosition(UnityEngine.AvatarIKHint hint, UnityEngine.Vector3 hintPosition, bool always = false)
        {
            if (Equals(hint, fields[57]) & Equals(hintPosition, fields[58]) & !always) return;
            fields[57] = hint;
            fields[58] = hintPosition;
            AddOperation(56, hint, hintPosition);
        }
        public System.Single GetIKHintPositionWeight(UnityEngine.AvatarIKHint hint)
        {
            return self.GetIKHintPositionWeight(hint);
        }
        public void SetIKHintPositionWeight(UnityEngine.AvatarIKHint hint, System.Single value, bool always = false)
        {
            if (Equals(hint, fields[60]) & Equals(value, fields[61]) & !always) return;
            fields[60] = hint;
            fields[61] = value;
            AddOperation(59, hint, value);
        }
        public void SetLookAtPosition(UnityEngine.Vector3 lookAtPosition, bool always = false)
        {
            if (Equals(lookAtPosition, fields[63]) & !always) return;
            fields[63] = lookAtPosition;
            AddOperation(62, lookAtPosition);
        }
        public void SetLookAtWeight(System.Single weight, bool always = false)
        {
            if (Equals(weight, fields[65]) & !always) return;
            fields[65] = weight;
            AddOperation(64, weight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, bool always = false)
        {
            if (Equals(weight, fields[67]) & Equals(bodyWeight, fields[68]) & !always) return;
            fields[67] = weight;
            fields[68] = bodyWeight;
            AddOperation(66, weight, bodyWeight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, System.Single headWeight, bool always = false)
        {
            if (Equals(weight, fields[70]) & Equals(bodyWeight, fields[71]) & Equals(headWeight, fields[72]) & !always) return;
            fields[70] = weight;
            fields[71] = bodyWeight;
            fields[72] = headWeight;
            AddOperation(69, weight, bodyWeight, headWeight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, System.Single headWeight, System.Single eyesWeight, bool always = false)
        {
            if (Equals(weight, fields[74]) & Equals(bodyWeight, fields[75]) & Equals(headWeight, fields[76]) & Equals(eyesWeight, fields[77]) & !always) return;
            fields[74] = weight;
            fields[75] = bodyWeight;
            fields[76] = headWeight;
            fields[77] = eyesWeight;
            AddOperation(73, weight, bodyWeight, headWeight, eyesWeight);
        }
        public void SetLookAtWeight(System.Single weight, System.Single bodyWeight, System.Single headWeight, System.Single eyesWeight, System.Single clampWeight, bool always = false)
        {
            if (Equals(weight, fields[79]) & Equals(bodyWeight, fields[80]) & Equals(headWeight, fields[81]) & Equals(eyesWeight, fields[82]) & Equals(clampWeight, fields[83]) & !always) return;
            fields[79] = weight;
            fields[80] = bodyWeight;
            fields[81] = headWeight;
            fields[82] = eyesWeight;
            fields[83] = clampWeight;
            AddOperation(78, weight, bodyWeight, headWeight, eyesWeight, clampWeight);
        }
        public void SetBoneLocalRotation(UnityEngine.HumanBodyBones humanBoneId, UnityEngine.Quaternion rotation, bool always = false)
        {
            if (Equals(humanBoneId, fields[85]) & Equals(rotation, fields[86]) & !always) return;
            fields[85] = humanBoneId;
            fields[86] = rotation;
            AddOperation(84, humanBoneId, rotation);
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

            AddOperation(87, null);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, bool always = false)
        {
            if (Equals(stateName, fields[89]) & Equals(fixedTransitionDuration, fields[90]) & !always) return;
            fields[89] = stateName;
            fields[90] = fixedTransitionDuration;
            AddOperation(88, stateName, fixedTransitionDuration);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[92]) & Equals(fixedTransitionDuration, fields[93]) & Equals(layer, fields[94]) & !always) return;
            fields[92] = stateName;
            fields[93] = fixedTransitionDuration;
            fields[94] = layer;
            AddOperation(91, stateName, fixedTransitionDuration, layer);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, System.Int32 layer, System.Single fixedTimeOffset, bool always = false)
        {
            if (Equals(stateName, fields[96]) & Equals(fixedTransitionDuration, fields[97]) & Equals(layer, fields[98]) & Equals(fixedTimeOffset, fields[99]) & !always) return;
            fields[96] = stateName;
            fields[97] = fixedTransitionDuration;
            fields[98] = layer;
            fields[99] = fixedTimeOffset;
            AddOperation(95, stateName, fixedTransitionDuration, layer, fixedTimeOffset);
        }
        public void CrossFadeInFixedTime(System.String stateName, System.Single fixedTransitionDuration, System.Int32 layer, System.Single fixedTimeOffset, System.Single normalizedTransitionTime, bool always = false)
        {
            if (Equals(stateName, fields[101]) & Equals(fixedTransitionDuration, fields[102]) & Equals(layer, fields[103]) & Equals(fixedTimeOffset, fields[104]) & Equals(normalizedTransitionTime, fields[105]) & !always) return;
            fields[101] = stateName;
            fields[102] = fixedTransitionDuration;
            fields[103] = layer;
            fields[104] = fixedTimeOffset;
            fields[105] = normalizedTransitionTime;
            AddOperation(100, stateName, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
        }
        public void CrossFadeInFixedTime(System.Int32 stateHashName, System.Single fixedTransitionDuration, System.Int32 layer, System.Single fixedTimeOffset, bool always = false)
        {
            if (Equals(stateHashName, fields[107]) & Equals(fixedTransitionDuration, fields[108]) & Equals(layer, fields[109]) & Equals(fixedTimeOffset, fields[110]) & !always) return;
            fields[107] = stateHashName;
            fields[108] = fixedTransitionDuration;
            fields[109] = layer;
            fields[110] = fixedTimeOffset;
            AddOperation(106, stateHashName, fixedTransitionDuration, layer, fixedTimeOffset);
        }
        public void CrossFadeInFixedTime(System.Int32 stateHashName, System.Single fixedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateHashName, fields[112]) & Equals(fixedTransitionDuration, fields[113]) & Equals(layer, fields[114]) & !always) return;
            fields[112] = stateHashName;
            fields[113] = fixedTransitionDuration;
            fields[114] = layer;
            AddOperation(111, stateHashName, fixedTransitionDuration, layer);
        }
        public void CrossFadeInFixedTime(System.Int32 stateHashName, System.Single fixedTransitionDuration, bool always = false)
        {
            if (Equals(stateHashName, fields[116]) & Equals(fixedTransitionDuration, fields[117]) & !always) return;
            fields[116] = stateHashName;
            fields[117] = fixedTransitionDuration;
            AddOperation(115, stateHashName, fixedTransitionDuration);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, System.Int32 layer, System.Single normalizedTimeOffset, bool always = false)
        {
            if (Equals(stateName, fields[119]) & Equals(normalizedTransitionDuration, fields[120]) & Equals(layer, fields[121]) & Equals(normalizedTimeOffset, fields[122]) & !always) return;
            fields[119] = stateName;
            fields[120] = normalizedTransitionDuration;
            fields[121] = layer;
            fields[122] = normalizedTimeOffset;
            AddOperation(118, stateName, normalizedTransitionDuration, layer, normalizedTimeOffset);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[124]) & Equals(normalizedTransitionDuration, fields[125]) & Equals(layer, fields[126]) & !always) return;
            fields[124] = stateName;
            fields[125] = normalizedTransitionDuration;
            fields[126] = layer;
            AddOperation(123, stateName, normalizedTransitionDuration, layer);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, bool always = false)
        {
            if (Equals(stateName, fields[128]) & Equals(normalizedTransitionDuration, fields[129]) & !always) return;
            fields[128] = stateName;
            fields[129] = normalizedTransitionDuration;
            AddOperation(127, stateName, normalizedTransitionDuration);
        }
        public void CrossFade(System.String stateName, System.Single normalizedTransitionDuration, System.Int32 layer, System.Single normalizedTimeOffset, System.Single normalizedTransitionTime, bool always = false)
        {
            if (Equals(stateName, fields[131]) & Equals(normalizedTransitionDuration, fields[132]) & Equals(layer, fields[133]) & Equals(normalizedTimeOffset, fields[134]) & Equals(normalizedTransitionTime, fields[135]) & !always) return;
            fields[131] = stateName;
            fields[132] = normalizedTransitionDuration;
            fields[133] = layer;
            fields[134] = normalizedTimeOffset;
            fields[135] = normalizedTransitionTime;
            AddOperation(130, stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
        }
        public void CrossFade(System.Int32 stateHashName, System.Single normalizedTransitionDuration, System.Int32 layer, System.Single normalizedTimeOffset, bool always = false)
        {
            if (Equals(stateHashName, fields[137]) & Equals(normalizedTransitionDuration, fields[138]) & Equals(layer, fields[139]) & Equals(normalizedTimeOffset, fields[140]) & !always) return;
            fields[137] = stateHashName;
            fields[138] = normalizedTransitionDuration;
            fields[139] = layer;
            fields[140] = normalizedTimeOffset;
            AddOperation(136, stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset);
        }
        public void CrossFade(System.Int32 stateHashName, System.Single normalizedTransitionDuration, System.Int32 layer, bool always = false)
        {
            if (Equals(stateHashName, fields[142]) & Equals(normalizedTransitionDuration, fields[143]) & Equals(layer, fields[144]) & !always) return;
            fields[142] = stateHashName;
            fields[143] = normalizedTransitionDuration;
            fields[144] = layer;
            AddOperation(141, stateHashName, normalizedTransitionDuration, layer);
        }
        public void CrossFade(System.Int32 stateHashName, System.Single normalizedTransitionDuration, bool always = false)
        {
            if (Equals(stateHashName, fields[146]) & Equals(normalizedTransitionDuration, fields[147]) & !always) return;
            fields[146] = stateHashName;
            fields[147] = normalizedTransitionDuration;
            AddOperation(145, stateHashName, normalizedTransitionDuration);
        }
        public void PlayInFixedTime(System.String stateName, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[149]) & Equals(layer, fields[150]) & !always) return;
            fields[149] = stateName;
            fields[150] = layer;
            AddOperation(148, stateName, layer);
        }
        public void PlayInFixedTime(System.String stateName, bool always = false)
        {
            if (Equals(stateName, fields[152]) & !always) return;
            fields[152] = stateName;
            AddOperation(151, stateName);
        }
        public void PlayInFixedTime(System.String stateName, System.Int32 layer, System.Single fixedTime, bool always = false)
        {
            if (Equals(stateName, fields[154]) & Equals(layer, fields[155]) & Equals(fixedTime, fields[156]) & !always) return;
            fields[154] = stateName;
            fields[155] = layer;
            fields[156] = fixedTime;
            AddOperation(153, stateName, layer, fixedTime);
        }
        public void PlayInFixedTime(System.Int32 stateNameHash, System.Int32 layer, bool always = false)
        {
            if (Equals(stateNameHash, fields[158]) & Equals(layer, fields[159]) & !always) return;
            fields[158] = stateNameHash;
            fields[159] = layer;
            AddOperation(157, stateNameHash, layer);
        }
        public void PlayInFixedTime(System.Int32 stateNameHash, bool always = false)
        {
            if (Equals(stateNameHash, fields[161]) & !always) return;
            fields[161] = stateNameHash;
            AddOperation(160, stateNameHash);
        }
        public void Play(System.String stateName, System.Int32 layer, bool always = false)
        {
            if (Equals(stateName, fields[163]) & Equals(layer, fields[164]) & !always) return;
            fields[163] = stateName;
            fields[164] = layer;
            AddOperation(162, stateName, layer);
        }
        public void Play(System.String stateName, bool always = false)
        {
            if (Equals(stateName, fields[166]) & !always) return;
            fields[166] = stateName;
            AddOperation(165, stateName);
        }
        public void Play(System.String stateName, System.Int32 layer, System.Single normalizedTime, bool always = false)
        {
            if (Equals(stateName, fields[168]) & Equals(layer, fields[169]) & Equals(normalizedTime, fields[170]) & !always) return;
            fields[168] = stateName;
            fields[169] = layer;
            fields[170] = normalizedTime;
            AddOperation(167, stateName, layer, normalizedTime);
        }
        public void Play(System.Int32 stateNameHash, System.Int32 layer, bool always = false)
        {
            if (Equals(stateNameHash, fields[172]) & Equals(layer, fields[173]) & !always) return;
            fields[172] = stateNameHash;
            fields[173] = layer;
            AddOperation(171, stateNameHash, layer);
        }
        public void Play(System.Int32 stateNameHash, bool always = false)
        {
            if (Equals(stateNameHash, fields[175]) & !always) return;
            fields[175] = stateNameHash;
            AddOperation(174, stateNameHash);
        }
        public UnityEngine.Transform GetBoneTransform(UnityEngine.HumanBodyBones humanBoneId)
        {
            return self.GetBoneTransform(humanBoneId);
        }
        public void Rebind(bool always = false)
        {
            if (!always) return;

            AddOperation(176, null);
        }
        public override void OnNetworkOperationHandler(Operation opt)
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
                        var stabilizeFeet = DeserializeObject<System.Boolean>(new Segment(opt.buffer, false));
                        fields[3] = stabilizeFeet;
                        self.stabilizeFeet = stabilizeFeet;
                    }
                    break;
                case 4:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var speed = DeserializeObject<System.Single>(new Segment(opt.buffer, false));
                        fields[4] = speed;
                        self.speed = speed;
                    }
                    break;
                case 5:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var cullingMode = DeserializeObject<UnityEngine.AnimatorCullingMode>(new Segment(opt.buffer, false));
                        fields[5] = cullingMode;
                        self.cullingMode = cullingMode;
                    }
                    break;
                case 6:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var keepAnimatorStateOnDisable = DeserializeObject<System.Boolean>(new Segment(opt.buffer, false));
                        fields[6] = keepAnimatorStateOnDisable;
                        self.keepAnimatorStateOnDisable = keepAnimatorStateOnDisable;
                    }
                    break;
                case 7:
                    {
                        if (opt.uid == ClientBase.Instance.UID)
                            return;
                        var writeDefaultValuesOnDisable = DeserializeObject<System.Boolean>(new Segment(opt.buffer, false));
                        fields[7] = writeDefaultValuesOnDisable;
                        self.writeDefaultValuesOnDisable = writeDefaultValuesOnDisable;
                    }
                    break;
                case 8:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[9] = data.Obj);
                        var value = (System.Single)(fields[10] = data.Obj);
                        self.SetFloat(name, value);
                    }
                    break;
                case 11:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[12] = data.Obj);
                        var value = (System.Single)(fields[13] = data.Obj);
                        var dampTime = (System.Single)(fields[14] = data.Obj);
                        var deltaTime = (System.Single)(fields[15] = data.Obj);
                        self.SetFloat(name, value, dampTime, deltaTime);
                    }
                    break;
                case 16:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[17] = data.Obj);
                        var value = (System.Single)(fields[18] = data.Obj);
                        self.SetFloat(id, value);
                    }
                    break;
                case 19:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[20] = data.Obj);
                        var value = (System.Single)(fields[21] = data.Obj);
                        var dampTime = (System.Single)(fields[22] = data.Obj);
                        var deltaTime = (System.Single)(fields[23] = data.Obj);
                        self.SetFloat(id, value, dampTime, deltaTime);
                    }
                    break;
                case 24:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[25] = data.Obj);
                        var value = (System.Boolean)(fields[26] = data.Obj);
                        self.SetBool(name, value);
                    }
                    break;
                case 27:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[28] = data.Obj);
                        var value = (System.Boolean)(fields[29] = data.Obj);
                        self.SetBool(id, value);
                    }
                    break;
                case 30:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[31] = data.Obj);
                        var value = (System.Int32)(fields[32] = data.Obj);
                        self.SetInteger(name, value);
                    }
                    break;
                case 33:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[34] = data.Obj);
                        var value = (System.Int32)(fields[35] = data.Obj);
                        self.SetInteger(id, value);
                    }
                    break;
                case 36:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[37] = data.Obj);
                        self.SetTrigger(name);
                    }
                    break;
                case 38:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[39] = data.Obj);
                        self.SetTrigger(id);
                    }
                    break;
                case 40:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var name = (System.String)(fields[41] = data.Obj);
                        self.ResetTrigger(name);
                    }
                    break;
                case 42:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var id = (System.Int32)(fields[43] = data.Obj);
                        self.ResetTrigger(id);
                    }
                    break;
                case 44:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[45] = data.Obj);
                        var goalPosition = (UnityEngine.Vector3)(fields[46] = data.Obj);
                        self.SetIKPosition(goal, goalPosition);
                    }
                    break;
                case 47:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[48] = data.Obj);
                        var goalRotation = (UnityEngine.Quaternion)(fields[49] = data.Obj);
                        self.SetIKRotation(goal, goalRotation);
                    }
                    break;
                case 50:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[51] = data.Obj);
                        var value = (System.Single)(fields[52] = data.Obj);
                        self.SetIKPositionWeight(goal, value);
                    }
                    break;
                case 53:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var goal = (UnityEngine.AvatarIKGoal)(fields[54] = data.Obj);
                        var value = (System.Single)(fields[55] = data.Obj);
                        self.SetIKRotationWeight(goal, value);
                    }
                    break;
                case 56:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var hint = (UnityEngine.AvatarIKHint)(fields[57] = data.Obj);
                        var hintPosition = (UnityEngine.Vector3)(fields[58] = data.Obj);
                        self.SetIKHintPosition(hint, hintPosition);
                    }
                    break;
                case 59:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var hint = (UnityEngine.AvatarIKHint)(fields[60] = data.Obj);
                        var value = (System.Single)(fields[61] = data.Obj);
                        self.SetIKHintPositionWeight(hint, value);
                    }
                    break;
                case 62:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var lookAtPosition = (UnityEngine.Vector3)(fields[63] = data.Obj);
                        self.SetLookAtPosition(lookAtPosition);
                    }
                    break;
                case 64:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[65] = data.Obj);
                        self.SetLookAtWeight(weight);
                    }
                    break;
                case 66:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[67] = data.Obj);
                        var bodyWeight = (System.Single)(fields[68] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight);
                    }
                    break;
                case 69:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[70] = data.Obj);
                        var bodyWeight = (System.Single)(fields[71] = data.Obj);
                        var headWeight = (System.Single)(fields[72] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight, headWeight);
                    }
                    break;
                case 73:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[74] = data.Obj);
                        var bodyWeight = (System.Single)(fields[75] = data.Obj);
                        var headWeight = (System.Single)(fields[76] = data.Obj);
                        var eyesWeight = (System.Single)(fields[77] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight);
                    }
                    break;
                case 78:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var weight = (System.Single)(fields[79] = data.Obj);
                        var bodyWeight = (System.Single)(fields[80] = data.Obj);
                        var headWeight = (System.Single)(fields[81] = data.Obj);
                        var eyesWeight = (System.Single)(fields[82] = data.Obj);
                        var clampWeight = (System.Single)(fields[83] = data.Obj);
                        self.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
                    }
                    break;
                case 84:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var humanBoneId = (UnityEngine.HumanBodyBones)(fields[85] = data.Obj);
                        var rotation = (UnityEngine.Quaternion)(fields[86] = data.Obj);
                        self.SetBoneLocalRotation(humanBoneId, rotation);
                    }
                    break;
                case 87:
                    {
                        self.InterruptMatchTarget();
                    }
                    break;
                case 88:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[89] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[90] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
                    }
                    break;
                case 91:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[92] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[93] = data.Obj);
                        var layer = (System.Int32)(fields[94] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration, layer);
                    }
                    break;
                case 95:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[96] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[97] = data.Obj);
                        var layer = (System.Int32)(fields[98] = data.Obj);
                        var fixedTimeOffset = (System.Single)(fields[99] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration, layer, fixedTimeOffset);
                    }
                    break;
                case 100:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[101] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[102] = data.Obj);
                        var layer = (System.Int32)(fields[103] = data.Obj);
                        var fixedTimeOffset = (System.Single)(fields[104] = data.Obj);
                        var normalizedTransitionTime = (System.Single)(fields[105] = data.Obj);
                        self.CrossFadeInFixedTime(stateName, fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
                    }
                    break;
                case 106:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[107] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[108] = data.Obj);
                        var layer = (System.Int32)(fields[109] = data.Obj);
                        var fixedTimeOffset = (System.Single)(fields[110] = data.Obj);
                        self.CrossFadeInFixedTime(stateHashName, fixedTransitionDuration, layer, fixedTimeOffset);
                    }
                    break;
                case 111:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[112] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[113] = data.Obj);
                        var layer = (System.Int32)(fields[114] = data.Obj);
                        self.CrossFadeInFixedTime(stateHashName, fixedTransitionDuration, layer);
                    }
                    break;
                case 115:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[116] = data.Obj);
                        var fixedTransitionDuration = (System.Single)(fields[117] = data.Obj);
                        self.CrossFadeInFixedTime(stateHashName, fixedTransitionDuration);
                    }
                    break;
                case 118:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[119] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[120] = data.Obj);
                        var layer = (System.Int32)(fields[121] = data.Obj);
                        var normalizedTimeOffset = (System.Single)(fields[122] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset);
                    }
                    break;
                case 123:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[124] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[125] = data.Obj);
                        var layer = (System.Int32)(fields[126] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration, layer);
                    }
                    break;
                case 127:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[128] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[129] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration);
                    }
                    break;
                case 130:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[131] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[132] = data.Obj);
                        var layer = (System.Int32)(fields[133] = data.Obj);
                        var normalizedTimeOffset = (System.Single)(fields[134] = data.Obj);
                        var normalizedTransitionTime = (System.Single)(fields[135] = data.Obj);
                        self.CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
                    }
                    break;
                case 136:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[137] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[138] = data.Obj);
                        var layer = (System.Int32)(fields[139] = data.Obj);
                        var normalizedTimeOffset = (System.Single)(fields[140] = data.Obj);
                        self.CrossFade(stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset);
                    }
                    break;
                case 141:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[142] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[143] = data.Obj);
                        var layer = (System.Int32)(fields[144] = data.Obj);
                        self.CrossFade(stateHashName, normalizedTransitionDuration, layer);
                    }
                    break;
                case 145:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateHashName = (System.Int32)(fields[146] = data.Obj);
                        var normalizedTransitionDuration = (System.Single)(fields[147] = data.Obj);
                        self.CrossFade(stateHashName, normalizedTransitionDuration);
                    }
                    break;
                case 148:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[149] = data.Obj);
                        var layer = (System.Int32)(fields[150] = data.Obj);
                        self.PlayInFixedTime(stateName, layer);
                    }
                    break;
                case 151:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[152] = data.Obj);
                        self.PlayInFixedTime(stateName);
                    }
                    break;
                case 153:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[154] = data.Obj);
                        var layer = (System.Int32)(fields[155] = data.Obj);
                        var fixedTime = (System.Single)(fields[156] = data.Obj);
                        self.PlayInFixedTime(stateName, layer, fixedTime);
                    }
                    break;
                case 157:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[158] = data.Obj);
                        var layer = (System.Int32)(fields[159] = data.Obj);
                        self.PlayInFixedTime(stateNameHash, layer);
                    }
                    break;
                case 160:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[161] = data.Obj);
                        self.PlayInFixedTime(stateNameHash);
                    }
                    break;
                case 162:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[163] = data.Obj);
                        var layer = (System.Int32)(fields[164] = data.Obj);
                        self.Play(stateName, layer);
                    }
                    break;
                case 165:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[166] = data.Obj);
                        self.Play(stateName);
                    }
                    break;
                case 167:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateName = (System.String)(fields[168] = data.Obj);
                        var layer = (System.Int32)(fields[169] = data.Obj);
                        var normalizedTime = (System.Single)(fields[170] = data.Obj);
                        self.Play(stateName, layer, normalizedTime);
                    }
                    break;
                case 171:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[172] = data.Obj);
                        var layer = (System.Int32)(fields[173] = data.Obj);
                        self.Play(stateNameHash, layer);
                    }
                    break;
                case 174:
                    {
                        var segment = new Segment(opt.buffer, false);
                        var data = DeserializeModel(segment);
                        var stateNameHash = (System.Int32)(fields[175] = data.Obj);
                        self.Play(stateNameHash);
                    }
                    break;
                case 176:
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