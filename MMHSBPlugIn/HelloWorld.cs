﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace MMHSBPlugIn
{
    public class ContactAddNote : IPlugin
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
                        context.SharedVariables.Add("key1","some info");

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

                 
            }
        }
    }
}
