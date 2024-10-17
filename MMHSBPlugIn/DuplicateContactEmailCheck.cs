using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MMHSBPlugIn
{
    public class DuplicateContactEmailCheck:IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);



                #region Create a task activity to follow up with the account customer in 7 days.

                if (entity.LogicalName == "contact")
                {
                    try
                    {
                        var emailAddress = string.Empty;
                        if (entity.Attributes.Contains("emailaddress1"))
                        {
                            emailAddress = entity.GetAttributeValue<string>("emailaddress1");
                            emailAddress = entity.Attributes["emailaddress1"].ToString();
                        }
                        var fetchXml = $@"
                            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                <entity name='contact'>
                                    <attribute name='emailaddress1' />
                                    <filter type='and'>
                                        <condition attribute='emailaddress1' operator='eq' value='{emailAddress}' />
                                    </filter>
                                </entity>
                            </fetch>";

                        QueryExpression query = new QueryExpression("contact");
                        query.ColumnSet = new ColumnSet(new string[]{ "emailaddress1" });
                        query.Criteria.AddCondition("emailaddress1",conditionOperator: ConditionOperator.Equal, emailAddress);

                        var collection = service.RetrieveMultiple(query);
                        if(collection.Entities.Count > 0)
                        {
                            throw new InvalidPluginExecutionException("Duplicate Email Found");
                        }

                        tracingService.Trace("FollowupPlugin: checking duplicate email."); 
                    }

                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                    }

                    catch (Exception ex)
                    {
                        tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                        throw;
                    }
                }

                #endregion
            }
        }

    }
}
