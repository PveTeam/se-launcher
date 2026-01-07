using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace CringeBootstrap.Transformers;

public readonly record struct TransformationContext(ModuleDefMD Module, ModuleWriterOptions WriterOptions);