namespace GameCore
{
    public enum Platform
    {
        //     Build a macOS standalone (Intel 64-bit).
        StandaloneOSX = 2,

        //     Build a Windows standalone.
        StandaloneWindows = 5,

        //     Build an iOS player.
        iOS = 9,

        //     Build an Android .apk standalone app.
        Android = 13,

        //     Build a Windows 64-bit standalone.
        StandaloneWindows64 = 19,

        //     Build to WebGL platform.
        WebGL = 20,

        //     Build an Windows Store Apps player.
        WSAPlayer = 21,

        //     Build a Linux 64-bit standalone.
        StandaloneLinux64 = 24,

        

        //     Build a PS4 Standalone.
        PS4 = 0x1F,

        //     Build a Xbox One Standalone.
        XboxOne = 33,


        //     Build to Apple's tvOS platform.
        tvOS = 37,

        //     Build a Nintendo Switch player.
        Switch = 38,

        //     Build a Stadia standalone.
        Stadia = 40,


        //     Build a LinuxHeadlessSimulation standalone.
        LinuxHeadlessSimulation = 41,

        GameCoreXboxSeries = 42,
        GameCoreXboxOne = 43,

        //     Build to PlayStation 5 platform.
        PS5 = 44,
        EmbeddedLinux = 45,
        QNX = 46,

        NoTarget = -2
    }
}