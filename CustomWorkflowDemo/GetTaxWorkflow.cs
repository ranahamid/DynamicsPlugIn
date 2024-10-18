using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace CustomWorkflowDemo
{
    public class GetTaxWorkflow : CodeActivity
    {
        [Input("Key")]
        public InArgument<string> Key { get; set; }

        [Output("Tax")]
        public OutArgument<string> Tax { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            //Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

          
            var key = Key.Get(executionContext);

            QueryByAttribute query = new QueryByAttribute("mmh_configuratoinsettingtbl");
            query.ColumnSet = new ColumnSet(new string[]{ "mmh_value" });
            query.AddAttributeValue("mmh_name", key);
            EntityCollection collection= service.RetrieveMultiple(query);
            if (collection .Entities.Count!= 1)
            {
                tracingService.Trace("wrong in configuration.");
            }

            var taxEntity = collection.Entities.FirstOrDefault();
            if (taxEntity != null)
            {
                var taxRate= taxEntity.Attributes["mmh_value"].ToString();
                Tax.Set(executionContext, taxRate);
            } 
        }
    }
}
