using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Zen
{
    /// <summary>
    /// Фабрика классов реализующих интерфейс через разрешение зависимостей
    /// </summary>
    /// <typeparam name="TInterface">Тип интерфейса</typeparam>
    public class EmitInterfaceImplementor<TInterface> : EmitInterfaceImplementorBase
    {
        private FieldBuilder _fieldScopeBuilder;
        private readonly AppScope _scope;

        public EmitInterfaceImplementor(AppCore scope)
        {
            this._scope = scope;
        }

        /// <summary>
        /// Получить реализацию интерфейса
        /// </summary>
        /// <param name="scope">Область видимости для IoC</param>
        /// <returns>Экземпляр класса реализующий интерфейс</returns>
        public TInterface ImplementInterface()
        {
            var interfaceType = typeof (TInterface);
            lock (_locker)
            {
                if (!Types.ContainsKey(interfaceType))
                {
                    IsDirty = true;
                    var typeBuilder = ModuleBuilder.DefineType("Implementor" + interfaceType.Name,
                                                               TypeAttributes.Class | TypeAttributes.Public |
                                                               TypeAttributes.AutoLayout | TypeAttributes.AnsiClass |
                                                               TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

                    typeBuilder.AddInterfaceImplementation(interfaceType);
                    var disposableInterface = interfaceType.GetInterface(typeof (IDisposable).Name);
                    var disposableType = disposableInterface != null;

                    MakeAppScopeField(typeBuilder);
                    ImplementConstructor(typeBuilder, disposableType);
                    ImplementInterfaces(interfaceType, typeBuilder);

                    if (disposableType)
                    {
                        ImplementDispose(typeBuilder);
                    }

                    Types[interfaceType] = typeBuilder.CreateType();
                }
                var type = Types[interfaceType];
                var inst = Activator.CreateInstance(type, _scope);

                return (TInterface) inst;
            }
        }

        private void ImplementInterfaces(Type interfaceType, TypeBuilder typeBuilder)
        {
            var props = interfaceType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in props)
            {
                ImplementProperty(typeBuilder, propertyInfo);
            }

            foreach (var ancestorInterface in interfaceType.GetInterfaces())
            {
                ImplementInterfaces(ancestorInterface, typeBuilder);
            }
        }

        private void ImplementDispose(TypeBuilder typeBuilder)
        {
            const MethodAttributes attributes =
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                MethodAttributes.Virtual | MethodAttributes.Final;
            var disposeBuilder = typeBuilder.DefineMethod("Dispose", attributes, CallingConventions.HasThis,
                                                          typeof (void), null);
            var ilGen = disposeBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, _fieldScopeBuilder);

            var genMethod = typeof (IDisposable).GetMethod("Dispose");
            ilGen.EmitCall(OpCodes.Callvirt, genMethod, null);
            ilGen.Emit(OpCodes.Ret);
        }

        private void ImplementProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
        {
            var propBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None,
                                                         propertyInfo.PropertyType, new Type[0]);

            const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            var getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyInfo.Name, attributes,
                                                            CallingConventions.HasThis, propertyInfo.PropertyType,
                                                            null);
            var ilGen = getMethodBuilder.GetILGenerator();
            ilGen.DeclareLocal(propertyInfo.PropertyType);
            //ilGen.Emit(OpCodes.Nop);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, _fieldScopeBuilder);

            var genMethod = ResolveMethodInfo.MakeGenericMethod(propertyInfo.PropertyType);
            ilGen.EmitCall(OpCodes.Callvirt, genMethod, null);

            ilGen.Emit(OpCodes.Stloc_0);
            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Ret);

            propBuilder.SetGetMethod(getMethodBuilder);
        }

        private void MakeAppScopeField(TypeBuilder typeBuilder)
        {
            _fieldScopeBuilder = typeBuilder.DefineField("_scope", typeof(IAppScope), FieldAttributes.Private);
        }

        private void ImplementConstructor(TypeBuilder typeBuilder, bool disposableType)
        {
            ConstructorBuilder ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public |
                                                                           MethodAttributes.HideBySig |
                                                                           MethodAttributes.SpecialName |
                                                                           MethodAttributes.RTSpecialName,
                                                                           CallingConventions.Standard,
                                                                           new Type[] { typeof(IAppScope) });
            ILGenerator ctorIlGen = ctorBuilder.GetILGenerator();
            if (disposableType) MakeDisposableConstructor(ctorIlGen);
            else MakeNonDisposableConstructor(ctorIlGen);
        }

        private void MakeDisposableConstructor(ILGenerator ctorIlGen)
        {
            ctorIlGen.Emit(OpCodes.Ldarg_0);
            ctorIlGen.Emit(OpCodes.Ldarg_1);
            var methodInfo = typeof (IAppScope).GetMethod("BeginScope", new Type[0]);
            ctorIlGen.EmitCall(OpCodes.Callvirt, methodInfo, null);
            ctorIlGen.Emit(OpCodes.Stfld, _fieldScopeBuilder);
            ctorIlGen.Emit(OpCodes.Ret);
        }

        private void MakeNonDisposableConstructor(ILGenerator ctorIlGen)
        {
            ctorIlGen.Emit(OpCodes.Ldarg_0);
            ctorIlGen.Emit(OpCodes.Ldarg_1);
            ctorIlGen.Emit(OpCodes.Stfld, _fieldScopeBuilder);
            ctorIlGen.Emit(OpCodes.Ret);
        }
    }

    /*public interface IUowSample:IDisposable
    {
        Config Config { get; }
    }

    public class UnitOfWorkExample:IUowSample
    {
        private IAppScope _scope;

        public UnitOfWorkExample(IAppScope scope)
        {
            _scope = scope.BeginScope();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public Config Config { get { return _scope.Resolve<Config>(); } }
    }*/
}