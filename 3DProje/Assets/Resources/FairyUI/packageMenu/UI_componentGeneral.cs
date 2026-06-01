/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_componentGeneral : GComponent
    {
        public GLoader m_loaderBackground;
        public GTextField m_textHeader;
        public GTextField m_textDescription;
        public UI_buttonMenu m_buttonBack;
        public const string URL = "ui://okk438l4j7p4jx";

        public static UI_componentGeneral CreateInstance()
        {
            return (UI_componentGeneral)UIPackage.CreateObject("packageMenu", "componentGeneral");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_loaderBackground = (GLoader)GetChildAt(0);
            m_textHeader = (GTextField)GetChildAt(1);
            m_textDescription = (GTextField)GetChildAt(2);
            m_buttonBack = (UI_buttonMenu)GetChildAt(3);
        }
    }
}