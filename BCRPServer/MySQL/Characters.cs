using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static void CharacterAdd(PlayerData.PlayerInfo pInfo)
        {
            MySqlCommand cmd = new MySqlCommand();

            cmd.CommandText = @"INSERT INTO characters (ID, AID, CreationDate, AdminLevel, LastJoinDate, IsOnline, TimePlayed, 
                    Name, Surname, Sex, BirthDate, Licenses, Fraction, OrgID, Cash, PhoneNumber, PhoneBalance, LastData, Familiars, Skills) 
                    VALUES (@ID, @AID, @CreationDate, @AdminLevel, @LastJoinDate, @IsOnline, @TimePlayed, 
                    @Name, @Surname, @Sex, @BirthDate, @Licenses, @Fraction, @OrgID, @Cash, @PhoneNumber, @PhoneBalance, @LastData, @Familiars, @Skills); 

                    INSERT INTO customizations (ID, HeadBlend, HeadOverlays, FaceFeatures, Decorations, HairStyle, EyeColor) VALUES (@ID, @HeadBlend, @HeadOverlays, @FaceFeatures, @Decorations, @HairStyle, @EyeColor); 

                    INSERT INTO inventories (ID, Items, Clothes, Accessories, Bag, Holster, Weapons, Armour) VALUES (@ID, @Items, @Clothes, @Accessories, @Bag, @Holster, @Weapons, @Armour);
                    
                    INSERT INTO achievements (ID) VALUES (@ID); INSERT INTO quests (ID) VALUES (@ID);";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@AID", pInfo.AID);

            cmd.Parameters.AddWithValue("@IsOnline", true);
            cmd.Parameters.AddWithValue("@TimePlayed", pInfo.TimePlayed);
            cmd.Parameters.AddWithValue("@Fraction", pInfo.Fraction);
            cmd.Parameters.AddWithValue("@OrgID", pInfo.OrganisationID);

            cmd.Parameters.AddWithValue("@CreationDate", pInfo.CreationDate);
            cmd.Parameters.AddWithValue("@AdminLevel", pInfo.AdminLevel);
            cmd.Parameters.AddWithValue("@LastJoinDate", pInfo.LastJoinDate);
            cmd.Parameters.AddWithValue("@Cash", pInfo.Cash);
            cmd.Parameters.AddWithValue("@LastData", pInfo.LastData.SerializeToJson());

            cmd.Parameters.AddWithValue("@Name", pInfo.Name);
            cmd.Parameters.AddWithValue("@Surname", pInfo.Surname);
            cmd.Parameters.AddWithValue("@Sex", pInfo.Sex);
            cmd.Parameters.AddWithValue("@BirthDate", pInfo.BirthDate);

            cmd.Parameters.AddWithValue("@PhoneNumber", pInfo.PhoneNumber);
            cmd.Parameters.AddWithValue("@PhoneBalance", pInfo.PhoneBalance);

            cmd.Parameters.AddWithValue("@Licenses", JsonConvert.SerializeObject(pInfo.Licenses));

            cmd.Parameters.AddWithValue("@Skills", JsonConvert.SerializeObject(pInfo.Skills));

            cmd.Parameters.AddWithValue("@Familiars", JsonConvert.SerializeObject(pInfo.Familiars));

            cmd.Parameters.AddWithValue("@Items", pInfo.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Clothes", pInfo.Clothes.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Accessories", pInfo.Accessories.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Weapons", pInfo.Weapons.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Armour", pInfo.Armour?.UID ?? 0);
            cmd.Parameters.AddWithValue("@Bag", pInfo.Bag?.UID ?? 0);
            cmd.Parameters.AddWithValue("@Holster", pInfo.Holster?.UID ?? 0);

            cmd.Parameters.AddWithValue("@HeadBlend", pInfo.HeadBlend.SerializeToJson());
            cmd.Parameters.AddWithValue("@HeadOverlays", pInfo.HeadOverlays.SerializeToJson());
            cmd.Parameters.AddWithValue("@FaceFeatures", pInfo.FaceFeatures.SerializeToJson());
            cmd.Parameters.AddWithValue("@Decorations", pInfo.Decorations.SerializeToJson());
            cmd.Parameters.AddWithValue("@HairStyle", pInfo.HairStyle.SerializeToJson());
            cmd.Parameters.AddWithValue("@EyeColor", pInfo.EyeColor);

            PushQuery(cmd);
        }

        public static void CharacterUpdateOnEnter(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET IsOnline=true, LastJoinDate=@LastJoinDate WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@LastJoinDate", pInfo.LastJoinDate);

            PushQuery(cmd);
        }

        public static void CharacterSaveOnExit(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET IsOnline=false, TimePlayed=@TimePlayed, LastData=@LastData, Familiars=@Familiars WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@TimePlayed", pInfo.TimePlayed);
            cmd.Parameters.AddWithValue("@LastData", pInfo.LastData.SerializeToJson());
            cmd.Parameters.AddWithValue("@Familiars", pInfo.Familiars.SerializeToJson());

            PushQuery(cmd);

            CharacterItemsUpdate(pInfo);
            CharacterClothesUpdate(pInfo);
            CharacterAccessoriesUpdate(pInfo);
            CharacterWeaponsUpdate(pInfo);
            CharacterHolsterUpdate(pInfo);
            CharacterBagUpdate(pInfo);
            CharacterArmourUpdate(pInfo);
        }

        public static void CharacterCustomizationFullUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE customizations SET HeadBlend=@HBlend, HeadOverlays=@HOverlays, FaceFeatures=@FFeatures, EyeColor=@EColor WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@HBlend", pInfo.HeadBlend.SerializeToJson());
            cmd.Parameters.AddWithValue("@HOverlays", pInfo.HeadOverlays.SerializeToJson());
            cmd.Parameters.AddWithValue("@FFeatures", pInfo.FaceFeatures.SerializeToJson());
            cmd.Parameters.AddWithValue("@EColor", pInfo.EyeColor);

            PushQuery(cmd);
        }

        public static void CharacterHeadOverlaysUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE customizations SET HeadOverlays=@HOverlays WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@HOverlays", pInfo.HeadOverlays.SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterHairStyleUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE customizations SET HairStyle=@HStyle WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@HStyle", pInfo.HairStyle.SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterDecorationsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE customizations SET Decorations=@Decors WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@Decors", pInfo.Decorations.SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterCashUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET Cash=@Cash WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@Cash", pInfo.Cash);

            PushQuery(cmd);
        }

        public static void CharacterPhoneBalanceUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET PhoneBalance=@PB WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@PB", pInfo.PhoneBalance);

            PushQuery(cmd);
        }

        public static void CharacterPhoneNumberUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET PhoneNumber=@PN WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@PN", pInfo.PhoneNumber);

            PushQuery(cmd);
        }

        public static void CharacterItemsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Items=@Items WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Items", pInfo.Items.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterClothesUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Clothes=@Clothes WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Clothes", pInfo.Clothes.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterAccessoriesUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Accessories=@Accessories WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Accessories", pInfo.Accessories.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterWeaponsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Weapons=@Weapons WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Weapons", pInfo.Weapons.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterBagUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Bag=@Bag WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Bag", pInfo.Bag?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void CharacterHolsterUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Holster=@Holster WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Holster", pInfo.Holster?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void CharacterArmourUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Armour=@Armour WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Armour", pInfo.Armour?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void CharacterFurnitureUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Furniture=@Furniture WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Furniture", pInfo.Furniture.Select(x => x.UID).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterWeaponSkinsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET WSkins=@WSkins WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@WSkins", pInfo.WeaponSkins.ToDictionary(x => x.Key, x => x.Value.UID).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterSkillsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET Skills=@Skills WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Skills", pInfo.Skills.SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterAchievementUpdate(PlayerData.PlayerInfo pInfo, PlayerData.Achievement achievement)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE achievements SET {achievement.Type.ToString()}=@Data WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Data", achievement.SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterQuestUpdate(PlayerData.PlayerInfo pInfo, Sync.Quest.QuestData.Types type, Sync.Quest quest)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE quests SET {type.ToString()}=@Data WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            if (quest != null)
                cmd.Parameters.AddWithValue("@Data", quest.SerializeToJson());
            else
                cmd.Parameters.AddWithValue("@Data", DBNull.Value);

            PushQuery(cmd);
        }

        public static void CharacterMedicalCardUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE characters SET MedicalCard=@MedCard WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            if (pInfo.MedicalCard == null)
                cmd.Parameters.AddWithValue("@MedCard", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@MedCard", pInfo.MedicalCard.SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterContactsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE characters SET Contacts=@Cont WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Cont", pInfo.Contacts.SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterPhoneBlacklistUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE characters SET PhoneBL=@PBL WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@PBL", pInfo.PhoneBlacklist.SerializeToJson());

            PushQuery(cmd);
        }
    }
}
