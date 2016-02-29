using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;


namespace Phonatech
{
    public class GenerateTowerRanges : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public GenerateTowerRanges()
        {
        }

        protected override void OnClick()
        {
             IMxDocument pMxdoc = (IMxDocument) ArcMap.Application.Document;

            IFeatureLayer pfeaturelayer = (IFeatureLayer) pMxdoc.ActiveView.FocusMap.Layer[0];
            IDataset pDS = (IDataset) pfeaturelayer.FeatureClass;
            TowerManager tm = new TowerManager(pDS.Workspace);
            Tower pTower = tm.GetTowerByID("T04");
            //range of 100 meters 
            double towerRange = 100;

            ITopologicalOperator pTopo = (ITopologicalOperator) pTower.TowerLocation ;

            IPolygon range3Bars = (IPolygon) pTopo.Buffer(towerRange / 3);
            IPolygon range2BarsWhole = (IPolygon)pTopo.Buffer((towerRange * 2) / 3);
            IPolygon range1BarsWhole = (IPolygon)pTopo.Buffer(towerRange);
             

            ITopologicalOperator pIntTopo= (ITopologicalOperator) range2BarsWhole;

            ITopologicalOperator pIntTopo1 = (ITopologicalOperator)range1BarsWhole;


            IPolygon range2BarsDonut = (IPolygon)pIntTopo.SymmetricDifference (range3Bars); //,esriGeometryDimension.esriGeometry2Dimension); 
            IPolygon range1BarsDonut = (IPolygon)pIntTopo1.SymmetricDifference(range2BarsWhole); //,esriGeometryDimension.esriGeometry2Dimension); 
             
            IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit) pDS.Workspace;
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)pWorkspaceEdit;
            IFeatureClass pTowerRangeFC =  pFWorkspace.OpenFeatureClass("TowerRange");
            IFeature pFeature= pTowerRangeFC.CreateFeature();

            pFeature.set_Value(pFeature.Fields.FindField("TOWERID"), "T04");
            pFeature.set_Value(pFeature.Fields.FindField("RANGE"), 3);
           
            pFeature.Shape = range3Bars;
            pFeature.Store();


            IFeature pFeature2Bar = pTowerRangeFC.CreateFeature();

            pFeature2Bar.set_Value(pFeature.Fields.FindField("TOWERID"), "T04");
            pFeature2Bar.set_Value(pFeature.Fields.FindField("RANGE"), 2);

            pFeature2Bar.Shape = range2BarsDonut;
            pFeature2Bar.Store();




            IFeature pFeature1Bar = pTowerRangeFC.CreateFeature();

            pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("TOWERID"), "T04");
            pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("RANGE"), 1);

            pFeature1Bar.Shape = range1BarsDonut;
            pFeature1Bar.Store();

                  
             pWorkspaceEdit.StopEditOperation();
             pWorkspaceEdit.StopEditing(true);


        }

        protected override void OnUpdate()
        {
        }
    }
}
