/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_componentMainMenu : GComponent
    {
        public GLoader m_loaderBackground;
        public GTextField m_textHeader;
        public GTextField m_textSplash;
        public UI_buttonMenu m_buttonSettings;
        public UI_buttonMenu m_buttonPlay;
        public UI_buttonMenu m_buttonHTP;
        public GTextField m_textVersion;
        public GTextField m_textAuthor;
        public Transition m_Splash;
        public const string URL = "ui://okk438l4bzjsj6";

        public static UI_componentMainMenu CreateInstance()
        {
            return (UI_componentMainMenu)UIPackage.CreateObject("packageMenu", "componentMainMenu");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_loaderBackground = (GLoader)GetChildAt(0);
            m_textHeader = (GTextField)GetChildAt(1);
            m_textSplash = (GTextField)GetChildAt(2);
            m_buttonSettings = (UI_buttonMenu)GetChildAt(3);
            m_buttonPlay = (UI_buttonMenu)GetChildAt(4);
            m_buttonHTP = (UI_buttonMenu)GetChildAt(5);
            m_textVersion = (GTextField)GetChildAt(6);
            m_textAuthor = (GTextField)GetChildAt(7);
            m_Splash = GetTransitionAt(0);
        }
    }
}