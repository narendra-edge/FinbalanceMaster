using System.ServiceModel;

namespace Kyc_Cdsl.Api.Services
{
    [ServiceContract]
    public class PanInquery : IPanInquery
    {
        public string GetPanStatus(string panNo, string userName, string PosCode, string password, string PassKey)
        {
            throw new NotImplementedException();
        }

        public string GetPassword(string password, string Passkey)
        {
            throw new NotImplementedException();
        }

        public string InsertUpdateKYCRecord(string inputXML, string userName, string PosCode, string password, string PassKey)
        {
            throw new NotImplementedException();
        }

        public string RTAPANDataDownload(string inputXML, string userName, string PosCode, string password, string PassKey)
        {
            throw new NotImplementedException();
        }

        public string RTAPANDataDownload_XMLDoc(string inputXML, string userName, string PosCode, string password, string PassKey)
        {
            throw new NotImplementedException();
        }

        public string SolicitPANDetailsFetchALLKRA(string inputXML, string userName, string PosCode, string password, string PassKey)
        {
            throw new NotImplementedException();
        }
    }
}
