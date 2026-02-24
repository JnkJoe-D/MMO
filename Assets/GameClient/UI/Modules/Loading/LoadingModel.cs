using Game.Framework;

namespace Game.UI.Modules.Loading
{
    public class LoadingModel : UIModel
    {
        public float Progress { get; set; }
        public string LoadingText { get; set; }

        public override void Reset()
        {
            Progress = 0f;
            LoadingText = string.Empty;
        }
    }
}
