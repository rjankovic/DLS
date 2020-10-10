using CD.DLS.DAL.Objects.Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Lookup
{
    public class OlapRuleLookup
    {
        private OlapRuleSet _rules;
        private Dictionary<int, OlapField> _fieldsById = null;
        private Dictionary<int, List<OlapRule>> _rulesByPremiseFields = null;

        public OlapRuleLookup(OlapRuleSet rules)
        {
            _rules = rules;

            _rulesByPremiseFields = new Dictionary<int, List<OlapRule>>();


            // index rules by premise fields
            foreach (var rule in _rules.Rules)
            {
                foreach (var field in rule.PremiseFields)
                {
                    if (!_rulesByPremiseFields.ContainsKey(field.OlapFieldId))
                    {
                        _rulesByPremiseFields.Add(field.OlapFieldId, new List<OlapRule>());
                    }

                    _rulesByPremiseFields[field.OlapFieldId].Add(rule);
                }
            }

            _fieldsById = rules.Fields.ToDictionary(x => x.OlapFieldId, x => x);
        }

        public List<OlapFielsSuggestion> SuggestFields(List<OlapField> currentFields)
        {
            var candidateRules = currentFields.SelectMany(f =>
                _rulesByPremiseFields.ContainsKey(f.OlapFieldId) ?
                _rulesByPremiseFields[f.OlapFieldId] :
                new List<OlapRule>()
                ).ToList();

            Dictionary<int, double> fieldWeghts = new Dictionary<int, double>();
            HashSet<int> currentFieldIds = new HashSet<int>(currentFields.Select(x => x.OlapFieldId));

            foreach (var candidateRule in candidateRules)
            {
                // premise matched
                if (candidateRule.PremiseFields.All(x => currentFieldIds.Contains(x.OlapFieldId)))
                {
                    foreach (var conclusion in candidateRule.ConclusionFields)
                    {
                        if (currentFieldIds.Contains(conclusion.OlapFieldId))
                        {
                            continue;
                        }

                        fieldWeghts[conclusion.OlapFieldId] = candidateRule.Confidence * candidateRule.Support * candidateRule.PremiseFields.Count;
                    }
                }

            }

            return fieldWeghts.Select(x => new OlapFielsSuggestion() { Field = _fieldsById[x.Key], Weight = x.Value }).ToList();
        }
    }

    public class OlapFielsSuggestion
    {
        public OlapField Field { get; set; }
        public double Weight { get; set; }
    }
}
