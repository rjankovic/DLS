using CD.DLS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Structures
{
    public abstract class CoreBase : ICore
    {
        private Guid _id;
        private string _name;
        private string _modelConnectionString;
        protected ILogger _log;
        private IStorageProvider _storageProvider;
        private bool _initialized;
        protected ProjectConfig _projectConfig;
        private bool _isBusy;
        
        public abstract CoreTypeEnum CoreType
        {
            get;
        }

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        public ILogger Log
        {
            get
            {
                return _log;
            }
        }
        
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
            }
        }

        public virtual void Dispose()
        {

        }
        

        public abstract Task<RequestMessage> ProcessMessage(RequestMessage input);

        public virtual void Init(Guid id, string name, ILogger log, ProjectConfig projectConfig)
        {
            _id = id;
            _name = name;
            _log = log;
            _initialized = true;
            _projectConfig = projectConfig;
        }

        public void UpdateConfig(ProjectConfig projectConfig)
        {
            _projectConfig = projectConfig;
        }
    }
}
