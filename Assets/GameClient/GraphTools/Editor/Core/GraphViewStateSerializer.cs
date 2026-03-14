using System;

namespace Game.GraphTools.Editor
{
    public static class GraphViewStateSerializer
    {
        public static void Capture(GraphAssetBase graphAsset, BaseGraphView graphView)
        {
            if (graphAsset == null || graphView == null) return;

            graphAsset.EnsureMetadata();
            graphAsset.Metadata.ViewPosition = graphView.viewTransform.position;
            graphAsset.Metadata.ViewScale = graphView.viewTransform.scale;
            graphAsset.Metadata.LastOpenedAtUtc = DateTime.UtcNow.ToString("O");
        }

        public static void Apply(GraphAssetBase graphAsset, BaseGraphView graphView)
        {
            if (graphAsset == null || graphView == null || graphAsset.Metadata == null) return;
            graphView.UpdateViewTransform(graphAsset.Metadata.ViewPosition, graphAsset.Metadata.ViewScale);
        }
    }
}
