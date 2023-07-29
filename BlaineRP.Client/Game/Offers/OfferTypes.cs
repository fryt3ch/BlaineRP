namespace BlaineRP.Client.Game.Offers
{
    public enum OfferTypes
    {
        /// <summary>Рукопожатие</summary>
        Handshake = 0,

        /// <summary>Обмен</summary>
        Exchange,

        /// <summary>Нести игркока</summary>
        Carry,

        /// <summary>Сыграть в орел и решка</summary>
        HeadsOrTails,

        /// <summary>Приглашение во фракцию</summary>
        InviteFraction,

        /// <summary>Приглашение в организацию</summary>
        InviteOrganisation,

        /// <summary>Передать наличные</summary>
        Cash,

        /// <summary>Показать паспорт</summary>
        ShowPassport,

        /// <summary>Показать мед. карту</summary>
        ShowMedicalCard,

        /// <summary>Показать лицензии</summary>
        ShowLicenses,

        /// <summary>Показать тех. паспорт</summary>
        ShowVehiclePassport,

        /// <summary>Показать резюме</summary>
        ShowResume,

        /// <summary>Показать удостоверение</summary>
        ShowFractionDocs,

        /// <summary>Продажа имущества</summary>
        PropertySell,

        /// <summary>Поделиться меткой</summary>
        WaypointShare,

        /// <summary>Подселить в дом/квартиру</summary>
        Settle,

        /// <summary>Продать недвижимость</summary>
        SellEstate,

        /// <summary>Продать транспорт</summary>
        SellVehicle,

        /// <summary>Продать бизнес</summary>
        SellBusiness,

        /// <summary>Штраф полиции</summary>
        PoliceFine,

        /// <summary>Лечение от врача</summary>
        EmsHeal,

        /// <summary>Лечение (психики) от врача</summary>
        EmsPsychHeal,

        /// <summary>Лечение (наркозавимиости) от врача</summary>
        EmsDrugHeal,

        /// <summary>Проверка здоровья от врача</summary>
        EmsDiagnostics,

        /// <summary>Выдача мед. карты от врача</summary>
        EmsMedicalCard,

        /// <summary>Продажа мед. маски от врача</summary>
        EmsSellMask,

        /// <summary>Использовать мед. предмет для лечения</summary>
        GiveHealingItem,
    }
}