using Avalonia.Media;
using Avalonia.Media.Imaging;
using FEHamarr.FEHArchive;
using FEHamarr.HSDArc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public static List<HSDArcSkills> SkillArcs = new List<HSDArcSkills>();
        public static List<HSDArcPersons> PersonArcs = new List<HSDArcPersons>();
        public static List<HSDArcMessages> MsgArcs = new List<HSDArcMessages>();

        public static List<uint> Versions;
        public static Bitmap[] ICON_ATLAS;
        public static Bitmap STATUS;
        public static Dictionary<int, IImage> SkillIcons = new Dictionary<int, IImage>();
        public static Dictionary<int, IImage> WeaponTypeIcons = new Dictionary<int, IImage>();
        public static Dictionary<int, IImage> MoveTypeIcons = new Dictionary<int, IImage>();

        static List<Person> persons;
        static List<Skill> skills;

        static IEnumerable<string> SkillArcNames => SkillArcs.Select(arc => arc.FilePath);
        static IEnumerable<string> PersonArcNames => PersonArcs.Select(arc => arc.FilePath);
        static IEnumerable<string> MsgArcNames => MsgArcs.Select(arc => arc.FilePath);
        public static List<Person> Persons { get
            {
                if (persons != null) return persons;
                List<Person> ps = new();
                foreach (var arc in PersonArcs)
                {
                    foreach (var p in arc.list.items.Values)
                    {
                        ps.Add(p);
                    }
                }
                persons = ps;
                return ps;
            } 
        }

        public static List<Skill> Skills
        {
            get
            {
                if (skills != null) return skills;
                List<Skill> ss = new();
                foreach (var arc in SkillArcs)
                {
                    foreach (var s in arc.list.items.Values)
                    {
                        ss.Add(s);
                    }
                }
                skills = ss;
                return ss;
            }
        }

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
                using (var rd = new FEHArcReader(msg))
                {
                    HSDArcMessages arc = new();
                    rd.ReadMsgs(arc);
                    MsgArcs.Add(arc);
                }
                
            }
            foreach (string skill in Directory.GetFiles(SKL_PATH, DATAEXT))
            {
                using (var rd = new FEHArcReader(skill))
                {
                    HSDArcSkills arc = new();
                    rd.ReadSkills(arc);
                    SkillArcs.Add(arc);
                }
            }
            foreach (string person in Directory.GetFiles(PERSON_PATH, DATAEXT))
            {
                using (var rd = new FEHArcReader(person))
                {
                    HSDArcPersons arc = new();
                    rd.ReadPersons(arc);
                    PersonArcs.Add(arc);
                }
                    
            }

            //Versions = Persons.Values.Select<Person, uint>(p => p.version_num).Distinct().ToList();
            //Versions.Sort();
            //Versions.Reverse();

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
        public static Skill? GetSkill(string id, string? arcName = null)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!string.IsNullOrEmpty(arcName))
            {
                var arc = SkillArcs.Find(arc => arc.FilePath == arcName);
                if (arc is not null)
                {
                    if (arc.list.items.TryGetValue(id, out Skill skill)) return skill;
                }
                return null;
            }
            foreach (var arc in SkillArcs)
            {
                if (arc.list.items.TryGetValue(id, out Skill skill)) return skill;
            }
            return null;
        }

        public static Person? GetPerson(string id, string? arcName = null)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!string.IsNullOrEmpty(arcName))
            {
                var arc = PersonArcs.Find(arc => arc.FilePath == arcName);
                if (arc is not null)
                {
                    if (arc.list.items.TryGetValue(id, out Person person)) return person;
                }
                return null;
            }
            foreach (var arc in PersonArcs)
            {
                if (arc.list.items.TryGetValue(id, out Person p)) return p;
            }
            return null;
        }

        public static string GetMessage(string id)
        {
            foreach (var arc in MsgArcs)
            {
                if (arc.items.TryGetValue(id, out string value)) return value;
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
