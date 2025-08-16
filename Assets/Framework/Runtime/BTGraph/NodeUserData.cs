using System;

namespace XSkillSystem
{
    [Serializable]
    public sealed class NodeUserData
    {
        public object Payload;
        public string NodeGuid;
        public string RuntimeName;

        public NodeUserData(object payload, string guid, string runtimeName)
        {
            Payload = payload;
            NodeGuid = guid;
            RuntimeName = runtimeName;
        }
    }
}