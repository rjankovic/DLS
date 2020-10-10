using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CD.DLS.DAL.Managers
{
    public class SearchManager
    {
        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public SearchManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public SearchManager()
        {
            _netBridge = new NetBridge();
        }


        public void IndexFulltext(Guid projectConfigId)
        {

            NetBridge.ExecuteProcedure("Search.sp_FindRootElements", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId }
            });


            NetBridge.ExecuteProcedure("Search.sp_IndexFulltext", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId }
            });
        }

        public List<SearchRootElement> GetRootElements(Guid projectConfigId)
        {
            var dt = NetBridge.ExecuteTableFunction("[Search].[f_GetRootElements]", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId }
            });

            List<SearchRootElement> res = new List<SearchRootElement>();

            foreach (DataRow dr in dt.Rows)
            {
                res.Add(new SearchRootElement()
                {
                    ModelElementId = (int)dr["ModelElementId"],
                    Caption = (string)dr["Caption"],
                    ElementType = (string)dr["ElementType"],
                    RefPath = (string)dr["RefPathPrefix"]
                });
            }

            res = res.OrderBy(x => x.ElementType).ToList();

            return res;
        }

        public List<SearchParentChildTypeMapping> GetParentChildTypeMapping()
        {
            var dt = NetBridge.ExecuteSelectStatement("SELECT * FROM [Search].[vw_TypeChildTypes]");

            List<SearchParentChildTypeMapping> res = new List<SearchParentChildTypeMapping>();

            foreach (DataRow dr in dt.Rows)
            {
                res.Add(new SearchParentChildTypeMapping()
                {
                    ParentType = (string)dr["ParentType"],
                    ChildType = (string)dr["ChildType"],
                    ChildTypeDescription = (string)dr["ChildTypeDescription"]
                });
            }

            return res;
        }

        public List<FulltextSearchResult> FindFulltext(Guid projectConfigId, string pattern, string refPathPrefix, List<string> typeFilter)
        {
            var typeList = CreateStringList(typeFilter);

            if (string.IsNullOrWhiteSpace(refPathPrefix))
            {
                refPathPrefix = string.Empty;
            }

            var dt = NetBridge.ExecuteProcedureTable("[Search].[sp_FindFulltext]", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId },
                { "pattern", pattern },
                { "refPathPrefix", refPathPrefix },
                { "typeFilter", typeList }
            });

           

            return ReadFulltextSearchResults(dt);
        }

        private List<FulltextSearchResult> ReadFulltextSearchResults(DataTable dt)
        {
            List<FulltextSearchResult> res = new List<FulltextSearchResult>();

            foreach (DataRow dr in dt.Rows)
            {
                FulltextSearchResult item = new FulltextSearchResult()
                {
                    ModelElementId = (int)dr["ModelElementId"],
                    ElementName = (string)dr["ElementName"],
                    TypeDescription = (string)dr["TypeDescription"],
                    DescriptiveRootPath = (string)dr["DescriptiveRootPath"],
                    //BusinessName = dr["BusinessName"] == DBNull.Value ? null : (string)dr["BusinessName"],
                    BusinessFields = dr["BusinessFields"] == DBNull.Value ? null : (string)dr["BusinessFields"]
                };
                res.Add(item);
            }

            return res;
        }


        private DataTable CreateStringList(IEnumerable<string> values)
        {
            DataTable dt = new DataTable();
            dt.TableName = "BIDoc.UDTT_StringList";

            dt.Columns.Add("Value", typeof(string));

            foreach (var v in values)
            {
                var nr = dt.NewRow();
                nr[0] = v;
                dt.Rows.Add(nr);
            }

            return dt;
        }
        
    }

}
