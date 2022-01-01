using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Model.Mssql.Ssis;

namespace CD.DLS.Parse.Mssql.Ssis
{
    /// <summary>
    /// Index of connection managers.
    /// </summary>
    /// <remarks>
    /// Stores elements of type <see cref="ConnectionManagerElement"/> in a hierarchy, allows finding them by id.
    /// </remarks>
    public class ConnectionIndex
    {
        private readonly Dictionary<string, ConnectionManagerElement> _connections;

        /// <summary>
        /// Creates an empty index of connection managers.
        /// </summary>
        public ConnectionIndex()
        {
            _connections = new Dictionary<string, ConnectionManagerElement>();
        }

        /// <summary>
        /// Creates a derived index of connection managers.
        /// </summary>
        /// <param name="parent">The index this index derives from.</param>
        public ConnectionIndex(ConnectionIndex parent)
        {
            _connections = new Dictionary<string, ConnectionManagerElement>(parent._connections);
        }

        /// <summary>
        /// Adds a connection manager to the index.
        /// </summary>
        /// <param name="node">The cnnection mananger element.</param>
        public void Add(ConnectionManagerElement connectionManager)
        {
            _connections.Add(connectionManager.ManagerId, connectionManager);
            
        }

        public void AddWithRefId(string refId, ConnectionManagerElement connectionManager)
        {
            _connections.Add(refId, connectionManager);

        }

        public bool TryGetConnectionManager(string id, out ConnectionManagerElement connectionManager)
        {
            return _connections.TryGetValue(id, out connectionManager);
        }

        public bool TryGetDbConnectionManager(string id, out DbConnectionManagerElement dbConnectionManager)
        {
            ConnectionManagerElement connManager;
            _connections.TryGetValue(id, out connManager);
            dbConnectionManager = connManager as DbConnectionManagerElement;
            return dbConnectionManager != null;
        }
        public ConnectionManagerElement this[string id] { get { return _connections[id]; } }
    }
}
