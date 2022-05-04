using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc.Extensions;
using Xbim.Ifc4.Interfaces;

namespace IFCextracter
{
    internal static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        public static XbimMatrix3D GetTransformation(this IIfcProduct product)
        {
            List<XbimMatrix3D> matrixList = new List<XbimMatrix3D>();
            XbimMatrix3D result = new XbimMatrix3D();
            IIfcLocalPlacement lp = product.ObjectPlacement as IIfcLocalPlacement;

            if (lp == null) { return XbimMatrix3D.Identity; }

            IIfcLocalPlacement relPlace = lp.PlacementRelTo as IIfcLocalPlacement;
            XbimMatrix3D objTr = lp.RelativePlacement.ToMatrix3D();
            matrixList.Add(objTr);

            while (relPlace != null)
            {
                XbimMatrix3D tr = relPlace.RelativePlacement.ToMatrix3D();
                matrixList.Add(tr);
                relPlace = relPlace.PlacementRelTo as IIfcLocalPlacement;
            }

            foreach (XbimMatrix3D m in matrixList) { result = XbimMatrix3D.Multiply(result, m); }

            return result;
        }
        static void Main()
        {
            const string fileName = "C:\\GoldenNugget.ifc";
            using (var model = IfcStore.Open(fileName))


            {
                // get all doors in the model (using IFC4 interface of IfcDoor - this will work both for IFC2x3 and IFC4)
                var allBuildings = model.Instances.OfType<IIfcBuildingElement>();
                Console.WriteLine(allBuildings.Count()) ;
                foreach (var Building in allBuildings)
                {
                    Console.WriteLine(Building.Name);
                    Console.WriteLine(GetTransformation(Building));
                 
                
                    
                    var properties = Building.IsDefinedBy
                    .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
                    .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
                    .OfType<IIfcPropertySingleValue>();
                    foreach (var property in properties)
                        Console.WriteLine($"Property: {property.Name}, Value: {property.NominalValue}");
                }
                

            }
        
        }
    }
}
