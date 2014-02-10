using System;
using System.Globalization;
using System.Linq;
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
					ImplementMethods(interfaceType, typeBuilder);

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

	    private void ImplementMethods(Type interfaceType, TypeBuilder typeBuilder)
	    {
			var props = interfaceType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
			foreach (var propertyInfo in props)
			{
				var attr = Attribute.GetCustomAttribute(propertyInfo, typeof(MethodProxyAttribute)) as MethodProxyAttribute;
				if (attr != null)
					ImplementMethod(typeBuilder, propertyInfo, attr.GetMethod(propertyInfo.Name));
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
		private void ImplementMethod(TypeBuilder typeBuilder, MethodInfo propertyInfo, MethodInfo targetMethodInfo)
		{
			//.method public hidebysig newslot virtual final 
			//		instance string  SomeMethod(int32 a,
			//									int32 b) cil managed
			//{
			//  // Размер кода:       19 (0x13)
			//  .maxstack  8
			//  IL_0000:  ldarg.0
			//  IL_0001:  ldfld      class [Zen]Zen.IAppScope Zen.Tests.AppScopeTests/TestClass1b::_scope
			//  IL_0006:  callvirt   instance !!0 [Zen]Zen.IAppScope::Resolve<class Zen.Tests.AppScopeTests/TestClass1>()
			//  IL_000b:  ldarg.1
			//  IL_000c:  ldarg.2
			//  IL_000d:  callvirt   instance string Zen.Tests.AppScopeTests/TestClass1::SomeMethod(int32,
			//																					  int32)
			//  IL_0012:  ret
			//} // end of method TestClass1b::SomeMethod

			var targetType = targetMethodInfo.DeclaringType;
			//var prop = typeBuilder.GetProperties().FirstOrDefault(x => x.PropertyType == targetType);
			//if (prop == null)
			//{
			//	prop = typeBuilder.DefineProperty(
			//		GetUniqueName(typeBuilder, targetType.Name),
			//		PropertyAttributes.None,
			//		targetType,
			//		new Type[0]);
			//}

			var propBuilder = typeBuilder.DefineMethod(propertyInfo.Name,
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, 
				propertyInfo.ReturnType, propertyInfo.GetParameters().Select(x=>x.ParameterType).ToArray());
			var ilGen = propBuilder.GetILGenerator();
			var methodParams = propertyInfo.GetParameters();
			var targetMethodParams = targetMethodInfo.GetParameters();
			ilGen.Emit(OpCodes.Ldarg_0);

			ilGen.Emit(OpCodes.Ldfld, _fieldScopeBuilder);
			var genMethod = ResolveMethodInfo.MakeGenericMethod(targetType);
			ilGen.EmitCall(OpCodes.Callvirt, genMethod, null);
			if (targetMethodParams.Length != methodParams.Length)
				throw new MissingMethodException(targetMethodParams.Length + " != " + methodParams.Length);
			for (int i = 0; i < methodParams.Length; ++i)
			{
				if (targetMethodParams[i].ParameterType != methodParams[i].ParameterType)
					throw new MissingMethodException(targetMethodParams[i].ParameterType + " != " + methodParams[i].ParameterType);
				if (i == 0)
					ilGen.Emit(OpCodes.Ldarg_1);
				else if (i == 1)
					ilGen.Emit(OpCodes.Ldarg_2);
				else if (i == 2)
					ilGen.Emit(OpCodes.Ldarg_3);
				else
					ilGen.Emit(OpCodes.Ldarg, i + 1);
			}
			ilGen.EmitCall(OpCodes.Callvirt, targetMethodInfo, null);
			ilGen.Emit(OpCodes.Ret);
		}

	    private string GetUniqueName(TypeBuilder typeBuilder, string name)
	    {
		    int i = 0;
		    var newName = name;			
		    for (;;)
		    {
			    if (typeBuilder.GetProperty(newName) == null) return newName;
			    ++i;
			    newName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, i);
		    }
		    
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