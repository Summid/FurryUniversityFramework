using System.Collections.Generic;
namespace SFramework.Core.GameManagers
{
   public static partial class UISpriteManager
   {
      static UISpriteManager()
      {
          atlasSprite = new Dictionary<string, string>();
        atlasSprite.Add("A","Common");
        atlasSprite.Add("AddIcon","Common");
        atlasSprite.Add("B","Common");
        atlasSprite.Add("Bell","Common");
        atlasSprite.Add("blank128","Common");
        atlasSprite.Add("blank128Light","Common");
        atlasSprite.Add("blank32","Common");
        atlasSprite.Add("blank32Light","Common");
        atlasSprite.Add("blank64","Common");
        atlasSprite.Add("blank64Light","Common");
        atlasSprite.Add("BTN_A1","Common");
        atlasSprite.Add("BTN_A2","Common");
        atlasSprite.Add("BTN_A3","Common");
        atlasSprite.Add("BTN_Line_4PX_Large","Common");
        atlasSprite.Add("BTN_Line_4PX_Medium","Common");
        atlasSprite.Add("BTN_Line_4PX_Small","Common");
        atlasSprite.Add("ButtonLargeRound","Common");
        atlasSprite.Add("ButtonMediumRound","Common");
        atlasSprite.Add("ButtonSmallRound","Common");
        atlasSprite.Add("Button_Large_A","Common");
        atlasSprite.Add("Button_Medium_A","Common");
        atlasSprite.Add("Button_Small_A","Common");
        atlasSprite.Add("C","Common");
        atlasSprite.Add("CIRCLE2PXLAR","Common");
        atlasSprite.Add("CIRCLE2PXMED","Common");
        atlasSprite.Add("CIRCLE2PXSMALL","Common");
        atlasSprite.Add("CIRCLE4PXLAR","Common");
        atlasSprite.Add("CIRCLE4PXMED","Common");
        atlasSprite.Add("CIRCLE4PXSMA","Common");
        atlasSprite.Add("CloseBTN","Common");
        atlasSprite.Add("Coin","Common");
        atlasSprite.Add("D","Common");
        atlasSprite.Add("Divider","Common");
        atlasSprite.Add("DownArrow","Common");
        atlasSprite.Add("DownBTN","Common");
        atlasSprite.Add("E","Common");
        atlasSprite.Add("F","Common");
        atlasSprite.Add("Facebook","Common");
        atlasSprite.Add("Facebook2","Common");
        atlasSprite.Add("Gem","Common");
        atlasSprite.Add("Heart","Common");
        atlasSprite.Add("IconSets","Common");
        atlasSprite.Add("Info","Common");
        atlasSprite.Add("InputField","Common");
        atlasSprite.Add("Inventory","Common");
        atlasSprite.Add("LeftArrow","Common");
        atlasSprite.Add("LeftBTN","Common");
        atlasSprite.Add("Lock","Common");
        atlasSprite.Add("Messages","Common");
        atlasSprite.Add("Missions","Common");
        atlasSprite.Add("Music","Common");
        atlasSprite.Add("Ranks","Common");
        atlasSprite.Add("RightArrow","Common");
        atlasSprite.Add("RightBTN","Common");
        atlasSprite.Add("Settings","Common");
        atlasSprite.Add("Shop","Common");
        atlasSprite.Add("Sound","Common");
        atlasSprite.Add("Star","Common");
        atlasSprite.Add("Up","Common");
        atlasSprite.Add("UpArrow","Common");
        atlasSprite.Add("World","Common");
        atlasSprite.Add("X","Common");

      }

       public class Common : UIAtlasSpritesObject
        {
            public static readonly string A = "A";
            public static readonly string AddIcon = "AddIcon";
            public static readonly string B = "B";
            public static readonly string Bell = "Bell";
            public static readonly string blank128 = "blank128";
            public static readonly string blank128Light = "blank128Light";
            public static readonly string blank32 = "blank32";
            public static readonly string blank32Light = "blank32Light";
            public static readonly string blank64 = "blank64";
            public static readonly string blank64Light = "blank64Light";
            public static readonly string BTN_A1 = "BTN_A1";
            public static readonly string BTN_A2 = "BTN_A2";
            public static readonly string BTN_A3 = "BTN_A3";
            public static readonly string BTN_Line_4PX_Large = "BTN_Line_4PX_Large";
            public static readonly string BTN_Line_4PX_Medium = "BTN_Line_4PX_Medium";
            public static readonly string BTN_Line_4PX_Small = "BTN_Line_4PX_Small";
            public static readonly string ButtonLargeRound = "ButtonLargeRound";
            public static readonly string ButtonMediumRound = "ButtonMediumRound";
            public static readonly string ButtonSmallRound = "ButtonSmallRound";
            public static readonly string Button_Large_A = "Button_Large_A";
            public static readonly string Button_Medium_A = "Button_Medium_A";
            public static readonly string Button_Small_A = "Button_Small_A";
            public static readonly string C = "C";
            public static readonly string CIRCLE2PXLAR = "CIRCLE2PXLAR";
            public static readonly string CIRCLE2PXMED = "CIRCLE2PXMED";
            public static readonly string CIRCLE2PXSMALL = "CIRCLE2PXSMALL";
            public static readonly string CIRCLE4PXLAR = "CIRCLE4PXLAR";
            public static readonly string CIRCLE4PXMED = "CIRCLE4PXMED";
            public static readonly string CIRCLE4PXSMA = "CIRCLE4PXSMA";
            public static readonly string CloseBTN = "CloseBTN";
            public static readonly string Coin = "Coin";
            public static readonly string D = "D";
            public static readonly string Divider = "Divider";
            public static readonly string DownArrow = "DownArrow";
            public static readonly string DownBTN = "DownBTN";
            public static readonly string E = "E";
            public static readonly string F = "F";
            public static readonly string Facebook = "Facebook";
            public static readonly string Facebook2 = "Facebook2";
            public static readonly string Gem = "Gem";
            public static readonly string Heart = "Heart";
            public static readonly string IconSets = "IconSets";
            public static readonly string Info = "Info";
            public static readonly string InputField = "InputField";
            public static readonly string Inventory = "Inventory";
            public static readonly string LeftArrow = "LeftArrow";
            public static readonly string LeftBTN = "LeftBTN";
            public static readonly string Lock = "Lock";
            public static readonly string Messages = "Messages";
            public static readonly string Missions = "Missions";
            public static readonly string Music = "Music";
            public static readonly string Ranks = "Ranks";
            public static readonly string RightArrow = "RightArrow";
            public static readonly string RightBTN = "RightBTN";
            public static readonly string Settings = "Settings";
            public static readonly string Shop = "Shop";
            public static readonly string Sound = "Sound";
            public static readonly string Star = "Star";
            public static readonly string Up = "Up";
            public static readonly string UpArrow = "UpArrow";
            public static readonly string World = "World";
            public static readonly string X = "X";

        }

   }
}