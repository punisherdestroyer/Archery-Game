/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace MainMenu
{
    public partial class UI_Next_Button : GButton
    {
        public Controller m_button;
        public GGraph m_n0;
        public GGraph m_n1;
        public GGraph m_n2;
        public GLoader m_cooldown;
        public const string URL = "ui://okk438l4au6ojp";

        public static UI_Next_Button CreateInstance()
        {
            return (UI_Next_Button)UIPackage.CreateObject("MainMenu", "Next Button");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_button = GetControllerAt(0);
            m_n0 = (GGraph)GetChildAt(0);
            m_n1 = (GGraph)GetChildAt(1);
            m_n2 = (GGraph)GetChildAt(2);
            m_cooldown = (GLoader)GetChildAt(3);
        }
    }
}