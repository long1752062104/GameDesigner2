#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if SHADER_ANIMATED
using FSG.MeshAnimator.ShaderAnimated;
#endif

namespace GameDesigner
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StateManager))]
    public class StateManagerEditor : StateMachineViewEditor
    {
        private SerializedProperty _animationProperty;
        private SerializedProperty animationProperty
        {
            get
            {
                if (_animationProperty == null)
                    _animationProperty = SupportObject.FindProperty("animation");
                return _animationProperty;
            }
        }

        private SerializedProperty _animatorProperty;
        private SerializedProperty animatorProperty
        {
            get
            {
                if (_animatorProperty == null)
                    _animatorProperty = SupportObject.FindProperty("animator");
                return _animatorProperty;
            }
        }

        private SerializedProperty _meshAnimatorProperty;
        private SerializedProperty meshAnimatorProperty
        {
            get
            {
                if (_meshAnimatorProperty == null)
                    _meshAnimatorProperty = SupportObject.FindProperty("meshAnimator");
                return _meshAnimatorProperty;
            }
        }

        private SerializedProperty _directorProperty;
        private SerializedProperty directorProperty
        {
            get
            {
                if (_directorProperty == null)
                    _directorProperty = SupportObject.FindProperty("director");
                return _directorProperty;
            }
        }

        protected override void OnEnable()
        {
            var sm = target as StateManager;
            self = sm.support;
            if (self != null)
            {
                self.EditorInit(sm.transform);
                self.stateMachine.transform = self.transform;
                StateMachineWindow.stateMachine = self.stateMachine;
            }
            if (findBehaviourTypes == null)
            {
                findBehaviourTypes = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes, typeof(StateBehaviour));
            }
            if (findBehaviourTypes1 == null)
            {
                findBehaviourTypes1 = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes1, typeof(ActionBehaviour));
            }
            if (findBehaviourTypes2 == null)
            {
                findBehaviourTypes2 = new List<Type>();
                AddBehaviourTypes(findBehaviourTypes2, typeof(TransitionBehaviour));
            }
        }

        protected override void OnDrawPreField()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("support"), new GUIContent(BlueprintGUILayout.Instance.Language["State Machine Controller"]));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initMode"), new GUIContent(BlueprintGUILayout.Instance.Language["initMode"]));
        }

        protected override void OnDrawAnimationField()
        {
            var view = (StateMachineMono)self;
            view.animMode = (AnimationMode)EditorGUILayout.EnumPopup(new GUIContent(BlueprintGUILayout.Instance.Language["Animation mode"], "animMode"), view.animMode);
            switch (view.animMode)
            {
                case AnimationMode.Animation:
                    EditorGUILayout.PropertyField(animationProperty, new GUIContent(BlueprintGUILayout.Instance.Language["Old animation"], "animation"));
                    break;
                case AnimationMode.Animator:
                    EditorGUILayout.PropertyField(animatorProperty, new GUIContent(BlueprintGUILayout.Instance.Language["New animation"], "animator"));
                    break;
                case AnimationMode.Timeline:
                    EditorGUILayout.PropertyField(directorProperty, new GUIContent(BlueprintGUILayout.Instance.Language["director animation"], "director"));
                    EditorGUILayout.PropertyField(animatorProperty, new GUIContent(BlueprintGUILayout.Instance.Language["New animation"], "animator"));
                    break;
#if SHADER_ANIMATED
                case AnimationMode.MeshAnimator:
                    EditorGUILayout.PropertyField(meshAnimatorProperty, new GUIContent(BlueprintGUILayout.Instance.Language["mesh Animated"], "meshAnimator"));
                    break;
#endif
            }
        }

        protected override void OnDrawActionPropertyField(SerializedProperty actionProperty)
        {
            var view = (StateMachineMono)self;
            if (view.animMode == AnimationMode.Timeline)
                EditorGUILayout.PropertyField(actionProperty.FindPropertyRelative("clipAsset"), new GUIContent(BlueprintGUILayout.Instance.Language["Playable Asset"], "clipAsset"));
        }

        protected override void OnPlayAnimation(StateAction action)
        {
            var view = (StateMachineMono)self;
            EditorGUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect();
            if (GUI.Button(new Rect(rect.x + 45, rect.y, 30, rect.height), EditorGUIUtility.IconContent(animPlay ? "PauseButton" : "PlayButton")))
            {
                animPlay = !animPlay;
                animAction = action;
            }
            EditorGUI.BeginChangeCheck();
            action.animTime = GUI.HorizontalSlider(new Rect(rect.x + 75, rect.y, rect.width - 75, rect.height), action.animTime, 0f, action.animTimeMax);
            var normalizedTime = action.animTime / action.animTimeMax;
            EditorGUI.ProgressBar(new Rect(rect.x + 75, rect.y, rect.width - 75, rect.height), normalizedTime, $"动画进度:{action.animTime.ToString("f0")}");
            if (EditorGUI.EndChangeCheck())
            {
                animPlay = false;
                animAction = action;
                if (!EditorApplication.isPlaying)
                    PlayAnimation(view, action, normalizedTime);
            }
            EditorGUILayout.EndHorizontal();
            if (animPlay && animAction == action && !EditorApplication.isPlaying)
            {
                action.animTime += 20f * Time.deltaTime;
                if (action.animTime >= action.animTimeMax)
                    action.animTime = 0f;
                PlayAnimation(view, action, normalizedTime);
            }
        }

        private void PlayAnimation(StateMachineMono view, StateAction action, float normalizedTime)
        {
            switch (view.animMode)
            {
                case AnimationMode.Animation:
                    {
                        var animation = view.animation;
                        var clip = animation[action.clipName].clip;
                        float time = clip.length * normalizedTime;
                        clip.SampleAnimation(view.gameObject, time);
                    }
                    break;
                case AnimationMode.Animator:
                    {
                        var animator = view.animator;
                        animator.Play(action.clipName, 0, normalizedTime);
                        animator.Update(0f);
                    }
                    break;
                case AnimationMode.Timeline:
                    {
                    }
                    break;
#if SHADER_ANIMATED
                case AnimationMode.MeshAnimator:
                    {
                    }
                    break;
#endif
            }
        }
    }
}
#endif