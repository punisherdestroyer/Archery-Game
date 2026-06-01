/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_componentHTP : GComponent
    {
        public GLoader m_loaderBackground;
        public GTextField m_textHeader;
        public UI_buttonMenu m_buttonBack;
        public UI_buttonMenu m_buttonGeneral;
        public UI_buttonMenu m_buttonDetails;
        public const string URL = "ui://okk438l4j7p4jw";

        public static UI_componentHTP CreateInstance()
        {
            return (UI_componentHTP)UIPackage.CreateObject("packageMenu", "componentHTP");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_loaderBackground = (GLoader)GetChildAt(0);
            m_textHeader = (GTextField)GetChildAt(1);
            m_buttonBack = (UI_buttonMenu)GetChildAt(2);
            m_buttonGeneral = (UI_buttonMenu)GetChildAt(3);
            m_buttonDetails = (UI_buttonMenu)GetChildAt(4);
        }
    }
}