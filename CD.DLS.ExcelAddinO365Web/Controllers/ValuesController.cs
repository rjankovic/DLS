// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See full license at the root of the repo.

/* 
    This file provides controller methods to get data from MS Graph. 
*/

using Microsoft.Identity.Client;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Net;
using System.Net.Http;
using CD.DLS.ExcelAddinO365Web.Helpers;
using CD.DLS.ExcelAddinO365Web.Models;

namespace CD.DLS.ExcelAddinO365Web.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        private HttpResponseMessage SendErrorToClient(HttpStatusCode statusCode, Exception e, string message)
        {
            HttpError error;
            if (e != null)
            {
                error = new HttpError(e, true);
            }
            else
            {
                error = new HttpError(message);
            }
            var requestMessage = new HttpRequestMessage();
            var errorMessage = requestMessage.CreateErrorResponse(statusCode, error);
            return errorMessage;
        }
        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        // GET api/values
        public async Task<HttpResponseMessage> Get()
        {
            // TODO1: Validate the scopes of the access token.
            string[] addinScopes = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value.Split(' ');
            if (addinScopes.Contains("access_as_user"))
            {
                // TODO2: Assemble all the information that is needed to get a token for Microsoft Graph using the "on behalf of" flow.
                var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as BootstrapContext;
                UserAssertion userAssertion = new UserAssertion(bootstrapContext.Token);
                ClientCredential clientCred = new ClientCredential(ConfigurationManager.AppSettings["ida:Password"]);
                ConfidentialClientApplication cca =
                                new ConfidentialClientApplication(ConfigurationManager.AppSettings["ida:ClientID"],
                                                                  "https://localhost:44355", clientCred, null, null);
                string[] graphScopes = { "Files.Read.All" };

                // TODO3: Get the access token for Microsoft Graph.
                AuthenticationResult result = null;
                try
                {
                    result = await cca.AcquireTokenOnBehalfOfAsync(graphScopes, userAssertion, "https://login.microsoftonline.com/common/oauth2/v2.0");
                }
                catch (MsalServiceException e)
                {
                    // TODO3a: Handle request for multi-factor authentication.
                    if (e.Message.StartsWith("AADSTS50076"))
                    {
                        string responseMessage = String.Format("{{\"AADError\":\"AADSTS50076\",\"Claims\":{0}}}", e.Claims);
                        return SendErrorToClient(HttpStatusCode.Forbidden, null, responseMessage);
                    }
                    // TODO3b: Handle lack of consent.
                    // TODO3c: Handle invalid scope (permission).
                    if ((e.Message.StartsWith("AADSTS65001"))
|| (e.Message.StartsWith("AADSTS70011: The provided value for the input parameter 'scope' is not valid.")))
                    {
                        return SendErrorToClient(HttpStatusCode.Forbidden, e, null);
                    }
                    // TODO3d: Handle all other MsalServiceExceptions.
                    else
                    {
                        throw e;
                    }
                }
                // TODO4: Get the names of files and folders in OneDrive by using the Microsoft Graph API.
                var fullOneDriveItemsUrl = GraphApiHelper.GetOneDriveItemNamesUrl("?$select=name&$top=3");
                IEnumerable<OneDriveItem> filesResult;
                try
                {
                    filesResult = await ODataHelper.GetItems<OneDriveItem>(fullOneDriveItemsUrl, result.AccessToken);
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
                {
                    return SendErrorToClient(HttpStatusCode.Unauthorized, e, null);
                }
                // TODO5: Remove excess information from the data and send the data to the client.
                List<string> itemNames = new List<string>();
                foreach (OneDriveItem item in filesResult)
                {
                    itemNames.Add(item.Name);
                }

                var requestMessage = new HttpRequestMessage();
                requestMessage.SetConfiguration(new HttpConfiguration());
                var response = requestMessage.CreateResponse<List<string>>(HttpStatusCode.OK, itemNames);
                return response;
            }
            return SendErrorToClient(HttpStatusCode.Unauthorized, null, "Missing access_as_user.");
        }
    }
}

