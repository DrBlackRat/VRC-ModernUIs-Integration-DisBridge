using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.Networking;
using VRC.Udon.Editor;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram.UI;

namespace Tests
{

    public class GraphNodeTests
    {
        //[TestCase("UnityEngine")] // Disabled because Unity's docs don't cover every member that's public. AI.NavMeshAgent has several holes for example.
        [TestCase("System")]
        public async Task CheckHelpUrlsForNodes(string nameSpace)
        {
            var systemRegistry = UdonEditorManager.Instance
                .GetTopRegistries()
                .First(r=>r.Key.Equals(nameSpace))
                .Value;
            
            foreach (var keyValuePair in systemRegistry)
            {
                var registry = keyValuePair.Value;
                var definitions = registry.GetNodeDefinitions();
                foreach (var definition in definitions)
                {
                    // Skip some links which we hide away
                    if (!UdonGraphExtensions.ShouldShowDocumentationLink(definition)) continue;
                    
                    var link = UdonGraphExtensions.GetDocumentationLink(definition);
                    UnityWebRequest.Result result = await CheckUrlResolves(link);
                    Assert.That(result, Is.EqualTo(UnityWebRequest.Result.Success), $"URL does not resolve for {definition.fullName}: {link}");
                }
            }

            Assert.Pass($"All UdonNodeDefinitions under \"{nameSpace}\" have valid links!");
        }
        
        private async Task<UnityWebRequest.Result> CheckUrlResolves(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                request.timeout = 1;
                request.SendWebRequest();
                while (!request.isDone)
                {
                    await Task.Yield();
                }

                return request.result;
            }
        }
    }

}