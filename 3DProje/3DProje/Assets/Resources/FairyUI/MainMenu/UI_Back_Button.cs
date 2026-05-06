/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace MainMenu
{
    public partial class UI_Back_Button : GButton
    {
        public Controller m_button;
        public GGraph m_up;
        public GGraph m_over;
        public GGraph m_down;
        public GTextField m_title;
        public const string URL = "ui://okk438l4bzjsje";

        public static UI_Back_Button CreateInstance()
        {
            return (UI_Back_Button)UIPackage.CreateObject("MainMenu", "Back Button");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_button = GetControllerAt(0);
            m_up = (GGraph)GetChildAt(0);
            m_over = (GGraph)GetChildAt(1);
            m_down = (GGraph)GetChildAt(2);
            m_title = (GTextField)GetChildAt(3);
        }
    }
}