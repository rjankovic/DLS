//using CD.DLS.Interfaces;
//using CD.DLS.Model.Mssql.Agent;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CD.DLS.Model.Mssql;
//using Microsoft.SqlServer.Management.Smo.Agent;
//using Microsoft.SqlServer.Management.IntegrationServices;

//namespace CD.DLS.Parse.Mssql.Agent
//{
//    class UrnBuilder
//    {
//        private IntegrationServices _ssis = null;

//        public UrnBuilder(IntegrationServices ssis)
//        {
//            _ssis = ssis;
//        }

//        internal RefPath GetOnStepSuccessUrn(StepElement stepElement)
//        {
//            return stepElement.RefPath.Child("OnSuccess");
//        }

//        internal RefPath GetOnStepFailureUrn(StepElement stepElement)
//        {
//            return stepElement.RefPath.Child("OnFailure");
//        }

//        internal RefPath GetPackageRefPath(JobStep step)
//        {
//            // / ISSERVER "\"\SSISDB\Manpower_SSIS\DWH\MasterAll.dtsx\"" / SERVER localhost / Par ...
//            // to IntegrationServices[@Name='RJ-THINK']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='Manpower_SSIS']/ProjectInfo[@Name='DWH']/PackageInfo[@Name='TransferTable.dtsx']
//            var packagePathStart = step.Command.IndexOf("\\\"") + 2;
//            // \SSISDB\Manpower_SSIS\DWH\MasterAll.dtsx
//            var packagePath = step.Command.Substring(packagePathStart, step.Command.IndexOf("\\\"", packagePathStart) - packagePathStart);
//            var pathSplit = packagePath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
//            var packageName = pathSplit[pathSplit.Length - 1];
//            var projectName = pathSplit[pathSplit.Length - 2];
//            var folderName = pathSplit[pathSplit.Length - 3];
//            var catalogName = pathSplit[pathSplit.Length - 4];

//            var packageRefPath = string.Format("{0}/Catalog[@Name='{1}']/CatalogFolder[@Name='{2}']/ProjectInfo[@Name='{3}']/PackageInfo[@Name='{4}']"
//                , _ssis.Urn, catalogName, folderName, projectName, packageName);

//            return new RefPath(packageRefPath);
//        }
//    }
//}
