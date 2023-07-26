using System.Linq;
using RAGE.Ui;

namespace BlaineRP.Client.Extensions.RAGE.Ui
{
    public static class HtmlWindowExtensions
    {
        /// <summary>Выполнить функцию в окне браузера</summary>
        /// <param name="window">Окно</param>
        /// <param name="function">Функция</param>
        /// <param name="args">
        ///     Аргументы (если передается массив или IEnumerable и для передачи подразумевается именно массив, то
        ///     нужно дополнительно обернуть его в new object[] { arr }
        /// </param>
        public static void ExecuteJs(this HtmlWindow window, string function, params object[] args)
        {
            window?.ExecuteJs($"{function}({string.Join(", ", args.Select(x => global::RAGE.Util.Json.Serialize(x)))});");
        }

        /// <summary>Выполнить кэшируемую функцию в окне браузера</summary>
        /// <param name="window">Окно</param>
        /// <param name="function">Функция</param>
        /// <param name="args">
        ///     Аргументы (если передается массив или IEnumerable и для передачи подразумевается именно массив, то
        ///     нужно дополнительно обернуть его в new object[] { arr }
        /// </param>
        /// <remarks>
        ///     Использовать только для ОДИНАКОВЫХ функций, которые часто выполняются и содержат ОДИНАКОВЫЕ аргументы, в
        ///     противном случае использовать обычный ExecuteJs
        /// </remarks>
        public static void ExecuteCachedJs(this HtmlWindow window, string function, params object[] args)
        {
            window?.ExecuteCachedJs($"{function}({string.Join(", ", args.Select(x => global::RAGE.Util.Json.Serialize(x)))});");
        }
    }
}