﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class Notifications
        {
            public static class Vehicles
            {
                public static string Header = "Транспорт";

                public static class Push
                {
                    public static string EngineOn = "Двигатель не заглушен!";
                }

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

                public static class GPS
                {
                    public static string Header = "Навигатор";

                    public static string RouteReady = "Маршрут проложен!";
                    public static string RouteCancel = "Маршрут отменён!";
                }

                public static class Engine
                {
                    public static string On = "Двигатель запущен!";
                    public static string Off = "Двигатель заглушен!";
                    public static string OutOfFuel = "Закончилось топливо!";
                }

                public static class Doors
                {
                    public static string AlreadyLocked = "Двери уже заблокированы!";
                    public static string AlreadyUnlocked = "Двери уже разблокированы!";
                    public static string Locked = "Двери заблокированы!";
                    public static string Unlocked = "Двери разлокированы!";
                }

                public static class Trunk
                {
                    public static string AlreadyLocked = "Багажник уже закрыт!";
                    public static string AlreadyUnlocked = "Багажник уже открыт!";
                    public static string Locked = "Багажник закрыт!";
                    public static string Unlocked = "Багажник открыт!";

                    public static string NoTrunk = "В этом транспорте нет багажника!";
                    public static string NoPhysicalTrunk = "В этом транспорте нет физического багажника!";
                }

                public static class Hood
                {
                    public static string AlreadyLocked = "Капот уже закрыт!";
                    public static string AlreadyUnlocked = "Капот уже открыт!";
                    public static string Locked = "Капот закрыт!";
                    public static string Unlocked = "Капот открыт!";
                }

                public static class Passengers
                {
                    public static string None = "Нет ни одного пассажира!";
                    public static string SomeoneSeating = "Кто-то уже сидит на этом месте!";
                    public static string IsDriver = "Пересаживаться могут только пассажиры!";

                    public static string NotEnterable = "В этот транспорт нельзя садиться!";
                }

                public static class Park
                {
                    public static string Success = "Вы успешно припарковали свой транспорт!";
                    public static string WrongPlace = "Здесь нельзя парковаться!";
                    public static string NotAllowed = "Этот транспорт нельзя парковать!";

                    public static string Warning = "Ваше Т/С не припарковано!\nОно может оказаться на штрафстоянке";
                }

                public static string FullOfGasDef = "Бак вашего Т/С уже полон!";
                public static string FullOfGasElectrical = "Ваше Т/С уже полностью заряжено!";

                public static string NotAtGasStationError = "Вы не на заправке!";
                public static string InVehicleError = "Выйдите из транспорта, чтобы заправить его!";

                public static string NotAllowed = "У вас нет ключей от этого транспорта!";

                public static string NoPlate = "На этом транспорте не установлен номерной знак!";
                public static string PlateExists = "На этом транспорте уже установлен номерной знак!\nДля начала нужно его снять";
                public static string PlateInstalled = "Номерной знак [{0}] установлен!";

                public static string NoOwnedVehicles = "Вы не владеете ни одним транспортом!";

                public static string VehicleOnPound = "Этот транспорт находится на штрафстоянке!";
                public static string VehicleKeyError = "Этот ключ не работает!";
                public static string VehicleKeyNoSignalError = "Отсутвует связь между ключом и транспортом! Попробуйте позже.";

                public static string VehicleIsDeadFixError = "Этот транспорт слишком сильно поврежден, вызовите механика!";
                public static string VehicleIsNotDamagedFixError = "Этот транспорт не поврежден!";

                public static string AlreadyHaveRentedVehicle = "Вы уже арендуете какой-либо транспорт! Откажитесь от аренды текущего транспорта и попробуйте снова.";

                public static string RentedVehicleTimeLeft = "{0} арендован вами, если вы не вернетесь в него в течение {1}, то аренда будет отменена!";

                public static string BoatTrailerNotNearWater = "Поблизости лодки либо нет водоема, либо он недостаточно глубокий!";
            }
        }
    }
}