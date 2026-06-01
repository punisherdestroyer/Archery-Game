/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_buttonDetails : GButton
    {
        public Controller m_button;
        public Controller m_cType;
        public GLoader m_type;
        public const string URL = "ui://okk438l4au6ojp";

        public static UI_buttonDetails CreateInstance()
        {
            return (UI_buttonDetails)UIPackage.CreateObject("packageMenu", "buttonDetails");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_button = GetControllerAt(0);
            m_cType = GetControllerAt(1);
            m_type = (GLoader)GetChildAt(0);
        }
    }
}