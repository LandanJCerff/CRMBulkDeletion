using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace CRMBulkDeletion
{
    class DataLayer
    {
        public EntityCollection GetCases(IOrganizationService _orgServ, int pageNo, string casTickNum, Guid last, Guid first)
        {
            string activeCaseFetch = string.Empty;
            if (pageNo == 1)
            {
                activeCaseFetch = @"<fetch version='1.0' count='1000' page ='" + pageNo + @"'  output-format='xml-platform' mapping='logical' distinct='true'>                 
             <entity name='incident'>
                <attribute name='ticketnumber'/>    
                <attribute name='incidentid'/>
	            <attribute name='statecode'/>
                    <attribute name='gcs_casetypes'/>
                    <attribute name='createdon'/>   
                    <attribute name='shg_casesavefield'/>      
	            <attribute name='statuscode'/>                  
                   </entity>
            </fetch>";
            }
            else
            {
                activeCaseFetch = @"<fetch version='1.0' count='1000' output-format='xml-platform' paging-cookie='&lt;cookie page=&quot;" + pageNo + @"&quot;&gt;&lt;incidentid last=&quot;{" + last + @"}&quot; first=&quot;{" + first + @"}&quot;/&gt;&lt;/cookie&gt;' page='" + pageNo + @"'>          
             <entity name='incident'>
                <attribute name='ticketnumber'/>    
                <attribute name='incidentid'/>
                    <attribute name='shg_casesavefield'/>      
                    <attribute name='gcs_casetypes'/>               
                   </entity>
            </fetch>";
            }
            FetchExpression f = new FetchExpression(activeCaseFetch);
            EntityCollection activeCases = _orgServ.RetrieveMultiple(f);
            if (activeCases.Entities.Count >= 1)
            {
                return activeCases;
            }
            else
            {
                return null;
            }
        }

        public class CaseStorage
        {
            public Guid caseGuid { get; set; }
            public string caseType { get; set; }
        }
    }
}
