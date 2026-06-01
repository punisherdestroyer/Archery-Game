/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_componentDetails : GComponent
    {
        public GLoader m_loaderBackground;
        public GTextField m_textHeader;
        public UI_buttonMenu m_buttonBack;
        public UI_buttonMenu m_buttonEnemies;
        public UI_buttonMenu m_buttonStats;
        public UI_buttonMenu m_buttonAbilities;
        public UI_buttonMenu m_buttonLevels;
        public const string URL = "ui://okk438l4j7p4jy";

        public static UI_componentDetails CreateInstance()
        {
            return (UI_componentDetails)UIPackage.CreateObject("packageMenu", "componentDetails");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_loaderBackground = (GLoader)GetChildAt(0);
            m_textHeader = (GTextField)GetChildAt(1);
            m_buttonBack = (UI_buttonMenu)GetChildAt(2);
            m_buttonEnemies = (UI_buttonMenu)GetChildAt(3);
            m_buttonStats = (UI_buttonMenu)GetChildAt(4);
            m_buttonAbilities = (UI_buttonMenu)GetChildAt(5);
            m_buttonLevels = (UI_buttonMenu)GetChildAt(6);
        }
    }
}