/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_sliderVoice : GSlider
    {
        public Controller m_cTitle;
        public GGraph m_n0;
        public GGraph m_bar;
        public GTextField m_title;
        public UI_sliderVoice_grip m_grip;
        public const string URL = "ui://okk438l4j7p4jv";

        public static UI_sliderVoice CreateInstance()
        {
            return (UI_sliderVoice)UIPackage.CreateObject("packageMenu", "sliderVoice");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cTitle = GetControllerAt(0);
            m_n0 = (GGraph)GetChildAt(0);
            m_bar = (GGraph)GetChildAt(1);
            m_title = (GTextField)GetChildAt(2);
            m_grip = (UI_sliderVoice_grip)GetChildAt(3);
        }
    }
}