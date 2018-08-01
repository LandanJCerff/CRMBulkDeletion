using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace CRMBulkDeletion
{
    class Program : ServiceLayer
    {
              
        public static void Main(string[] args)
        {
           
            IOrganizationService _orgServ = null;

            try
            {
                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = "landan.cerff@shgroup.org.uk";
                clientCredentials.UserName.Password = "Ep1c.tul1p1";

                Console.WriteLine("credentials set");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                _orgServ = (IOrganizationService)new OrganizationServiceProxy(new Uri("https://shgl-sandbox1.api.crm4.dynamics.com/XRMServices/2011/Organization.svc"),
                    null, clientCredentials, null);

               

                Console.WriteLine("connection made");
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                Console.WriteLine(e.Message);
            }
                       
            try
            {
                Console.WriteLine("Program started; Case Deletion initiated");
                ServiceLayer myClass = new ServiceLayer();
                myClass.CaseDeletion(_orgServ);               
                Console.WriteLine("End");
            }

            catch (FaultException<OrganizationServiceFault> e)
            {               
                Console.WriteLine(e.Message);
            }
        }
   
    }
}
