namespace BlaineRP.Server.Game.Management.Phone
{
    public partial class SMS
    {
        public enum PredefinedTypes
        {
            [Language.Localized("GEN_SMS_DELIVERY_BUSINESS_ORDER_NEW_0", "SMS_TEXT")]
            DeliveryBusinessOrderNew = 0,
            [Language.Localized("GEN_SMS_DELIVERY_BUSINESS_ORDER_CANCEL_0", "SMS_TEXT")]
            DeliveryBusinessOrderCancel,
            [Language.Localized("GEN_SMS_DELIVERY_BUSINESS_ORDER_TAKEN_0", "SMS_TEXT")]
            DeliveryBusinessOrderTaken,
            [Language.Localized("GEN_SMS_DELIVERY_BUSINESS_ORDER_DELAY_0", "SMS_TEXT")]
            DeliveryBusinessOrderDelay,
            [Language.Localized("GEN_SMS_DELIVERY_BUSINESS_ORDER_FINISH_0", "SMS_TEXT")]
            DeliveryBusinessOrderFinish,
        }
    }
}