using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

namespace DotnetPatcher.Decompile
{
	public class EmbeddedAssemblyResolver : IAssemblyResolver
	{
		private readonly PEFile baseModule;
		private readonly UniversalAssemblyResolver _resolver;
		private readonly Dictionary<string, PEFile> cache = new Dictionary<string, PEFile>();

		public EmbeddedAssemblyResolver(PEFile baseModule, string targetFramework, string sourceOutputDirectory)
		{
			this.baseModule = baseModule;
			_resolver = new UniversalAssemblyResolver(baseModule.FileName, true, targetFramework, streamOptions: PEStreamOptions.PrefetchMetadata);
			_resolver.AddSearchDirectory(Path.GetDirectoryName(baseModule.FileName));
		}

		public PEFile? Resolve(IAssemblyReference name)
		{
			lock (this)
			{
				if (cache.TryGetValue(name.FullName, out PEFile module))
					return module;

				string resName = name.Name + ".dll";
				Resource res = baseModule.Resources.Where(r => r.ResourceType == ResourceType.Embedded).SingleOrDefault(r => r.Name.EndsWith(resName));
				if (res != null)
					module = new PEFile(res.Name, res.TryOpenStream());

                module ??= (PEFile)_resolver.Resolve(name);

				cache[name.FullName] = module;

				return module;
			}
		}

        MetadataFile? IAssemblyResolver.Resolve(IAssemblyReference reference)
            => Resolve(reference);

        public MetadataFile? ResolveModule(MetadataFile mainModule, string moduleName)
            => _resolver.ResolveModule(mainModule, moduleName);

        Task<MetadataFile?> IAssemblyResolver.ResolveAsync(IAssemblyReference reference)
            => Task.Run(() => (MetadataFile?)Resolve(reference));

        public Task<MetadataFile?> ResolveModuleAsync(MetadataFile mainModule, string moduleName)
            => Task.Run(() => ResolveModule(mainModule, moduleName));
    }
}
