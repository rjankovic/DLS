using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Tools
{
    public class Repository : IDisposable
    {

        private EA.Repository _r = new EA.Repository();
        private string _connectionString;
        private string _repoName;
        private string _repoConnectionString;
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public string RepoName
        {
            get
            {
                return _repoName;
            }
        }

        public Repository(string connectionString, string repoName)
        {
            //"Enterprise_Architect_NOIS --- DBType=1;Connect=Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;LazyLoad=1;"
            _connectionString = connectionString;
            _repoName = repoName;
            if (!_connectionString.EndsWith(";"))
            {
                _connectionString = _connectionString + ";";
            }
            _repoConnectionString = string.Format("{0} --- DBType=1;Connect=Provider=SQLOLEDB.1;{1}LazyLoad=1;", _repoName, _connectionString);
            //_r.OpenFile(repoConnString);
        }

        public EA.Package GetPackageById(int id)
        {
            try
            {
                return _r.GetPackageByID(id);
            }
            catch (COMException comEx)
            {
                _r.OpenFile(_repoConnectionString);
                return _r.GetPackageByID(id);
            }
        }

        public EA.Element GetElementById(int id)
        {
            try
            {
                return _r.GetElementByID(id);
            }
            catch (COMException comEx)
            {
                _r.OpenFile(_repoConnectionString);
                return _r.GetElementByID(id);
            }
        }

        public void Dispose()
        {
            _r.CloseFile();
            Marshal.ReleaseComObject(_r);
        }
    }
    
}
