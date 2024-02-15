using Avalonia.Media;
using FEHamarr.HSDArc;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace FEHamarr.ViewModels
{
    public class PersonSelectWindowViewModel : ViewModelBase
    {
        public ObservableCollection<PersonViewModel> FilteredPersons { get; } = new();
        PersonViewModel? _selectedPerson;
        IEnumerable<Person> _personPool;
        public PersonViewModel? SelectedPerson { 
            get => _selectedPerson; 
            set => this.RaiseAndSetIfChanged(ref _selectedPerson, value);
        }

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SearchCommand { get; }
        public ReactiveCommand<System.Reactive.Unit, PersonViewModel?> ConfirmCommand { get; }

        public PersonSelectWindowViewModel()
        {
            SearchCommand = ReactiveCommand.Create(DoSearch);
            ConfirmCommand = ReactiveCommand.Create(() => {
                return SelectedPerson;
            });

            _personPool = DataManager.Persons.OrderBy(p=>p.version_num).Reverse();
            foreach (var p in _personPool)
            {
                FilteredPersons.Add(new PersonViewModel(p));
            }


            _moveTypeIcons = DataManager.MoveTypeIcons.Values.ToArray();
            _weaponTypeIcons = DataManager.WeaponTypeIcons.Values.ToArray();
        }

        void DoSearch()
        {
            FilteredPersons.Clear();
            foreach (var p in _personPool)
            {
                if ((!IsMoveTypeSelectorEnabled || p.move_type == (MoveType)MoveType) && 
                    (!IsWeaponTypeSelectorEnabled || p.weapon_type == (WeaponType)WeaponType) &&
                    (!IsVersionSelectorEnabled || p.version_num == Version)) FilteredPersons.Add(new PersonViewModel(p));
            }
        }

        public bool IsVersionSelectorEnabled { get; set; } = false;
        uint _version = 0;
        public uint Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }
        public bool IsMoveTypeSelectorEnabled { get; set; } = false;
        int _moveType = -1;
        public int MoveType
        {
            get => _moveType;
            set => this.RaiseAndSetIfChanged(ref _moveType, value);
        }
        public bool IsWeaponTypeSelectorEnabled { get; set; } = false;
        int _weaponType = -1;
        public int WeaponType
        {
            get => _weaponType;
            set => this.RaiseAndSetIfChanged(ref _weaponType, value);
        }
        IImage[] _moveTypeIcons;
        IImage[] _weaponTypeIcons;
        public IImage[] MoveTypeIcons => _moveTypeIcons;
        public IImage[] WeaponTypeIcons => _weaponTypeIcons;
    }

    public class PersonViewModel : ViewModelBase {
        readonly Person _person;
        List<IImage> _skillIcons = new List<IImage>();

        public PersonViewModel(Person person)
        {
            _person = person;
            var list = _person.skills.ToList();
            list.Reverse();
            w = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.Weapon));
            h = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.Assist));
            if (!XString.IsEmpty(h)) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(h).icon));
            a = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.A));
            if (!XString.IsEmpty(a)) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(a).icon));
            b = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.B));
            if (!XString.IsEmpty(b)) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(b).icon));
            c = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.C));
            if (!XString.IsEmpty(c)) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(c).icon));
            x = list.Find(id => id != null && DataManager.CheckSkillCategory(id, SkillCategory.X));
            if (!XString.IsEmpty(x)) _skillIcons.Add(DataManager.GetSkillIcon((int)DataManager.GetSkill(x).icon));
                //w = ((XEnemy)person).top_weapon;

        }
        public Person Person => _person;
        public XString? w;
        public XString? h;
        public XString? a;
        public XString? b;
        public XString? c;
        public XString? x;
        public string Name => DataManager.GetMessage("M" + _person.id);
        public string Title
        {
            get
            {
                string body = DataManager.StripIdPrefix(_person.id, out string prefix);
                return DataManager.GetMessage("M" + prefix + "HONOR_" + body);
            }
        }
        public IImage MoveIcon => DataManager.GetMoveIcon((int)_person.move_type);
        public IImage AtkIcon => DataManager.GetWeaponIcon((int)_person.weapon_type);
        public uint Version => _person.version_num;

        public List<IImage> SkillIcons => _skillIcons;
    }
}
