namespace BlaineRP.Client.Game.Helpers.Colshapes.Enums
{
    /// <summary>Типы колшейпов</summary>
    public enum ColshapeTypes
    {
        /// <summary>Сферический (трехмерный)</summary>
        Sphere = 0,
        /// <summary>Круговой (двумерный)</summary>
        Circle,
        /// <summary>Цилиндрический (трехмерный)</summary>
        Cylinder,
        /// <summary>Многогранник (трехмерный/двумерный)</summary>
        /// <remarks>Размерность зависит от высоты (0 - двухмерный, > 0 - трехмерный</remarks>
        Polygon,
        /// <summary>Кубический (трехмерный/двумерный)</summary>
        /// <remarks>Размерность зависит от высоты (0 - двухмерный, > 0 - трехмерный<br/>Фактически - Polygon</remarks>
        Cuboid,
    }
}