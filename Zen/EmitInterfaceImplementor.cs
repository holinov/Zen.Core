using System;
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

        public EmitInterfaceImplementor(AppScope scope)
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
            var interfaceType = typeof(TInterface);
            if (!Types.ContainsKey(interfaceType))
            {
                IsDirty = true;
                var typeBuilder = ModuleBuilder.DefineType("Implementor" + interfaceType.Name,
                                                           TypeAttributes.Class | TypeAttributes.Public |
                                                           TypeAttributes.AutoLayout | TypeAttributes.AnsiClass |
                                                           TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit);

                typeBuilder.AddInterfaceImplementation(interfaceType);
                MakeAppScopeField(typeBuilder);
                ImplementConstructor(typeBuilder);
                foreach (var propertyInfo in interfaceType.GetProperties())
                {
                    ImplementProperty(typeBuilder, propertyInfo);
                }

                Types[interfaceType] = typeBuilder.CreateType();
            }
            var type = Types[interfaceType];
            var inst = Activator.CreateInstance(type, _scope);
            return (TInterface)inst;
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

        private void ImplementConstructor(TypeBuilder typeBuilder)
        {
            ConstructorBuilder ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public |
                                                                           MethodAttributes.HideBySig |
                                                                           MethodAttributes.SpecialName |
                                                                           MethodAttributes.RTSpecialName,
                                                                           CallingConventions.Standard,
                                                                           new Type[] { typeof(IAppScope) });
            ILGenerator ctorILGen = ctorBuilder.GetILGenerator();
            ctorILGen.Emit(OpCodes.Ldarg_0);
            ctorILGen.Emit(OpCodes.Ldarg_1);
            ctorILGen.Emit(OpCodes.Stfld, _fieldScopeBuilder);
            ctorILGen.Emit(OpCodes.Ret);
        }
    }
}