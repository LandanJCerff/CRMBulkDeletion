using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace CRMBulkDeletion
{
    class ServiceLayer : DataLayer
    {
        List<CaseStorage> safeList = new List<CaseStorage>();

        public void CaseDeletion(IOrganizationService _orgServ)
        {
            Console.WriteLine("Starting Case Deletion");
            OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);
            Incident myCase = null;
            int pageNo = 1;
            int record = 0;           
            string casTickNum = "";
            string casType;
            Guid  last = new Guid();
            Guid first = new Guid();
        start:
            EntityCollection retrievedCases = GetCases(_orgServ, pageNo, casTickNum, first, last);
            if (retrievedCases.Entities.Count >1)
            {
                Console.WriteLine("Cases Retrieved");

                for (int r = 0; r < retrievedCases.Entities.Count; r++)
                {
                    record++;
                    Console.WriteLine("record number = " + record);
                    myCase = retrievedCases[r].ToEntity<Incident>();
                    var casId = myCase.IncidentId;

                    if (myCase.gcs_CaseTypes == null)
                    {
                        _orgContext.Attach(myCase);
                        _orgContext.DeleteObject(myCase);
                        _orgContext.Dispose();
                        continue;
                    }
                    casType = myCase.gcs_CaseTypes.Name.ToString();

                    CaseSaveList((Guid)casId, casType);

                    int idExist = safeList.FindIndex(s => s.caseGuid == casId);

                    try
                    {
                        if (idExist >= 0)
                        {
                            Entity updateCase = new Entity();
                            updateCase.LogicalName = myCase.LogicalName;
                            updateCase.Attributes["shg_casesavefield"] = true;
                            updateCase.Id = myCase.Id;
                            if (myCase.StateCode != 0)
                            {
                                SetStateRequest setStateRequest = new SetStateRequest()
                                {
                                    EntityMoniker = new EntityReference
                                    {
                                        Id = myCase.Id,
                                        LogicalName = myCase.LogicalName,
                                    },
                                    Status = new OptionSetValue(1),
                                    State = new OptionSetValue(0)
                                };
                                _orgServ.Execute(setStateRequest);
                            }
                            _orgServ.Update(updateCase);
                            continue;
                        }
                    }
                    catch (FaultException<OrganizationServiceFault> e)
                    {
                        Console.WriteLine(e + "case which failed = " + myCase.TicketNumber + " case type = " + myCase.gcs_CaseTypes);
                        continue;
                    }
                }                
                pageNo++;
                first = (Guid)retrievedCases.Entities.Select(s => s.Attributes["incidentid"]).First();                
                last = (Guid)retrievedCases.Entities.Select(s => s.Attributes["incidentid"]).Last();
                goto start;
            }  
            else
            {
                BulkCaseDeletion(_orgServ);
                BulkActivityDeletion(_orgServ);
            }         
        }

        public void CaseSaveList(Guid casId, string casType)
        {
            //safe list for holding records which meet criteria

            CaseStorage match = new CaseStorage();
            var count = safeList.Where(s => s.caseType == casType).Count();

            if (safeList.Count < 1)
            {                
                safeList.Add(new CaseStorage { caseGuid = casId, caseType = casType });
                Console.WriteLine("Case added to safe list = " + casId + " case type count = " + (count + 1) + " " + casType);
            }
            else if (count > 200)
            {
                return;
            }
            else
            {                
                safeList.Add(new CaseStorage { caseGuid = casId, caseType = casType });
                Console.WriteLine("Case added to safe list = " + casId + " case type count = " + (count + 1) + " " + casType);
            }
        }

        public  void BulkActivityDeletion(IOrganizationService _orgServ)
        {
            OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);
            
            ConditionExpression conEx = new ConditionExpression("regardingobjectid", ConditionOperator.Null);
            FilterExpression fiEx = new FilterExpression();
            fiEx.AddCondition(conEx);
            BulkDeleteRequest request = new BulkDeleteRequest
            {
                JobName = "Delete unlinked activites",
                ToRecipients = new Guid[] { },
                CCRecipients = new Guid[] { },
                RecurrencePattern = string.Empty,
                QuerySet = new QueryExpression[]
        {
            new QueryExpression { EntityName = "activitypointer", Criteria = fiEx}            
        }
            };
            BulkDeleteResponse response = (BulkDeleteResponse)_orgServ.Execute(request);            
    }

        public void BulkCaseDeletion(IOrganizationService _orgServ)
        {
            OrganizationServiceContext _orgContext = new OrganizationServiceContext(_orgServ);

            ConditionExpression conEx = new ConditionExpression("shg_casesavefield", ConditionOperator.NotEqual, true);
            FilterExpression fiEx = new FilterExpression();
            fiEx.AddCondition(conEx);
            BulkDeleteRequest request = new BulkDeleteRequest
            {
                JobName = "Delete unsaved cases",
                ToRecipients = new Guid[] { },
                CCRecipients = new Guid[] { },
                RecurrencePattern = string.Empty,
                QuerySet = new QueryExpression[]
        {
            new QueryExpression { EntityName = "incident", Criteria = fiEx}
        }
            };
            BulkDeleteResponse response = (BulkDeleteResponse)_orgServ.Execute(request);
        }
    }
}

