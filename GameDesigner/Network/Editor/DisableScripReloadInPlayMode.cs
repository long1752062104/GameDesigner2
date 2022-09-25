#if UNITY_EDITOR && UNITY_2018_1_OR_NEWER
using UnityEditor;

[InitializeOnLoad]
public class DisableScripReloadInPlayMode
{
	static DisableScripReloadInPlayMode()
	{
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
	{
		switch (stateChange)
		{
			case PlayModeStateChange.EnteredPlayMode:
				EditorApplication.LockReloadAssemblies();
				break;
			case PlayModeStateChange.ExitingPlayMode:
				EditorApplication.UnlockReloadAssemblies();
				break;
		}
	}
}
#endif