using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Tools
{
    public static class WebTools
    {
        public static bool TestWebAccess(string url, out string error)
        {
            var myRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)myRequest.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;
                if (response == null)
                {
                    error = ex.Message;
                    return false;
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    error = null;
                    return true;
                }
                else
                {
                    error = response.StatusDescription;
                    return false;
                }
            }
            error = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                error = response.StatusDescription;
                return false;
            }
        }

        public static string GetHost(string uri)
        {
            return new Uri(uri).Host;
        }

    }
}
