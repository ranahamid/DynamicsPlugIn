using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MMHSBPlugIn
{
    public class TaskCreate : IPlugin
    {
        private int tax;
        public TaskCreate(string unsecureConfig, string secureConfig)
        {
            Int32.TryParse(unsecureConfig, out tax);
        }
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



                #region Create a task activity to follow up with the account contact in 7 days.

                if (entity.LogicalName == "contact")
                {
                    try
                    {
                        var sharedVariableValue= context.SharedVariables["key1"];

                        // Plug-in business logic goes here.  
                        // Create a task activity to follow up with the contact in 7 days. 
                        Entity followup = new Entity("task");

                        followup["subject"] = "Send e-mail to the new contact.";
                        followup["description"] =
                            $"Follow up with the contact. Check if there are any new issues that need resolution. sharedVariableValue {sharedVariableValue}";
                        followup["scheduledstart"] = DateTime.Now.AddDays(7);
                        followup["scheduledend"] = DateTime.Now.AddDays(7);
                        followup["category"] = context.PrimaryEntityName;
                        followup["prioritycode"] = new OptionSetValue(1);// Normal

                        string regardingobjectidType = "contact";
                        Guid regardingobjectid = Guid.Empty;

                        // Refer to the account in the task activity.
                        //if (context.OutputParameters.Contains("id"))
                        //{
                        //    regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        //}
                        regardingobjectid= entity.Id;

                        //followup["regardingobjectid"] =
                        //    new EntityReference(regardingobjectidType, regardingobjectid);

                        followup["regardingobjectid"] = entity.ToEntityReference();

                        // Create the task in Microsoft Dynamics CRM.
                        tracingService.Trace("FollowupPlugin: Creating the task activity.");
                        var taskGuid = service.Create(followup);
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
