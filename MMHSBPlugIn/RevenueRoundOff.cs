using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MMHSBPlugIn
{
    public class RevenueRoundOff : IPlugin
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



                #region revenue

                if (entity.LogicalName == "account")
                {
                    tracingService.Trace(context.Depth.ToString());
                    //if(context.Depth > 1)
                    //{
                    //    return;
                    //}
                    if (entity.Attributes["revenue"] != null)
                    {
                        var revenue = ((Money)entity.Attributes["revenue"]).Value;
                        revenue= Math.Round(revenue, 1);
                        entity.Attributes["revenue"] = new Money(revenue);

                    }
                    
                }

                #endregion
            }
        }

    }
}
