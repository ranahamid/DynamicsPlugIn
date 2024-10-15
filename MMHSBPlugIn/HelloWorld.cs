using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace MMHSBPlugIn
{
    public class HelloWorld : IPlugin
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

                #region contact create

                if (entity.LogicalName == "contact")
                {
                    try
                    {
                        // Plug-in business logic goes here.


                        var firstName = string.Empty;
                        if (entity.Attributes.Contains("firstname"))
                            firstName = entity.Attributes["firstname"].ToString();
                        var lastname = entity.Attributes["lastname"].ToString(); //lastname is mandatory field
                        entity["description"] = "Hello " + firstName + " " + lastname + "!";

                        // Create the task in Microsoft Dynamics CRM.
                        tracingService.Trace("FollowupPlugin: Creating the task activity.");
                        //service.Update(entity);
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

                #region Create a task activity to follow up with the account customer in 7 days.

                if (entity.LogicalName == "account")
                {
                    try
                    {
                        // Plug-in business logic goes here.  
                        // Create a task activity to follow up with the account customer in 7 days. 
                        Entity followup = new Entity("task");

                        followup["subject"] = "Send e-mail to the new customer.";
                        followup["description"] =
                            "Follow up with the customer. Check if there are any new issues that need resolution.";
                        followup["scheduledstart"] = DateTime.Now.AddDays(7);
                        followup["scheduledend"] = DateTime.Now.AddDays(7);
                        followup["category"] = context.PrimaryEntityName;

                        // Refer to the account in the task activity.
                        if (context.OutputParameters.Contains("id"))
                        {
                            Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                            string regardingobjectidType = "account";

                            followup["regardingobjectid"] =
                                new EntityReference(regardingobjectidType, regardingobjectid);
                        }

                        // Create the task in Microsoft Dynamics CRM.
                        tracingService.Trace("FollowupPlugin: Creating the task activity.");
                        service.Create(followup);
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
