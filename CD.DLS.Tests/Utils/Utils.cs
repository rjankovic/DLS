using System;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CD.DLS.Tests.Utils
{
    [TestClass]
    public class Utils
    {
        [TestMethod]
        public void MergeManifests()
        {
            var targetPath = "C:\\DLS_Extract\\MIS\\7d4ef203-b98b-40e1-b666-b449c6cc1cdd\\manifest.json";
            var partPath = "C:\\DLS_Extract\\MIS\\84b5bc4a-f35e-4c42-8e87-e404996c4e6b\\manifest.json";

            var targetManifest = DAL.Objects.Extract.Manifest.Deserialize(File.ReadAllText(targetPath));
            var partManifest = DAL.Objects.Extract.Manifest.Deserialize(File.ReadAllText(partPath));

            targetManifest.Merge(partManifest);

            File.WriteAllText("C:\\DLS_Extract\\MIS\\7d4ef203-b98b-40e1-b666-b449c6cc1cdd\\manifest_merged.json", targetManifest.Serialize());

            
        }

        [TestMethod]
        public void Zip()
        {
            ZipFile.CreateFromDirectory("C:\\DLS_Extract\\MIS\\7d4ef203-b98b-40e1-b666-b449c6cc1cdd", "C:\\DLS_Extract\\MIS\\MIS_1.zip", CompressionLevel.Optimal, false);
        }
    }
}
