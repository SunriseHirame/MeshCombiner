namespace Hirame.MeshCombiner
{
    [System.Flags]
    public enum CombineOptions { IgnoreInstancedMaterials = 1 }

    public static class CombineOptionsExtensions
    {
        public static bool HasFlagNonAlloc (this CombineOptions self, CombineOptions flag)
        {
            return (self & flag) == flag;
        }
    }
}