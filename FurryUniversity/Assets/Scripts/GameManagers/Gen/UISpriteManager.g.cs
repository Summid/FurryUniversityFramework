using System.Collections.Generic;
namespace SFramework.Core.GameManager
{
   public static partial class UISpriteManager
   {
      static UISpriteManager()
      {
          atlasSprite = new Dictionary<string, string>();
        atlasSprite.Add("Discipleship0","Discipleship");
        atlasSprite.Add("Discipleship1","Discipleship");
        atlasSprite.Add("Discipleship2","Discipleship");
        atlasSprite.Add("Duality0","Duality");
        atlasSprite.Add("Duality1","Duality");
        atlasSprite.Add("Duality2","Duality");
        atlasSprite.Add("KingsFall1","KingsFall");
        atlasSprite.Add("KingsFall2","KingsFall");

      }

       public class Discipleship : UIAtlasSpritesObject
        {
            public static readonly string Discipleship0 = "Discipleship0";
            public static readonly string Discipleship1 = "Discipleship1";
            public static readonly string Discipleship2 = "Discipleship2";

        }

       public class Duality : UIAtlasSpritesObject
        {
            public static readonly string Duality0 = "Duality0";
            public static readonly string Duality1 = "Duality1";
            public static readonly string Duality2 = "Duality2";

        }

       public class KingsFall : UIAtlasSpritesObject
        {
            public static readonly string KingsFall1 = "KingsFall1";
            public static readonly string KingsFall2 = "KingsFall2";

        }

   }
}