using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using CRMConnection.Interfaces;
using CRMConnection.Models;
using UOPXRM;

namespace CRMConnection
{
    class Queries
    {
        private readonly IConnection _connection;
        private readonly IOrganizationService _service;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Queries(IConnection connection)
        {
            this._connection = connection;
            this._service = _connection.Connect();
        }
        
        public void EarlyBoundLinqQuery()
        {
            try
            {
                using (var context = new XrmServiceContext(_service))
                {
                    var getContactRecords = (from conSet in context.ContactSet
                                             join student in context.AccountSet on conSet.ContactId equals student.new_Person.Id
                                             join applications in context.LeadSet on conSet.ContactId equals applications.new_Person.Id
                                             select new ContactData()
                                             {
                                                 FullName = conSet.FullName,
                                                 StudentId = student.New_UserID,
                                                 ApplicantId = applications.New_CRMApplicantID
                                             })
                                             .ToList();


                    if (getContactRecords.Count > 0)
                    {
                        foreach (var item in getContactRecords)
                        {
                            if (item.FullName != null && item.StudentId != null && item.ApplicantId != null)
                            {
                                Console.WriteLine($"Name: {item.FullName}, StudentID: {item.StudentId}, ApplicantID : {item.ApplicantId}");
                            }
                            else
                            {
                                _log.Error($"Missing field value in entity - Name: {item.FullName}, StudentID: {item.StudentId}, ApplicantID : {item.ApplicantId}");
                            }
                        }
                        _log.Info("Finished Early Bound with LINQ Query");
                        _log.Info($"Number of records found {getContactRecords.Count}");
                    }
                    else
                    {
                        _log.Error($"No records returned from the LINQ Query");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught during LINQ query execution - {ex.Message}");
                _log.Error($"Exception caught during LINQ query execution - {ex.Message}");
                throw;
            }
        }
        
        public void QueryExpressionQuery()
        {
            try
            {
                QueryExpression query = new QueryExpression {EntityName = "contact"};
                query.ColumnSet.Columns.Add("fullname");

                LinkEntity application = new LinkEntity("contact", "lead", "contactid", "new_person", JoinOperator.Inner);
                application.Columns = new ColumnSet("new_crmapplicantid");
                application.EntityAlias = "application";
                query.LinkEntities.Add(application);

                // Join Operator can be changed if there is chance of Null values in the Lookup. Use Left Outer join
                LinkEntity student = new LinkEntity("contact", "account", "contactid", "new_person", JoinOperator.Inner);
                student.Columns = new ColumnSet("new_userid");
                student.EntityAlias = "student";
                query.LinkEntities.Add(student);
                
                _log.Info($"Start executing QueryExpression Query for entity: {query.EntityName}");

                EntityCollection entityCollection = _service.RetrieveMultiple(query);
                if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
                {
                    foreach (var entity in entityCollection.Entities)
                    {
                        // Get the main Entity
                        string fullName = entity.Contains("fullname") ? entity["fullname"].ToString() : string.Empty;
                        // Use Link Entity Alias with column name
                        string applicationId = entity.Contains("application.new_crmapplicantid") ? ((AliasedValue) entity["application.new_crmapplicantid"]).Value.ToString() : string.Empty;
                        string studentId = entity.Contains("student.new_userid") ? ((AliasedValue) entity["student.new_userid"]).Value.ToString() : string.Empty;

                        Console.WriteLine($"Full Name: {fullName}, ApplicationID: {applicationId}, StudentID: {studentId}");
                    }
                    _log.Info($"Finished QueryExpression Query for entity {query.EntityName}");
                    _log.Info($"Number of records found {entityCollection.Entities.Count}");
                }
                else
                {
                    _log.Error($"Invalid entity collection generated for entity {query.EntityName}");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught during QueryExpression query execution - {ex.Message}");
                _log.Error($"Exception caught during QueryExpression query execution - {ex.Message}");
                throw;
            }
        }


        public void FetchXmlQuery()
        {
            try
            {
                const string fetchXmlString = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                            <entity name='contact'>
                                                <attribute name='fullname' />
                                                <attribute name='contactid' />
                                                <link-entity name='lead' from='new_person' to='contactid' link-type='inner' alias='bl'>
                                                    <attribute name='new_crmapplicantid' />
                                                </link-entity>
                                                <link-entity name='account' from='new_person' to='contactid' link-type='inner' alias='bm'>
                                                    <attribute name='new_userid' />
                                                </link-entity>
                                            </entity>
                                        </fetch>";

                _log.Info($"Start executing FetchXML Query with input: {fetchXmlString}");

                EntityCollection entityCollection = _service.RetrieveMultiple(new FetchExpression(fetchXmlString));
                if (entityCollection != null && entityCollection.Entities != null && entityCollection.Entities.Count > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    foreach (var item in entityCollection.Entities)
                    {
                        //Check for fullname value exists or not in Entity Collection
                        if (item.Attributes.Contains("fullname"))
                        {
                            stringBuilder.Append($"Full Name: {item.Attributes["fullname"]}, ");
                        }

                        //Check for studentid value exists or not in Entity Collection
                        if (item.Attributes.Contains("bm.new_userid") && item.Attributes["bm.new_userid"] != null)
                        {
                            stringBuilder.Append($"StudentId: {((AliasedValue)item["bm.new_userid"]).Value}, ");
                        }
                        
                        //Check for applicationid value exists or not in Entity Collection
                        if (item.Attributes.Contains("bl.new_crmapplicantid") && item.Attributes["bl.new_crmapplicantid"] != null)
                        {
                            stringBuilder.AppendLine($"ApplicationId: {((AliasedValue)item["bl.new_crmapplicantid"]).Value}");
                        }
                    }

                    string output = stringBuilder.ToString().TrimEnd();
                    Console.WriteLine(output);
                    _log.Info($"Finished FetchXML Query with input {fetchXmlString}");
                    _log.Info($"Number of records found {entityCollection.Entities.Count}");
                }
                else
                {
                    _log.Error($"Invalid entity collection generated for input {fetchXmlString}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught during FetchXML query execution - {ex.Message}");
                _log.Error($"Exception caught during FetchXML query execution - {ex.Message}");
                throw;
            }
        }
    }
}
