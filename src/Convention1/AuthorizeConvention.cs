using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;

namespace Convention1
{
    public class AuthorizeConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var actionModel in controller.Actions)
                {
                    if (actionModel.ActionName.StartsWith("Authorized") && 
                        !actionModel.Filters.OfType<IAuthorizeData>().Any()) 
                    {
                        actionModel.Filters.Add(new AuthorizeFilter());
                    }
                }
            }
        }
    }
}
