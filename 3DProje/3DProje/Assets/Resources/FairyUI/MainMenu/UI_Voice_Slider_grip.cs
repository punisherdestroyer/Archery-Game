/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace MainMenu
{
    public partial class UI_Voice_Slider_grip : GButton
    {
        public Controller m_button;
        public GGraph m_n0;
        public GGraph m_n1;
        public GGraph m_n2;
        public const string URL = "ui://okk438l4bzjsjg";

        public static UI_Voice_Slider_grip CreateInstance()
        {
            return (UI_Voice_Slider_grip)UIPackage.CreateObject("MainMenu", "Voice Slider_grip");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_button = GetControllerAt(0);
            m_n0 = (GGraph)GetChildAt(0);
            m_n1 = (GGraph)GetChildAt(1);
            m_n2 = (GGraph)GetChildAt(2);
        }
    }
}