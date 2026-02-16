using System.Collections.Generic;

public class UiConfig
{
    public int viewId;
    public string pathFilename;
    public int layerType;
    public bool isLarge;

    public List<int> closeLayerList = new List<int>();

    public UiConfig(int viewId, string pathFilename, int layerType, bool isLarge = false)
    {
        this.viewId = viewId;
        this.pathFilename = pathFilename;
        this.layerType = layerType;
        this.isLarge = isLarge;
    }
}