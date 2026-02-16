using System.Collections.Generic;

public enum ViewId
{
    None,
    DemoUi = 100,
    SuperListUi = 200,
    SuperGridUi = 300,
    LoginUi = 400,
    MainCityUi = 500,
    DialogueUi = 600,
    GuideUi = 700,
    Max,
}

public class UiConfigManger
{
    public const int None = -1;
    public const int Back = 100;
    public const int System = 200;
    public const int Menu = 300;
    public const int Dialog = 400;
    public const int MessageBox = 500;
    public const int MessageTip = 600;
    public const int Story = 700;
    public const int Guide = 800;
    public const int Loading = 900;
    public const int Max = 1000;

    private const string Demo_Path = "GameUi/DemoUi/prefab/DemoUi";
    private const string SuperList_Path = "GameUi/SuperListUi/prefab/SuperListUi";
    private const string SuperGrid_Path = "GameUi/SuperGridUi/prefab/SuperGridUi";
    private const string Login_Path = "GameUi/LoginUi/prefab/LoginUi";
    private const string MainCity_Path = "GameUi/MainCityUi/prefab/MainCityUi";
    private const string Dialogue_Path = "GameUi/DialogueUi/prefab/DialogueUi";
    private const string Guide_Path = "GameUi/GuideUi/prefab/GuideUi";

    private static Dictionary<int, UiConfig> uiDictionary = new Dictionary<int, UiConfig>()
    {
        { (int)ViewId.DemoUi,new UiConfig((int)ViewId.DemoUi,Demo_Path,System)},
        { (int)ViewId.SuperListUi,new UiConfig((int)ViewId.SuperListUi,SuperList_Path,System)},
        { (int)ViewId.SuperGridUi,new UiConfig((int)ViewId.SuperGridUi,SuperGrid_Path,System)},
        { (int)ViewId.LoginUi,new UiConfig((int)ViewId.LoginUi,Login_Path,System)},
        { (int)ViewId.MainCityUi,new UiConfig((int)ViewId.MainCityUi,MainCity_Path,Back)},
        { (int)ViewId.DialogueUi,new UiConfig((int)ViewId.DialogueUi,Dialogue_Path,Dialog)},
        { (int)ViewId.GuideUi,new UiConfig((int)ViewId.GuideUi,Guide_Path,Dialog)},
    };

    public static UiConfig GetConfig(int viewId)
    {
        if (uiDictionary.ContainsKey(viewId))
        {
            return uiDictionary[viewId];
        }

        return null;
    }

    public static void AddConfig(UiConfig uiConfig)
    {
        if (uiConfig != null && !uiDictionary.ContainsKey(uiConfig.viewId))
        {
            uiDictionary.Add(uiConfig.viewId, uiConfig);
        }
    }

    public static void RemoveConfig(int viewId)
    {
        if (uiDictionary.ContainsKey(viewId))
        {
            uiDictionary.Remove(viewId);
        }
    }
}
