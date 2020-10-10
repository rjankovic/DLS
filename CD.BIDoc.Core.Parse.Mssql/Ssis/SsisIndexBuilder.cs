using CD.DLS.Model.Mssql.Ssis;
using System.Collections.Generic;
using System.Linq;

namespace CD.DLS.Parse.Mssql.Ssis
{
    /// <summary>
    /// Builds an index of nodes and referrables from a server model.
    /// </summary>
    public class SsisIndexBuilder
    {
        public SsisIndex BuildPackagesIndex(List<ServerElement> serverElements)
        {
            SsisIndex index = new SsisIndex();
            var catalogs = serverElements.SelectMany(x => x.Children).Where(c => c is CatalogElement).Cast<CatalogElement>();
            var folders = catalogs.SelectMany(cat => cat.Children).Where(c => c is FolderElement).Cast<FolderElement>();
            var projects = folders.SelectMany(fol => fol.Children).Where(c => c is ProjectElement).Cast<ProjectElement>();
            var packages = projects.SelectMany(prj => prj.Children).Where(c => c is PackageElement).Cast<PackageElement>();

            foreach(var package in packages)
            {
                index.Add(package.RefPath.Path /*Caption*/, package.RefPath.Path, null, package);

            }
            
            return index;
        }
    }
}
