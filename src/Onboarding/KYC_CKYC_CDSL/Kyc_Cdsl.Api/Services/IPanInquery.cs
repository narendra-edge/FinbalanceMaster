using System.ServiceModel;

namespace Kyc_Cdsl.Api.Services
{
    [ServiceContract]
    public interface IPanInquery
    {
        [OperationContract] 
        string GetPassword(string password,string Passkey);
        string GetPanStatus(string panNo, string userName,
                           string PosCode, string password, string PassKey);
        string RTAPANDataDownload(string inputXML, string userName, string PosCode,
                                  string password, string PassKey);
        string RTAPANDataDownload_XMLDoc(string inputXML, string userName, string PosCode,
                                         string password, string PassKey);
        string InsertUpdateKYCRecord(string inputXML, string userName, string PosCode,
                                     string password, string PassKey);
        string SolicitPANDetailsFetchALLKRA(string inputXML, string userName, string PosCode,
                                            string password, string PassKey);
    }
}
