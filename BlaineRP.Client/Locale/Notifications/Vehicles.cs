namespace BlaineRP.Client
{
    public static partial class Locale
    {
        public static partial class Notifications
        {
            public static class Vehicles
            {
                public static string Header = "Транспорт";

                public static string FullOfGasDef = "Бак вашего Т/С уже полон!";
                public static string FullOfGasElectrical = "Ваше Т/С уже полностью заряжено!";

                public static string NotAtGasStationError = "Вы не на заправке и у Вас нет предметов, которыми можно заправить транспорт!";
                public static string InVehicleError = "Выйдите из транспорта, чтобы заправить его!";

                public static string NotAllowed = "У вас нет ключей от этого транспорта!";

                public static string NotFullOwner = "Вы не являетесь владельцем этого транспорта!";

                public static string NoPlate = "На этом транспорте не установлен номерной знак!";
                public static string PlateExists = "На этом транспорте уже установлен номерной знак!\nДля начала нужно его снять";
                public static string PlateInstalled = "Номерной знак [{0}] установлен!";

                public static string NoOwnedVehicles = "Вы не владеете ни одним транспортом!";

                public static string VehicleOnPound = "Этот транспорт находится на штрафстоянке!";
                public static string VehicleKeyError = "Этот ключ не работает!";
                public static string VehicleKeyNoSignalError = "Отсутствует связь с транспортом, попробуйте позже!";

                public static string VehicleIsDeadFixError = "Этот транспорт слишком сильно поврежден, вызовите механика!";
                public static string VehicleIsNotDamagedFixError = "Этот транспорт не поврежден!";

                public static string AlreadyHaveRentedVehicle = "Вы уже арендуете какой-либо транспорт! Откажитесь от аренды текущего транспорта и попробуйте снова.";

                public static string RentedVehicleTimeLeft = "{0} арендован вами, если вы не вернетесь в него в течение {1}, то аренда будет отменена!";

                public static string BoatTrailerNotNearWater = "Поблизости лодки либо нет водоема, либо он недостаточно глубокий!";

                public static class Additional
                {
                    public static string HeaderCruise = "Круиз-контроль";
                    public static string HeaderAutoPilot = "Автопилот";

                    public static string On = "Система активирована!";
                    public static string Off = "Система деактивирована!";

                    public static string Reverse = "Недоступно при движении задним ходом!";
                    public static string MinSpeed = "Минимальная скорость - {0} км/ч!";
                    public static string MaxSpeed = "Максимальная скорость - {0} км/ч!";
                    public static string Danger = "Обнаружен опасный манёвр";
                    public static string Collision = "Обнаружено столкновение!";
                    public static string Invtervention = "Обнаружено вмешательство в управление!";

                    public static string Unsupported = "В текущем транспорте нет этой системы!";
                }

                public static class SeatBelt
                {
                    public static string Header = "Ремень безопасности";

                    public static string TakeOffToLeave = "Вы пристегнуты!\nОтстегните ремень, чтобы выйти";
                    public static string TakeOffToSeat = "Вы пристегнуты!\nОтстегните ремень, чтобы пересесть";
                }

                public static class Park
                {
                    public static string Success = "Вы успешно припарковали свой транспорт!";
                    public static string WrongPlace = "Здесь нельзя парковаться!";
                    public static string NotAllowed = "Этот транспорт нельзя парковать!";

                    public static string Warning = "Ваше Т/С не припарковано!\nОно может оказаться на штрафстоянке";
                }
            }
        }
    }
}