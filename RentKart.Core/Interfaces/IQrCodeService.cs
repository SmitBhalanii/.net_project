namespace RentKart.Core.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string content);
}
