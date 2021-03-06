using NServiceKit.ServiceHost;
using NServiceKit.WebHost.Endpoints.Tests.Support.Operations;

namespace NServiceKit.WebHost.Endpoints.Tests.Support.Services
{
    /// <summary>A custom form data service.</summary>
	public class CustomFormDataService : ServiceInterface.Service
	{
		//Parsing: &first-name=tom&item-0=blah&item-1-delete=1
		public object Post(CustomFormData request)
		{
			var httpReq = base.RequestContext.Get<IHttpRequest>();

			return new CustomFormDataResponse
			{
				FirstName = httpReq.FormData["first-name"],
				Item0 = httpReq.FormData["item-0"],
				Item1Delete = httpReq.FormData["item-1-delete"]
			};
		}
	}
}