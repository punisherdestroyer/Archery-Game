/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace MainMenu
{
    public partial class UI_Voice_Slider : GSlider
    {
        public GGraph m_n0;
        public GGraph m_bar;
        public GTextField m_title;
        public UI_Voice_Slider_grip m_grip;
        public const string URL = "ui://okk438l4bzjsjf";

        public static UI_Voice_Slider CreateInstance()
        {
            return (UI_Voice_Slider)UIPackage.CreateObject("MainMenu", "Voice Slider");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_n0 = (GGraph)GetChildAt(0);
            m_bar = (GGraph)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_grip = (UI_Voice_Slider_grip)GetChildAt(3);
        }
    }
}