using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Serialization
{
    public class SerializationHelper
    {
        private GraphManager _graphManager;
        private ProjectConfig _projectConfig;

        public SerializationHelper(ProjectConfig projectConfig, GraphManager graphManager)
        {
            _graphManager = graphManager;
            _projectConfig = projectConfig;
        }

        public MssqlModelElement LoadElementModel(string refPath, bool loadDefinitions = true)
        {
            ConfigManager.Log.Info("Deserializing " + refPath);
            BIDocModelStored modelStored = new BIDocModelStored(_projectConfig.ProjectConfigId, refPath, _graphManager);
            modelStored.LoadDefinitions = loadDefinitions;
            FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
            IReflectionHelper reflection = new JsonReflectionHelper(new ModelActivator());
            ModelConverter converterTo = new ModelConverter(reflection);
            MssqlModelElement convertedElement = converterFrom.Convert(converterTo, modelStored.RootId);
            return convertedElement;
        }

        public MssqlModelElement LoadElementModelToChildrenOfType(string refPath, Type leafElementType)
        {
            ConfigManager.Log.Info("Deserializing " + refPath);
            BIDocModelStored modelStored = new BIDocModelStored(_projectConfig.ProjectConfigId, 
                refPath, leafElementType, _graphManager);
            FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
            IReflectionHelper reflection = new JsonReflectionHelper(new ModelActivator());
            ModelConverter converterTo = new ModelConverter(reflection);
            MssqlModelElement convertedElement = converterFrom.Convert(converterTo, modelStored.RootId);
            return convertedElement;
        }

        public MssqlModelElement LoadElementModelToChildrenOfTypesNoDef(string refPath, List<Type> leafElementTypes)
        {
            ConfigManager.Log.Info("Deserializing " + refPath);
            BIDocModelStored modelStored = new BIDocModelStored(_projectConfig.ProjectConfigId,
                refPath, leafElementTypes, _graphManager);
            modelStored.LoadDefinitions = false;
            FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
            IReflectionHelper reflection = new JsonReflectionHelper(new ModelActivator());
            ModelConverter converterTo = new ModelConverter(reflection);
            MssqlModelElement convertedElement = converterFrom.Convert(converterTo, modelStored.RootId);
            return convertedElement;
        }

        public Dictionary<MssqlModelElement, int> CreatePremappedModel(MssqlModelElement element)
        {
            Dictionary<MssqlModelElement, int> map = new Dictionary<MssqlModelElement, int>();
            map.Add(element, element.Id);
            foreach (var child in element.Children)
            {
                var childMap = CreatePremappedModel(child);
                foreach (var cmi in childMap)
                {
                    if (!map.ContainsKey(cmi.Key))
                    {
                        map.Add(cmi.Key, cmi.Value);
                    }
                    else
                    {
                    }
                }
            }
            //var childCollection = element.Children.SelectMany(x => CreatePremappedModel(x))
            //    .ToDictionary(x => x.Key, y => y.Value);
            //childCollection[element] = element.Id;
            //return childCollection;
            return map;
        }

        /// <summary>
        /// Saves a model element to the DB.
        /// </summary>
        /// <param name="element">the element to be serialized</param>
        /// <param name="premappedModel">the elements that this element may refer to that are already saved in the DB</param>
        /// <returns>the mapping of newly saved elements to their DB IDs</returns>
        public Dictionary<MssqlModelElement, int> SaveModelPart(MssqlModelElement element, Dictionary<MssqlModelElement, int> premappedModel, bool enableUpdate = false)
        {
            ConfigManager.Log.Important(string.Format("Converting {0} to DB format", element.RefPath.Path));
            var premappedTargetIds = new HashSet<int>(premappedModel.Values);

            BIDocModelBulk modelBulk = new BIDocModelBulk(_graphManager, premappedTargetIds);
            modelBulk.EnableUpdate = enableUpdate;
            ToBIDocModelConverter modelConverterTo = new ToBIDocModelConverter(modelBulk);
            ModelConverter modelConverterFrom = new ModelConverter(new JsonReflectionHelper(new ModelActivator()));
            // map from elements to preassigned BIDocModelElement IDs
            var conversionMap = modelConverterFrom.Convert(element, modelConverterTo, premappedModel);

            ConfigManager.Log.Important(string.Format("Persisting {0} model", element.RefPath.Path));
            // map from preassigned IDs to actual saved DBs
            var dbElementIdMap = modelBulk.UpdateModel(_projectConfig.ProjectConfigId);

            var savedElementMap = conversionMap.ToDictionary(x => x.Key, y => dbElementIdMap[y.Value]);
            return savedElementMap;
        }

        //public Dictionary<MssqlModelElement, int> GetModelIdMap(MssqlModelElement element)
        //{
        //    Dictionary<MssqlModelElement, int> res = new Dictionary<MssqlModelElement, int>();

        //    res.Add(element, element.Id);

        //    foreach (var child in element.Children)
        //    {
        //        var childMap = GetModelIdMap(child);
        //        foreach (var childMapItem in childMap)
        //        {
        //            res.Add(childMapItem.Key, childMapItem.Value);
        //        }
        //    }

        //    return res;
        //}
    }
}
