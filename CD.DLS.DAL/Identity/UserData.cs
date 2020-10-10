using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Common.Structures;

namespace CD.DLS.DAL.Identity
{

    public class UserGroup
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class UserData
    {
        public const string CUSTOMER_GROUP_PREFIX = "cust_";
        public int UserId { get; set; }
        public string Identity { get; set; }
        public string DisplayName { get; set; }
        public List<UserGroup> Groups { get; set; }

        public string GetCustomerCode()
        {
            if (Groups == null)
            {
                return "default";
            }

            var customerGroup = Groups.FirstOrDefault(x => x.Name.StartsWith(CUSTOMER_GROUP_PREFIX));
            if (customerGroup == null)
            {
                return "default";
            }

            return customerGroup.Name.Substring(CUSTOMER_GROUP_PREFIX.Length).ToLower();
        }
    }
}