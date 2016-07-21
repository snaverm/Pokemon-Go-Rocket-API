using System.Configuration;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AllEnum;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        public AuthType AuthType => (AuthType)Enum.Parse(typeof(AuthType), UserSettings.Default.AuthType);
        public string PtcUsername => UserSettings.Default.PtcUsername;
        public string PtcPassword => UserSettings.Default.PtcPassword;
        public double DefaultLatitude => UserSettings.Default.DefaultLatitude;
        public double DefaultLongitude => UserSettings.Default.DefaultLongitude;

        ICollection<KeyValuePair<ItemId, int>> ISettings.itemRecycleFilter
        {
            get
            {
                ICollection<KeyValuePair<ItemId, int>> itemFilter = new Dictionary<ItemId, int>();

                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemPokeBall, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemGreatBall, 50));
                //// itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemUltraBall, 0));
                //// itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemMasterBall, 0));

                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemPotion, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemSuperPotion, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemHyperPotion, 20));
                //// itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemMaxPotion, 0));

                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemRevive, 10));
                // itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemMaxRevive, 0));

                // itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemLuckyEgg, 0));

                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemIncenseOrdinary, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemIncenseSpicy, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemIncenseCool, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemIncenseFloral, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemTroyDisk, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemXAttack, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemXDefense, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemXMiracle, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemRazzBerry, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemBlukBerry, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemNanabBerry, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemWeparBerry, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemPinapBerry, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemSpecialCamera, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemIncubatorBasic, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 0));
                //itemFilter.Add(new KeyValuePair<AllEnum.ItemId, int>(ItemId.ItemItemStorageUpgrade, 0));

                return itemFilter;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string GoogleRefreshToken
        {
            get { return UserSettings.Default.GoogleRefreshToken; }
            set
            {
                UserSettings.Default.GoogleRefreshToken = value;
                UserSettings.Default.Save();
            }
        }
    }
}
