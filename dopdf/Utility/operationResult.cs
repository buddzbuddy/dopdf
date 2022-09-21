using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace dopdf.Utility
{
    public class operationResult
    {
        public bool isSuccess { get; private set; }

        public object data { get; }

        public List<string> errors { get; private set; }
        public string message { get; private set; }

        private operationResult(object data)
        {
            this.data = data;
        }

        private operationResult()
        {
        }

        public static operationResult success()
        {
            var result = new operationResult();
            result.isSuccess = true;
            return result;
        }

        public static operationResult success(object data)
        {
            var result = new operationResult(data);
            result.isSuccess = true;
            return result;
        }

        public static operationResult success(string msg)
        {
            var result = new operationResult();
            result.isSuccess = true;
            result.message = msg;
            return result;
        }

        public static operationResult success(object data, string msg)
        {
            var result = new operationResult(data);
            result.isSuccess = true;
            result.message = msg;
            return result;
        }

        public static operationResult failure()
        {
            var result = new operationResult();
            result.isSuccess = false;
            result.errors = new List<string>();

            return result;
        }

        public operationResult addError(params string[] errorMessages)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }
            errors.AddRange(errorMessages);

            return this;
        }
    }
    public static class Extensions
    {
        static Extensions()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Web"), AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        }

        public static object Spread(this object obj, params object[] anotherObject)
        {
            var properties = anotherObject
                .Concat(new[] { obj })
                .SelectMany(o => o.GetType().GetProperties(), (o, p) => (o: o, p: p))
                .ToDictionary(t => t.p.Name, t => (propName: t.p.Name, propType: t.p.PropertyType, propValue: t.p.GetValue(t.o)));

            var objType = CreateClass(properties);

            var finalObj = Activator.CreateInstance(objType);
            foreach (var prop in objType.GetProperties())
                prop.SetValue(finalObj, properties[prop.Name].propValue);

            return finalObj;
        }

        const MethodAttributes METHOD_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        private static ModuleBuilder ModuleBuilder;

        internal static Type CreateClass(IDictionary<string, (string propName, Type type, object)> parameters)
        {
            var typeBuilder = ModuleBuilder.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, null);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            foreach (var parameter in parameters)
                CreateProperty(typeBuilder, parameter.Key, parameter.Value.type);
            var type = typeBuilder.CreateTypeInfo().AsType();
            return type;
        }

        private static PropertyBuilder CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propBuilder.SetGetMethod(DefineGet(typeBuilder, fieldBuilder, propBuilder));
            propBuilder.SetSetMethod(DefineSet(typeBuilder, fieldBuilder, propBuilder));

            return propBuilder;
        }

        private static MethodBuilder DefineSet(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, PropertyBuilder propBuilder)
            => DefineMethod(typeBuilder, $"set_{propBuilder.Name}", null, new[] { propBuilder.PropertyType }, il =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, fieldBuilder);
                il.Emit(OpCodes.Ret);
            });

        private static MethodBuilder DefineGet(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, PropertyBuilder propBuilder)
            => DefineMethod(typeBuilder, $"get_{propBuilder.Name}", propBuilder.PropertyType, Type.EmptyTypes, il =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ret);
            });

        private static MethodBuilder DefineMethod(TypeBuilder typeBuilder, string methodName, Type propertyType, Type[] parameterTypes, Action<ILGenerator> bodyWriter)
        {
            var methodBuilder = typeBuilder.DefineMethod(methodName, METHOD_ATTRIBUTES, propertyType, parameterTypes);
            bodyWriter(methodBuilder.GetILGenerator());
            return methodBuilder;
        }
    }
}
