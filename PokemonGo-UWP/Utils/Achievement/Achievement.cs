
using System;
using Template10.Mvvm;

namespace PokemonGo_UWP.Utils {

    public enum Medal {
        None,
        Bronze,
        Silver,
        Gold
    }

    public class Achievement : BindableBase {
        public static Achievement Jogger = new Achievement(AchievementType.Jogger, 10.0f, 100.0f, 1000.0f);
        public static Achievement Kanto = new Achievement(AchievementType.Kanto, 5, 50, 100);
        public static Achievement Collector = new Achievement(AchievementType.Collector, 30, 500, 2000);
        public static Achievement Scientist = new Achievement(AchievementType.Scientist, 3, 20, 200);
        public static Achievement Breeder = new Achievement(AchievementType.Breeder, 10, 100, 1000);
        public static Achievement Backpacker = new Achievement(AchievementType.Backpacker, 100, 1000, 2000);
        public static Achievement Fisherman = new Achievement(AchievementType.Fisherman, 3, 50, 200);
        public static Achievement BattleGirl = new Achievement(AchievementType.BattleGirl, 10, 100, 1000);
        public static Achievement AceTrainer = new Achievement(AchievementType.AceTrainer, 10, 100, 1000);
        public static Achievement SchoolKid = new Achievement(AchievementType.SchoolKid, 10, 50, 200);
        public static Achievement BirdKeeper = new Achievement(AchievementType.BirdKeeper, 10, 50, 200);
        public static Achievement PunkGirl = new Achievement(AchievementType.PunkGirl, 10, 50, 200);
        public static Achievement BugCatcher = new Achievement(AchievementType.BugCatcher, 10, 50, 200);
        public static Achievement HexManiac = new Achievement(AchievementType.HexManiac, 10, 50, 200);
        public static Achievement Swimmer = new Achievement(AchievementType.Swimmer, 10, 50, 200);
        public static Achievement Gardener = new Achievement(AchievementType.Gardener, 10, 50, 200);
        public static Achievement Rocker = new Achievement(AchievementType.Rocker, 10, 50, 200);
        public static Achievement Psychic = new Achievement(AchievementType.Psychic, 10, 50, 200);
        public static Achievement Skier = new Achievement(AchievementType.Skier, 10, 50, 200);
        public static Achievement RuinManiac = new Achievement(AchievementType.RuinManiac, 10, 50, 200);
        public static Achievement Kindler = new Achievement(AchievementType.Kindler, 10, 50, 200);
        public static Achievement FairyTaleGirl = new Achievement(AchievementType.FairyTaleGirl, 10, 50, 200);
        public static Achievement DragonTamer = new Achievement(AchievementType.DragonTamer, 10, 50, 200);
        public static Achievement Youngster = new Achievement(AchievementType.Youngster, 3, 50, 200);
        public static Achievement DepotAgent = new Achievement(AchievementType.DepotAgent, 10, 50, 200);
        public static Achievement Hiker = new Achievement(AchievementType.Hiker, 10, 50, 200);
        public static Achievement BlackBelt = new Achievement(AchievementType.BlackBelt, 10, 50, 200);
        public static Achievement Pikachu = new Achievement(AchievementType.Pikachu, 10, 50, 200);

        private Achievement(AchievementType type, object bronze, object silver, object gold) {
            Type = type;
            Bronze = bronze;
            Silver = silver;
            Gold = gold;
        }

        public AchievementType Type { get; private set;}
        public object Bronze { get; private set; }
        public object Silver { get; private set; }
        public object Gold { get; private set; }

        private Medal _medal;
        public Medal Medal
        {
            get
            {
                return _medal;
            }
            private set
            {
                _medal = value;
                RaisePropertyChanged(nameof(Medal));
            }
        }
        public string TranslatedType
        {
            get
            {
                return Utils.Resources.Translation.GetString(Type.ToString());
            }
        }

        private object _value;
        public object Value {
            get
            {
                if (_value == null) {
                    return 0;
                }
                if(typeof(float) == _value.GetType()) {
                    return float.Parse(_value.ToString()).ToString("N1");
                } else {
                    return _value;
                }
            }
            set
            {
                _value = value;
                RaisePropertyChanged(nameof(Medal));
            }
        }

        public object NextValue {
            get
            {
                if(_value == null) {
                    return 0;
                }
                if (typeof(float) == _value.GetType()) {
                    if (float.Parse(_value.ToString()) < float.Parse(Bronze.ToString())) {
                        Medal = Medal.None;
                        return float.Parse(Bronze.ToString()).ToString("N1");
                    } else if(float.Parse(_value.ToString()) < float.Parse(Silver.ToString())) {
                        Medal = Medal.Bronze;
                        return float.Parse(Silver.ToString()).ToString("N1");
                    }else if(float.Parse(_value.ToString()) < float.Parse(Gold.ToString())) {
                        Medal = Medal.Silver;
                        return float.Parse(Gold.ToString()).ToString("N1");
                    }else {
                        Medal = Medal.Gold;
                        return float.Parse(Gold.ToString()).ToString("N1");
                    }
                } else {
                    if(int.Parse(_value.ToString()) < int.Parse(Bronze.ToString())) {
                        Medal = Medal.None;
                        return Bronze;
                    } else if(int.Parse(_value.ToString()) < int.Parse(Silver.ToString())) {
                        Medal = Medal.Bronze;
                        return Silver;
                    } else if(int.Parse(_value.ToString()) < int.Parse(Gold.ToString())) {
                        Medal = Medal.Silver;
                        return Gold;
                    } else {
                        Medal = Medal.Gold;
                        return Gold;
                    }
                }
            }
        }
    }
}
