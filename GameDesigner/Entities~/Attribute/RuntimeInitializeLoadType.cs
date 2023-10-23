
namespace Net.Entities
{
    public enum RuntimeInitializeLoadType
    {
        AfterSceneLoad,
		BeforeSceneLoad,
		AfterAssembliesLoaded,
		BeforeSplashScreen,
		SubsystemRegistration
    }
}
