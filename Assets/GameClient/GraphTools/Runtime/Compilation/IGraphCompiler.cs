namespace Game.GraphTools
{
    public interface IGraphCompiler<in TGraphAsset> where TGraphAsset : GraphAssetBase
    {
        GraphCompileReport Compile(TGraphAsset graphAsset);
    }
}
