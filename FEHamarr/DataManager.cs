using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.X11;
using FEHamarr.FEHArchive;
using FEHamarr.SerializedData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEHamarr
{
    public class DataManager
    {
        private static readonly string DATAEXT = "*.lz";
        public static readonly string MSG_PATH = @"Data\MSG\";
        public static readonly string SKL_PATH = @"Data\SRPG\Skill\";
        public static readonly string PERSON_PATH = @"Data\SRPG\Person\";
        public static readonly string ENEMY_PATH = @"Data\SRPG\Enemy\";
        public static readonly string FACE_PATH = @"Data\FACE\";
        public static readonly string FIELD_PATH = @"Data\Field\";
        public static readonly string UI_PATH = @"Data\UI\";

        public static Dictionary<string, string> Messages = new Dictionary<string, string>();
        public static Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();
        public static Dictionary<string, Person> Persons = new Dictionary<string, Person>();
        public static List<uint> Versions;
        public static Bitmap[] ICON_ATLAS;
        public static Bitmap STATUS;
        public static Dictionary<int, IImage> SkillIcons = new Dictionary<int, IImage>();
        public static Dictionary<int, IImage> WeaponTypeIcons = new Dictionary<int, IImage>();
        public static Dictionary<int, IImage> MoveTypeIcons = new Dictionary<int, IImage>();

        public static string StripIdPrefix(string id, out string prefix)
        {
            var split = id.Split('_', 2);
            prefix = split[0] + "_";
            return split[1];
        }
        public static void Init()
        {
            foreach (string msg in Directory.GetFiles(MSG_PATH, DATAEXT))
            {
                FEHArcMsg arc = new FEHArcMsg(msg);
                var items = arc.GetData();
                foreach (var item in items) Messages.TryAdd(item.Key, item.Value);
            }
            foreach (string skl in Directory.GetFiles(SKL_PATH, DATAEXT))
            {
                FEHArcSkill arc = new FEHArcSkill(skl);
                var items = arc.GetData();
                foreach (var item in items) Skills.TryAdd(item.Key, item.Value);
            }
            foreach (string person in Directory.GetFiles(PERSON_PATH, DATAEXT))
            {
                FEHArcPerson arc = new FEHArcPerson(person);
                var items = arc.GetData();
                foreach (var item in items) Persons.TryAdd(item.Key, item.Value);
            }
            
            foreach (string enemy in Directory.GetFiles(ENEMY_PATH, DATAEXT))
            {
                FEHArcEnemy arc = new FEHArcEnemy(enemy);
                var items = arc.GetData();
                foreach (var item in items) Persons.TryAdd(item.Key, item.Value);
            }
            Versions = Persons.Values.Select<Person, uint>(p => p.version_num).Distinct().ToList();
            Versions.Sort();
            Versions.Reverse();
            
            InitImage();
        }

        static void InitImage()
        {
            STATUS = new Bitmap(UI_PATH + "Status.png");
            var atlas = (new DirectoryInfo(UI_PATH)).GetFiles("Skill_Passive*.png");
            ICON_ATLAS = new Bitmap[atlas.Length];
            for (int i = 0; i < ICON_ATLAS.Length; i++)
            {
                ICON_ATLAS[i] = new Bitmap(UI_PATH + "Skill_Passive" + (i + 1) + ".png");
            }
            for (int i = 0; i <= (int)WeaponType.ColorlessBeast; i++) GetWeaponIcon(i);
            for (int i = 0; i <= (int)MoveType.Flying; i++) GetMoveIcon(i);
        }
        public static Skill GetSkill(string id)
        {
            if (id == null) return null;
            return Skills.GetValueOrDefault(id);
        }

        public static Person GetPerson(string id)
        {
            return Persons.GetValueOrDefault(id);
        }

        public static string GetMessage(string id)
        {
            if (Messages.TryGetValue(id, out string ct))
            {
                return ct;
            }
            return string.Empty;
        }

        public static bool CheckSkillCategory(string id, SkillCategory cat)
        {
            if (id == null) return false;
            Skill sk = GetSkill(id);
            if (sk != null)
            {
                return sk.category == cat;
            }
            return false;
        }

        public static IImage GetSkillIcon(int id)
        {
            if (SkillIcons.TryGetValue(id, out var icon)) { return icon; }
            else
            {
                int pic = id / 169;
                if (pic > ICON_ATLAS.Length) { pic = 0; id = 1; }
                int pos = id % 169;
                int row = pos / 13;
                int col = pos - row * 13;
                var cropped = new CroppedBitmap(ICON_ATLAS[pic], new Avalonia.PixelRect(col * 76, row * 76, 76, 76));
                SkillIcons.Add(id, cropped);
                return cropped;
            }
        }

        public static IImage GetMoveIcon(int id)
        {
            if (MoveTypeIcons.TryGetValue(id, out var icon)) { return icon; }
            else
            {   
                CroppedBitmap cropped = new CroppedBitmap(STATUS, new Avalonia.PixelRect(353 + 55 * id, 413, 55, 55));
                MoveTypeIcons.Add(id, cropped);
                return cropped;
            }
            

        }

        public static IImage GetWeaponIcon(int id)
        {
            if (WeaponTypeIcons.TryGetValue(id, out var icon)) { return icon; }
            else
            {
                var cropped = new CroppedBitmap(STATUS, new Avalonia.PixelRect(1 + 56 * id, 205, 56, 56));
                WeaponTypeIcons.Add(id, cropped);
                return cropped;
            }
              
        }
    }


}
