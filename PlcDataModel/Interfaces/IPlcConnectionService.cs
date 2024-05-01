namespace PlcDataModel.Interfaces;
public interface IPlcConnectionService
{
    void RegistrerNofitications();
    void HandleNotificationRequest();
    bool GetPlcDataName();

}