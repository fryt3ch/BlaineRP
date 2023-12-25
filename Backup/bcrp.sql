-- phpMyAdmin SQL Dump
-- version 5.2.0
-- https://www.phpmyadmin.net/
--
-- Хост: 127.0.0.1
-- Время создания: Июл 29 2023 г., 16:51
-- Версия сервера: 10.4.27-MariaDB
-- Версия PHP: 8.2.0

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `bcrp`
--

-- --------------------------------------------------------

--
-- Структура таблицы `accounts`
--

CREATE TABLE `accounts` (
  `ID` int(11) UNSIGNED NOT NULL COMMENT 'Порядковый номер аккаунта',
  `SCID` bigint(20) UNSIGNED NOT NULL COMMENT 'SocialClub ID',
  `HWID` text DEFAULT NULL COMMENT 'HardwareID',
  `Login` text DEFAULT NULL COMMENT 'Логин',
  `Password` text DEFAULT NULL COMMENT 'Пароль',
  `Mail` text DEFAULT NULL COMMENT 'Почта',
  `RegDate` datetime NOT NULL DEFAULT current_timestamp() COMMENT 'Дата регистрации',
  `RegIP` text DEFAULT NULL COMMENT 'Регистрационный IP',
  `LastIP` text DEFAULT NULL COMMENT 'Последний IP',
  `AdminLevel` int(11) NOT NULL DEFAULT 0 COMMENT 'Уровень глобального администратора',
  `BCoins` int(11) UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Донат баланс'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Дамп данных таблицы `accounts`
--

INSERT INTO `accounts` (`ID`, `SCID`, `HWID`, `Login`, `Password`, `Mail`, `RegDate`, `RegIP`, `LastIP`, `AdminLevel`, `BCoins`) VALUES
(1, 223694143, 'E1AEB2381EDE5078C2FE31407D7A0FD0F11E12187E2E5068E22E61408D2A1FA0018E72F8DE7E5058025E91409DDA2F7011FED2D83ECE5048228EC140AD8A3F40', 'frytech', '$2b$05$ZK73/iB7evDIpwhvvOkf2ezrvMTcvu7FHGZdbpvuMSB7H9B7UsJ7O', 'frytech228@gmail.com', '2022-06-12 13:35:12', '127.0.0.1', '127.0.0.1', -1, 0),
(2, 67704536, 'D8903A045BE8AFB812502C18F7D22C10B91831709C3CA1F018500A1C438807E042108C48BF3A0CA891383990BCAC49E05810DA342B285F0872D0EC7887004F80', 'frytech123', '$2b$05$urfXGh7/pHGae/l9SWwXYO2KV6mCjCbN/zfhFep.49.GXsrVd0avO', 'frytech@mail.ru', '2022-06-17 13:59:46', '10.16.176.226', '192.168.0.106', -1, 0),
(3, 106593526, 'D7B088B474FA4DE8B99AEF0422AE942099F6F5E0B85E7CE813F22B7C190A1620AC643BEC1310E80884D6D9FC9D5E43100A92C4A8DDE2314018B28CC49F3EB500', 'qwerty', '$2b$05$6rP4472vuRnAfBGriDR/V.Q50romwbstxpZim67xKmpsrs.KVzhmy', '1@gmail.com', '2022-07-14 11:27:41', '89.179.104.17', '90.154.70.13', -1, 0);

-- --------------------------------------------------------

--
-- Структура таблицы `global_blacklist`
--

CREATE TABLE `global_blacklist` (
  `ID` int(11) NOT NULL,
  `HWID` text DEFAULT NULL,
  `SCID` bigint(20) UNSIGNED NOT NULL DEFAULT 0,
  `Date` datetime NOT NULL,
  `Reason` text NOT NULL DEFAULT '',
  `AdminID` int(10) UNSIGNED NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Дамп данных таблицы `global_blacklist`
--

INSERT INTO `global_blacklist` (`ID`, `HWID`, `SCID`, `Date`, `Reason`, `AdminID`) VALUES
(1, NULL, 412312, '2023-07-13 16:57:49', 'prost', 1);

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`ID`);

--
-- Индексы таблицы `global_blacklist`
--
ALTER TABLE `global_blacklist`
  ADD PRIMARY KEY (`ID`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `accounts`
--
ALTER TABLE `accounts`
  MODIFY `ID` int(11) UNSIGNED NOT NULL AUTO_INCREMENT COMMENT 'Порядковый номер аккаунта', AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT для таблицы `global_blacklist`
--
ALTER TABLE `global_blacklist`
  MODIFY `ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
