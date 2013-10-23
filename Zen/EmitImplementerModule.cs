using Autofac;

namespace Zen
{
    /// <summary>
    /// Модуль обеспечивающий работу EmitInterfaceImplementor
    /// </summary>
    public class EmitImplementerModule:Module
    {
        protected override void Load(ContainerBuilder b)
        {
            b.RegisterGeneric(typeof(EmitInterfaceImplementor<>))
             .AsSelf()
             .InstancePerLifetimeScope();
        }        
    }
}