namespace FoodDelivery.Domain.Enums;

public enum UserRole
{
    Customer = 1,
    Merchant = 2,
    Driver = 3,
    Admin = 4
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Preparing = 3,
    ReadyForPickup = 4,
    PickedUp = 5,
    Delivering = 6,
    Delivered = 7,
    Cancelled = 8
}

public enum PaymentMethod
{
    Cash = 1,
    MoMo = 2,
    ZaloPay = 3,
    Card = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}

public enum DriverStatus
{
    Offline = 1,
    Online = 2,
    Busy = 3
}

public enum VoucherType
{
    Percentage = 1,
    FixedAmount = 2,
    FreeShipping = 3
}

public enum EarningType
{
    Delivery = 1,
    Tip = 2,
    Bonus = 3
}
