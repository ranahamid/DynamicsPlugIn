using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace MMHSBPlugIn
{
    public class PreEntityImage : IPlugin
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

                if (entity.LogicalName == "lead")
                {
                    try
                    {
                        var modifiedBusinessPhone = entity.GetAttributeValue<string>("telephone1");

                        var preImage = context.PreEntityImages["PreImage"];
                        var preBusinessPhone = preImage.GetAttributeValue<string>("telephone1"); //preBusinessPhone is the value of the telephone1 attribute before the update

                        //throw new InvalidPluginExecutionException(
                        //    $"old phone:{preBusinessPhone}. New phone: {modifiedBusinessPhone}");

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
