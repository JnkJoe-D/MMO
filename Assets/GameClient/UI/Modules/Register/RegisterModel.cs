using Game.UI;

namespace Game.UI.Modules.Register
{
    public class RegisterModel : UIModel
    {
        public string Account { get; set; } = "";
        public string Password { get; set; } = "";
        public string RepeatPassword { get; set; } = "";
        public string Email {get;set;} = "";
    }
}
