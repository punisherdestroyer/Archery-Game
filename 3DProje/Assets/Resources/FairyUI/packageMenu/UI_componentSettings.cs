/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_componentSettings : GComponent
    {
        public GLoader m_loaderBackground;
        public GTextField m_textHeader;
        public GTextField m_textSubHeader;
        public UI_buttonMenu m_buttonBack;
        public UI_sliderVoice m_sliderVoice;
        public const string URL = "ui://okk438l4j7p4jr";

        public static UI_componentSettings CreateInstance()
        {
            return (UI_componentSettings)UIPackage.CreateObject("packageMenu", "componentSettings");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_loaderBackground = (GLoader)GetChildAt(0);
            m_textHeader = (GTextField)GetChildAt(1);
            m_textSubHeader = (GTextField)GetChildAt(2);
            m_buttonBack = (UI_buttonMenu)GetChildAt(3);
            m_sliderVoice = (UI_sliderVoice)GetChildAt(4);
        }
    }
}