using System;

namespace Net.Entities
{
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        private RuntimeInitializeLoadType m_LoadType;
        public RuntimeInitializeLoadType loadType
        {
            get
            {
                return this.m_LoadType;
            }
            private set
            {
                this.m_LoadType = value;
            }
        }

        public RuntimeInitializeOnLoadMethodAttribute()
        {
            this.loadType = RuntimeInitializeLoadType.AfterSceneLoad;
        }

        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType)
        {
            this.loadType = loadType;
        }
    }
}
