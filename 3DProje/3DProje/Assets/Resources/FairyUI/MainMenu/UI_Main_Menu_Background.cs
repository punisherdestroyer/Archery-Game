/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace MainMenu
{
    public partial class UI_Main_Menu_Background : GComponent
    {
        public Controller m_Manager;
        public GImage m_Background;
        public GTextField m_Main_Menu_Header;
        public GTextField m_Splash_Text;
        public UI_Play_Button m_Play_Button;
        public UI_Settings_Button m_Settings_Button;
        public UI_How_To_Play_Button m_How_To_Play_Button;
        public GTextField m_Version;
        public GTextField m_Author;
        public GGroup m_Main_Menu;
        public GTextField m_Settings_Header;
        public UI_Back_Button m_SBack_Button;
        public UI_Voice_Slider m_Voice_Slider;
        public GGroup m_Settings;
        public GTextField m_How_To_Play_Header;
        public UI_Back_Button m_HTPBack_Button;
        public UI_General_Button m_General_Button;
        public UI_Details_Button m_Details_Button;
        public GGroup m_How_To_Play;
        public GTextField m_General_Header;
        public UI_Back_Button m_GBack_Button;
        public GTextField m_General_Text;
        public GGroup m_General;
        public GTextField m_Details_Header;
        public UI_Back_Button m_DBack_Button;
        public UI_Back_Button m_Enemies_Button;
        public UI_Back_Button m_Stats_Button;
        public UI_Back_Button m_Abilities_Button;
        public UI_Back_Button m_Levels_Button;
        public GGroup m_Details;
        public GTextField m_Enemies_Header;
        public UI_Back_Button m_EDBack_Button;
        public UI_Back_Button m_EEnemies_Button;
        public UI_Back_Button m_EStats_Button;
        public UI_Back_Button m_EAbilities_Button;
        public UI_Back_Button m_ELevels_Button;
        public GGroup m_Enemies;
        public GTextField m_Stats_Header;
        public UI_Back_Button m_SDBack_Button;
        public UI_Back_Button m_SEnemies_Button;
        public UI_Back_Button m_SStats_Button;
        public UI_Back_Button m_SAbilities_Button;
        public UI_Back_Button m_SLevels_Button;
        public GGroup m_Stats;
        public GTextField m_Abilities_Header;
        public UI_Back_Button m_ADBack_Button;
        public UI_Back_Button m_AEnemies_Button;
        public UI_Back_Button m_AStats_Button;
        public UI_Back_Button m_AAbilities_Button;
        public UI_Back_Button m_ALevels_Button;
        public GGroup m_Abilities;
        public GTextField m_Levels_Header;
        public UI_Back_Button m_LDBack_Button;
        public UI_Back_Button m_LEnemies_Button;
        public UI_Back_Button m_LStats_Button;
        public UI_Back_Button m_LAbilities_Button;
        public UI_Back_Button m_LLevels_Button;
        public GGroup m_Levels;
        public Transition m_Splash;
        public const string URL = "ui://okk438l4bzjsj6";

        public static UI_Main_Menu_Background CreateInstance()
        {
            return (UI_Main_Menu_Background)UIPackage.CreateObject("MainMenu", "Main Menu Background");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_Manager = GetControllerAt(0);
            m_Background = (GImage)GetChildAt(0);
            m_Main_Menu_Header = (GTextField)GetChildAt(1);
            m_Splash_Text = (GTextField)GetChildAt(2);
            m_Play_Button = (UI_Play_Button)GetChildAt(3);
            m_Settings_Button = (UI_Settings_Button)GetChildAt(4);
            m_How_To_Play_Button = (UI_How_To_Play_Button)GetChildAt(5);
            m_Version = (GTextField)GetChildAt(6);
            m_Author = (GTextField)GetChildAt(7);
            m_Main_Menu = (GGroup)GetChildAt(8);
            m_Settings_Header = (GTextField)GetChildAt(9);
            m_SBack_Button = (UI_Back_Button)GetChildAt(10);
            m_Voice_Slider = (UI_Voice_Slider)GetChildAt(11);
            m_Settings = (GGroup)GetChildAt(12);
            m_How_To_Play_Header = (GTextField)GetChildAt(13);
            m_HTPBack_Button = (UI_Back_Button)GetChildAt(14);
            m_General_Button = (UI_General_Button)GetChildAt(15);
            m_Details_Button = (UI_Details_Button)GetChildAt(16);
            m_How_To_Play = (GGroup)GetChildAt(17);
            m_General_Header = (GTextField)GetChildAt(18);
            m_GBack_Button = (UI_Back_Button)GetChildAt(19);
            m_General_Text = (GTextField)GetChildAt(20);
            m_General = (GGroup)GetChildAt(21);
            m_Details_Header = (GTextField)GetChildAt(22);
            m_DBack_Button = (UI_Back_Button)GetChildAt(23);
            m_Enemies_Button = (UI_Back_Button)GetChildAt(24);
            m_Stats_Button = (UI_Back_Button)GetChildAt(25);
            m_Abilities_Button = (UI_Back_Button)GetChildAt(26);
            m_Levels_Button = (UI_Back_Button)GetChildAt(27);
            m_Details = (GGroup)GetChildAt(28);
            m_Enemies_Header = (GTextField)GetChildAt(29);
            m_EDBack_Button = (UI_Back_Button)GetChildAt(30);
            m_EEnemies_Button = (UI_Back_Button)GetChildAt(31);
            m_EStats_Button = (UI_Back_Button)GetChildAt(32);
            m_EAbilities_Button = (UI_Back_Button)GetChildAt(33);
            m_ELevels_Button = (UI_Back_Button)GetChildAt(34);
            m_Enemies = (GGroup)GetChildAt(35);
            m_Stats_Header = (GTextField)GetChildAt(36);
            m_SDBack_Button = (UI_Back_Button)GetChildAt(37);
            m_SEnemies_Button = (UI_Back_Button)GetChildAt(38);
            m_SStats_Button = (UI_Back_Button)GetChildAt(39);
            m_SAbilities_Button = (UI_Back_Button)GetChildAt(40);
            m_SLevels_Button = (UI_Back_Button)GetChildAt(41);
            m_Stats = (GGroup)GetChildAt(42);
            m_Abilities_Header = (GTextField)GetChildAt(43);
            m_ADBack_Button = (UI_Back_Button)GetChildAt(44);
            m_AEnemies_Button = (UI_Back_Button)GetChildAt(45);
            m_AStats_Button = (UI_Back_Button)GetChildAt(46);
            m_AAbilities_Button = (UI_Back_Button)GetChildAt(47);
            m_ALevels_Button = (UI_Back_Button)GetChildAt(48);
            m_Abilities = (GGroup)GetChildAt(49);
            m_Levels_Header = (GTextField)GetChildAt(50);
            m_LDBack_Button = (UI_Back_Button)GetChildAt(51);
            m_LEnemies_Button = (UI_Back_Button)GetChildAt(52);
            m_LStats_Button = (UI_Back_Button)GetChildAt(53);
            m_LAbilities_Button = (UI_Back_Button)GetChildAt(54);
            m_LLevels_Button = (UI_Back_Button)GetChildAt(55);
            m_Levels = (GGroup)GetChildAt(56);
            m_Splash = GetTransitionAt(0);
        }
    }
}