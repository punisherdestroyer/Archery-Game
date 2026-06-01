/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_buttonMenu : GButton
    {
        public Controller m_button;
        public Controller m_cTitle;
        public GGraph m_up;
        public GGraph m_over;
        public GGraph m_down;
        public GTextField m_title;
        public const string URL = "ui://okk438l4bzjsj7";

        public static UI_buttonMenu CreateInstance()
        {
            return (UI_buttonMenu)UIPackage.CreateObject("packageMenu", "buttonMenu");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_button = GetControllerAt(0);
            m_cTitle = GetControllerAt(1);
            m_up = (GGraph)GetChildAt(0);
            m_over = (GGraph)GetChildAt(1);
            m_down = (GGraph)GetChildAt(2);
            m_title = (GTextField)GetChildAt(3);
        }
    }
}