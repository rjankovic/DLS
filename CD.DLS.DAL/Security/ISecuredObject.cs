using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Security
{
    /*
    (N'ManageProject'),
	(N'ViewLineage'),
	(N'UpdateLineage'),
	(N'ViewAnnotations'),
	(N'EditAnnotations'),
	(N'EditPermissions')
     */

    public enum PermissionEnum { CreateProject, ManageProject, ViewLineage, UpdateLineage, ViewAnnotations, EditAnnotations, EditPermissions }

    /// <summary>
    /// A GUI object or requestprocessor / other feature that needs permissions
    /// </summary>
    public interface ISecuredObject
    {
        /// <summary>
        /// The permission the user needs to use this object
        /// </summary>
        PermissionEnum RequiredPermission { get; }

        /// <summary>
        /// The RefPath / other specification of the subset of the model for which the permission is required
        /// </summary>
        string PermissionScope { get; }
    }
}
