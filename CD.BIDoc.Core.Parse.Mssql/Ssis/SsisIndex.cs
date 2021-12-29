using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Ssis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Ssis
{
    public class SsisIndex
    {
        private class Referrable
        {
            public ReferrableValueElement _element;
            public SsisModelElement _definingElement;
            public Referrable(ReferrableValueElement element, SsisModelElement definingElement)
            {
                _element = element;
                _definingElement = definingElement;
            }
        }

        private readonly Dictionary<string, Referrable> _referrablesByName;
        private readonly Dictionary<string, Referrable> _referrablesById;
        private readonly Dictionary<string, Referrable> _definingElementsByRefPath;
        private readonly Dictionary<string, DfColumnElement> _columnElementsByName;
        private readonly Dictionary<string, DfColumnElement> _columnElementsByLineageId;
        /// <summary>
        /// Creates an empty index.
        /// </summary>
        public SsisIndex()
        {
            _referrablesByName = new Dictionary<string, Referrable>();
            _referrablesById = new Dictionary<string, Referrable>();
            _definingElementsByRefPath = new Dictionary<string, Referrable>();
            _columnElementsByName = new Dictionary<string, DfColumnElement>();
            _columnElementsByLineageId = new Dictionary<string, DfColumnElement>();

        }
        /// <summary>
        /// Creates a derived index.
        /// </summary>
        /// <param name="parent">The index that is used as a parent.</param>
        public SsisIndex(SsisIndex parent)
        {
            _referrablesByName = new Dictionary<string, Referrable>(parent._referrablesByName);
            _referrablesById = new Dictionary<string, Referrable>(parent._referrablesById);
            _definingElementsByRefPath = new Dictionary<string, Referrable>(parent._definingElementsByRefPath);
        }
        /// <summary>
        /// Tests whether the name is stored in the index.
        /// </summary>
        /// <param name="name">Name of a referrable.</param>
        /// <returns>true, if <paramref name="name"/> is in the index</returns>
        public bool ContainsName(string name)
        {
            return _referrablesByName.ContainsKey(name);
        }
        public string GetValueByName(string name)
        {
            return _referrablesByName[name]._element.Value;
        }

        public bool TryGetNodeByName(string name, out ReferrableValueElement node)
        {
            Referrable referrable;
            if(_referrablesByName.TryGetValue(name, out referrable))
            {
                node = referrable._element;
                return true;
            }
            else
            {
                node = default(ReferrableValueElement);
                return false;
            }
        }
        public bool TryGetNodeById(string id, out ReferrableValueElement node)
        {
            Referrable referrable;
            if (_referrablesById.TryGetValue(id, out referrable))
            {
                node = referrable._element;
                return true;
            }
            else
            {
                node = default(ReferrableValueElement);
                return false;
            }
        }
        public ReferrableValueElement GetNodeByName(string name)
        {
            return _referrablesByName[name]._element;
        }


        public SsisModelElement TryGetDefiningElementByRefPath(RefPath refPath)
        {
            if (_definingElementsByRefPath.ContainsKey(refPath.Path))
            {
                return _definingElementsByRefPath[refPath.Path]._definingElement;
            }
            return null;
        }

        public bool TryGetNodeByColumnLineageId(string lineageID, out DfColumnElement node)
        {
            DfColumnElement dfColumnElement;
            if (_columnElementsByLineageId.TryGetValue(lineageID, out dfColumnElement))
            {
                node = dfColumnElement;
                return true;
            }
            else
            {
                node = default(DfColumnElement);
                return false;
            }
        }

        public void Add(string name, string id, ReferrableValueElement referrableElement, SsisModelElement definingElement)
        {
            var referrable = new Referrable(referrableElement, definingElement);
            _referrablesByName.Add(name, referrable);
            _referrablesById.Add(id, referrable);
            if (!_definingElementsByRefPath.ContainsKey(definingElement.RefPath.Path))
            {
                _definingElementsByRefPath.Add(definingElement.RefPath.Path, referrable);
            }
        }

        public void AddColumn(string lineageId, DfColumnElement dfColumn)
        {
            _columnElementsByLineageId.Add(lineageId, dfColumn);
        }
    }
}
