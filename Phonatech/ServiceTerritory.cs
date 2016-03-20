using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonatech
{
    public class ServiceTerritory
    {

        public IFeature ServiceTerritoryFeature { get; set; }
       
        public ServiceTerritory(IWorkspace workspace, string name)
        {
                
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)workspace;
            IFeatureClass pServiceTerritory = pFWorkspace.OpenFeatureClass("ServiceTerritory");
            IQueryFilter pQFilter = new QueryFilter();
            pQFilter.WhereClause = "NAME='" + name +"'";

            IFeatureCursor pFCursor= pServiceTerritory.Search(pQFilter, false);
            ServiceTerritoryFeature = pFCursor.NextFeature();
             
        }

        public void updateCoverages(double deadCoverage, double receptionCoverage)
        {
              IWorkspaceEdit pWorkspaceEdit = null;
            try
            {

             pWorkspaceEdit = (IWorkspaceEdit)((IDataset)ServiceTerritoryFeature.Class).Workspace;
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            ServiceTerritoryFeature.set_Value(ServiceTerritoryFeature.Fields.FindField("DEADCOVERAGE"), deadCoverage);
            ServiceTerritoryFeature.set_Value(ServiceTerritoryFeature.Fields.FindField("RECEPTIONCOVERAGE"), receptionCoverage);
            ServiceTerritoryFeature.Store();

            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);
                
            }
            catch
            {
                pWorkspaceEdit.AbortEditOperation();
            }
        }

    }
}
