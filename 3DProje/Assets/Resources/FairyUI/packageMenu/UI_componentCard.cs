/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace packageMenu
{
    public partial class UI_componentCard : GComponent
    {
        public Controller m_cCard;
        public GGraph m_loaderBackgroundCard1;
        public GLoader m_iconAvatarCard1;
        public GRichTextField m_textHeaderCard1;
        public GRichTextField m_textDescriptionCard1;
        public UI_buttonDetails m_buttonNextCard1;
        public UI_buttonDetails m_buttonPreviousCard2;
        public const string URL = "ui://okk438l4j7p4jz";

        public static UI_componentCard CreateInstance()
        {
            return (UI_componentCard)UIPackage.CreateObject("packageMenu", "componentCard");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cCard = GetControllerAt(0);
            m_loaderBackgroundCard1 = (GGraph)GetChildAt(0);
            m_iconAvatarCard1 = (GLoader)GetChildAt(1);
            m_textHeaderCard1 = (GRichTextField)GetChildAt(2);
            m_textDescriptionCard1 = (GRichTextField)GetChildAt(3);
            m_buttonNextCard1 = (UI_buttonDetails)GetChildAt(4);
            m_buttonPreviousCard2 = (UI_buttonDetails)GetChildAt(5);
        }
    }
}