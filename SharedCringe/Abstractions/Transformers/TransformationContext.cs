using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace SharedCringe.Abstractions.Transformers;

public readonly record struct TransformationContext(ModuleDef Module, ModuleWriterOptions WriterOptions);
