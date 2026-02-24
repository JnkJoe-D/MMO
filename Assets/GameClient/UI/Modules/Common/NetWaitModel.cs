using Game.Framework;

namespace Game.UI.Modules.Common
{
    public class NetWaitModel : UIModel
    {
        public string TipMessage { get; set; }

        public override void Reset()
        {
            TipMessage = string.Empty;
        }
    }
}
