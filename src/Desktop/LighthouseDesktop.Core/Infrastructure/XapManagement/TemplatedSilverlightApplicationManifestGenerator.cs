using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LighthouseDesktop.Core.Infrastructure.XapManagement
{
    public interface ITemplatedSilverlightApplicationManifestGenerator
    {
        string ManifestTemplate { get; set; }
        string AdditionalAssembyPartsInjectionPlaceholder { get; set; }
        void AddAssemblyPartItem(ManifestAssemblyPartItem item);
        string GenerateNewApplicationManifest();
    }

    public class TemplatedSilverlightApplicationManifestGenerator : ITemplatedSilverlightApplicationManifestGenerator
    {
        public string ManifestTemplate { get; set; }

        private string _additionalAssembyPartsInjectionPlaceholder = "{ADDITIONAL_ASSEMBLY_PARTS}";
        public string AdditionalAssembyPartsInjectionPlaceholder
        {
            get { return _additionalAssembyPartsInjectionPlaceholder; }
            set { _additionalAssembyPartsInjectionPlaceholder = value; }
        }

        private List<ManifestAssemblyPartItem> _assemblyPartsToAdd = new List<ManifestAssemblyPartItem>();
        private List<ManifestAssemblyPartItem> AssemblyPartsToAdd
        {
            get { return _assemblyPartsToAdd; }
            set { _assemblyPartsToAdd = value; }
        }

        public void AddAssemblyPartItem(ManifestAssemblyPartItem item)
        {
            if (!AssemblyPartsToAdd.Any(p => p.Source.ToLower() == item.Source.ToLower()))
            {
                AssemblyPartsToAdd.Add(item);
            }
        }

        public string GenerateNewApplicationManifest()
        {
            var newAssemblyPartsContent = new StringBuilder();

            foreach (var newAssemblyPart in AssemblyPartsToAdd)
            {
                var newItemContent = string.Format(@"        <AssemblyPart x:Name=""{0}"" Source=""{1}"" />", newAssemblyPart.Name, newAssemblyPart.Source);
                newAssemblyPartsContent.AppendLine(newItemContent);
            }

            var newManifestContent = ManifestTemplate.Replace(AdditionalAssembyPartsInjectionPlaceholder, newAssemblyPartsContent.ToString());
            return newManifestContent;
        }
    }
}
