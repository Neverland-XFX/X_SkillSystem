namespace XSkillSystem
{
    public enum ModOp : byte
    {
        Add,
        // Mul 表示 “+%”，在取值时会 (1 + sumMul)
        Mul
    } 
}