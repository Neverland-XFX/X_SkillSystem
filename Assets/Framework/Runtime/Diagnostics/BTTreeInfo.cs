namespace XSkillSystem
{
    // 行为树“实例”级的标识
    public readonly struct BTTreeInfo
    {
        // 运行时实例Id
        public readonly int TreeId;

        // 友好名（Graph名）
        public readonly string TreeName;

        // Unity InstanceID（可用于Editor定位）
        public readonly int GraphAssetId;

        public BTTreeInfo(int treeId, string treeName, int graphAssetId)
        {
            TreeId = treeId;
            TreeName = treeName;
            GraphAssetId = graphAssetId;
        }
    }
}