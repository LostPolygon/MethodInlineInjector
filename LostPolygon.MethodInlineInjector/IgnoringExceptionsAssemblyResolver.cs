using log4net;
using Mono.Cecil;

namespace LostPolygon.MethodInlineInjector {
    public class IgnoringExceptionsAssemblyResolver : DefaultAssemblyResolver {
        private static readonly ILog Log = LogManager.GetLogger(nameof(IgnoringExceptionsAssemblyResolver));

        public override AssemblyDefinition Resolve(AssemblyNameReference name) {
            try {
                return base.Resolve(name);
            } catch {
                Log.InfoFormat("Assembly reference {0} not resolved", name);
                return null;
            }
        }
    }
}