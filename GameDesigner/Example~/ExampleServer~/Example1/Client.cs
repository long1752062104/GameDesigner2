using Net.Event;
using Net.Server;
using Net.Share;
using System;

namespace Example1
{
    public class Client : UdxPlayer
    {
        [SyncVar(id = 1, hook = nameof(intCheck))]
        public int testint;
        [SyncVar(id = 2, hook = nameof(stringCheck))]
        public string teststring;

        private void intCheck(int value)
        {
            NDebug.Log(value);
        }

        private void stringCheck(string value)
        {
            NDebug.Log(value);
        }
    }
}