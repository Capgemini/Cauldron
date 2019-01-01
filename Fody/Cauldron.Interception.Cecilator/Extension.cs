using Cauldron.Interception.Cecilator.Coders;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Cauldron.Interception.Cecilator
{
    public static class Extension
    {
        public static void AddRange<T>(this Mono.Collections.Generic.Collection<T> typeReferences, IEnumerable<T> genericTypes)
        {
            foreach (var item in genericTypes)
                typeReferences.Add(item);
        }

        public static bool AreEqual(this Type a, ModuleDefinition b) => a?.Assembly.GetName().Name.AreEqual(b?.Assembly.Name.Name) ?? false;

        public static bool AreEqual(this Type a, Type b) => a?.Assembly.FullName.AreEqual(b?.Assembly.FullName) ?? false;

        public static bool AreEqual(this ModuleDefinition a, Type b) => a?.Assembly.Name.Name.AreEqual(b?.Assembly.GetName().Name) ?? false;

        public static bool AreEqual(this ModuleDefinition a, ModuleDefinition b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return a.Assembly.AreEqual(b.Assembly);
        }

        public static bool AreEqual(this AssemblyDefinition a, AssemblyDefinition b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            return a.Name.Name.AreEqual(b.Name.Name);
        }

        public static bool AreEqual(this TypeDefinition a, TypeDefinition b) => AreEqual(a as TypeReference, b as TypeReference);

        public static bool AreEqual(this TypeReference a, TypeReference b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            if (a.IsGenericParameter && b.IsGenericParameter)
                return a.FullName == b.FullName;
            try
            {
                if (a is TypeDefinition && b is TypeDefinition)
                    return AreEqual(a.BetterResolve()?.Module, b.BetterResolve()?.Module) && a.BetterResolve().FullName == b.BetterResolve().FullName;

                if (a is TypeDefinition && b is TypeReference)
                    return AreEqual(a.BetterResolve()?.Module, b.BetterResolve()?.Module) && a.FullName == b.BetterResolve().FullName;

                if (a is TypeReference && b is TypeDefinition)
                    return AreEqual(a.BetterResolve()?.Module, b.BetterResolve()?.Module) && a.BetterResolve().FullName == b.FullName;

                return AreEqual(a.BetterResolve()?.Module, b.BetterResolve()?.Module) && a.FullName == b.FullName;
            }
            catch (Exception e)
            {
                throw new Exception($"a: {a?.FullName}, b: {b?.FullName}", e);
            }
        }

        public static bool AreEqual(this TypeReference a, TypeDefinition b) => AreEqual(a, b as TypeReference);

        public static bool AreEqual(this Type a, TypeDefinition b) => AreEqual(a, b.BetterResolve()?.Module) && a.FullName == b.FullName;

        public static bool AreEqual(this Type a, BuilderType b) =>
            a.AreEqual(b.typeReference) ||
            a.AreEqual(b.typeDefinition ?? b.typeReference);

        public static bool AreEqual(this Type a, TypeReference b) => a.FullName == b.FullName;

        public static bool AreEqual(this TypeReference a, BuilderType b) =>
            a.AreEqual(b.typeReference) ||
            a.AreEqual(b.typeDefinition ?? b.typeReference) ||
            (a.BetterResolve()?.AreEqual(b.typeDefinition ?? b.typeReference) ?? false);

        public static bool AreEqual(this MethodReference a, MethodReference b)
        {
            if (!a.DeclaringType.AreEqual(b.DeclaringType))
                return false;

            if (a.Name != b.Name)
                return false;

            if (a.Parameters.Count != b.Parameters.Count)
                return false;

            if (!a.ReturnType.AreEqual(b.ReturnType))
                return false;

            if (a.GenericParameters.Count != b.GenericParameters.Count)
                return false;

            for (int i = 0; i < a.Parameters.Count; i++)
                if (!a.Parameters[i].ParameterType.AreEqual(b.Parameters[i].ParameterType))
                    return false;

            for (int i = 0; i < a.GenericParameters.Count; i++)
                if (!a.GenericParameters[i].AreEqual(b.GenericParameters[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Checks if <paramref name="toBeAssigned"/> is assignable to <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to assign to.</param>
        /// <param name="toBeAssigned">The type that will be assigned to <paramref name="type"/>.</param>
        /// <returns>Returns true if <paramref name="toBeAssigned"/> is assignable to <paramref name="type"/>; otherwise false.</returns>
        public static bool AreReferenceAssignable(this BuilderType[] type, BuilderType[] toBeAssigned)
        {
            if (type == null && toBeAssigned == null)
                return true;

            if (type == null || toBeAssigned == null)
                return false;

            if (type.Length != toBeAssigned.Length)
                return false;

            for (int i = 0; i < type.Length; i++)
            {
                if (!type[i].AreReferenceAssignable(toBeAssigned[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if <paramref name="toBeAssigned"/> is assignable to <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to assign to.</param>
        /// <param name="toBeAssigned">The type that will be assigned to <paramref name="type"/>.</param>
        /// <returns>Returns true if <paramref name="toBeAssigned"/> is assignable to <paramref name="type"/>; otherwise false.</returns>
        public static bool AreReferenceAssignable(this BuilderType type, BuilderType toBeAssigned)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            // If the target type is an object... Everything goes into an object
            if (type == BuilderTypes.Object)
                return true;

            // If the type or value to be assigned is null and we are sure that the target type
            // is not a value type then it is safe to say that it is indeed assignable
            if (toBeAssigned == null && !type.IsValueType)
                return true;

            // From this point the value that has to be assigned has to be non null
            if (toBeAssigned == null) throw new ArgumentNullException(nameof(toBeAssigned));

            // If both are of the same type
            if (type == toBeAssigned)
                return true;

            // If the type to be assigened inherited from the target type then go for it
            if (!type.IsInterface && toBeAssigned.IsSubclassOf(type))
                return true;

            // If the target type is an interface lets check if the type to be assigned has it implemented.
            if (type.IsInterface && toBeAssigned.Implements(type))
                return true;

            // Special case for RuntimeType and Type in NetCore
            if (toBeAssigned.Fullname == "System.RuntimeType" && type.Fullname == "System.Type")
                return true;

            return false;
        }

        /// <summary>
        /// Checks if <paramref name="toBeAssigned"/> is assignable to <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to assign to.</param>
        /// <param name="toBeAssigned">The type that will be assigned to <paramref name="type"/>.</param>
        /// <returns>Returns true if <paramref name="toBeAssigned"/> is assignable to <paramref name="type"/>; otherwise false.</returns>
        public static bool AreReferenceAssignable(this TypeReference type, TypeReference toBeAssigned)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            // If the target type is an object... Everything goes into an object
            if (type.AreEqual((TypeReference)BuilderTypes.Object))
                return true;

            // If the type or value to be assigned is null and we are sure that the target type
            // is not a value type then it is safe to say that it is indeed assignable
            if (toBeAssigned == null && !type.IsValueType)
                return true;

            // From this point the value that has to be assigned has to be non null
            if (toBeAssigned == null) throw new ArgumentNullException(nameof(toBeAssigned));

            // If both are of the same type
            if (type.AreEqual(toBeAssigned))
                return true;

            var isInterface = type.BetterResolve()?.IsInterface ?? false;

            // If the type to be assigened inherited from the target type then go for it
            if (!isInterface && toBeAssigned.IsSubclassOf(type))
                return true;

            // If the target type is an interface lets check if the type to be assigned has it implemented.
            if (isInterface && toBeAssigned.Implements(type))
                return true;

            // Special case for RuntimeType and Type in NetCore
            if (toBeAssigned.FullName == "System.RuntimeType" && type.FullName == "System.Type")
                return true;

            return false;
        }

        /// <summary>
        /// Tries to resolve the <see cref="TypeReference"/> to its <see cref="TypeDefinition"/> using <see cref="TypeDefinition.Resolve"/>.
        /// If <see cref="TypeDefinition.Resolve"/> returns null it will try to resolve it using a list of types.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TypeDefinition BetterResolve(this TypeReference value)
        {
            if (value is TypeDefinition typeDefinition)
                return typeDefinition;

            TypeDefinition resolve()
            {
                var position = value.FullName.IndexOf('<');
                var name = value.FullName.Substring(0, position < 0 ? value.FullName.Length : position);
                var result = WeaverBase.AllTypes.FirstOrDefault(x => x.FullName.StartsWith(name));
                return result;
            }

            TypeDefinition threadSafeResolve()
            {
                lock (value.Module)
                {
                    try
                    {
                        return value.Resolve();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            try
            {
                return value.Resolve() ?? threadSafeResolve() ?? resolve();
            }
            catch
            {
                return resolve();
            }
        }

        public static MethodDefinition BetterResolve(this MethodReference value)
        {
            if (value == null)
                return null;

            if (value is MethodDefinition methodDefinition)
                return methodDefinition;

            MethodDefinition resolve()
            {
                try
                {
                    var declaringType = value.DeclaringType.BetterResolve();
                    foreach (var item in declaringType.Methods)
                    {
                        if (item.Name != value.Name)
                            continue;

                        if (item.Parameters.Count != value.Parameters.Count)
                            continue;

                        var next = false;
                        for (int i = 0; i < item.Parameters.Count; i++)
                        {
                            if (!item.Parameters[i].ParameterType.AreEqual(value.Parameters[i].ParameterType))
                            {
                                next = true;
                                break;
                            }
                        }

                        if (next)
                            continue;

                        return item;
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            }

            try
            {
                return value.Resolve() ?? resolve();
            }
            catch
            {
                return resolve();
            }
        }

        public static GenericParameter Clone(this GenericParameter genericParameter, TypeDefinition typeDefinition)
        {
            var result = new GenericParameter(typeDefinition);
            result.Attributes = genericParameter.Attributes;
            result.Name = genericParameter.Name;
            return result;
        }

        public static IEnumerable<GenericParameter> Clone(this Mono.Collections.Generic.Collection<GenericParameter> genericParameters, TypeDefinition typeDefinition)
        {
            for (int i = 0; i < genericParameters.Count; i++)
                yield return genericParameters[i].Clone(typeDefinition);
        }

        public static Builder CreateBuilder(this WeaverBase weaver)
        {
            if (weaver == null)
                throw new ArgumentNullException(nameof(weaver), $"Argument '{nameof(weaver)}' cannot be null");

            Builder.Current = new Builder(weaver);
            return Builder.Current;
        }

        public static bool EqualsEx(this string me, string other) => me.GetHashCode() == other.GetHashCode() && me == other;

        public static CustomAttribute Get(this Mono.Collections.Generic.Collection<CustomAttribute> collection, string name)
        {
            if (name.IndexOf('.') > 0)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    var fullname = collection[i].AttributeType.BetterResolve().FullName;
                    if (fullname.GetHashCode() == name.GetHashCode() && fullname == name)
                        return collection[i];
                }
            }
            else
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    var shortname = collection[i].AttributeType.BetterResolve().Name;
                    if (shortname.GetHashCode() == name.GetHashCode() && shortname == name)
                        return collection[i];
                }
            }

            return null;
        }

        public static T Get<T>(this IEnumerable<T> source, string name) where T : MemberReference
        {
            var collection = source.ToArray();
            if (name.IndexOf('.') > 0)
            {
                for (int i = 0; i < collection.Length; i++)
                {
                    var fullname = collection[i].FullName;
                    if (fullname.GetHashCode() == name.GetHashCode() && fullname == name)
                        return collection[i];
                }
            }
            else
            {
                for (int i = 0; i < collection.Length; i++)
                {
                    var shortname = collection[i].Name;
                    if (shortname.GetHashCode() == name.GetHashCode() && shortname == name)
                        return collection[i];
                }
            }

            return null;
        }

        public static IEnumerable<TypeReference> GetBaseClasses(this TypeReference type)
        {
            var typeDef = type.BetterResolve();

            if (typeDef != null && typeDef.BaseType != null)
            {
                yield return typeDef.BaseType;
                foreach (var item in GetBaseClasses(typeDef.BaseType))
                    yield return item;
            }
        }

        /// <summary>
        /// Tries to get the child type of an array of IEnumerable.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="type"></param>
        /// <returns>
        /// If the child type was successfully extracted, then <see cref="Tuple{T1, T2}.Item2"/> is true; otherwise false.
        /// <see cref="Tuple{T1, T2}.Item1"/> contains the child type; otherwise always <see cref="Object"/>
        /// </returns>
        public static (TypeReference childType, bool isSuccessful) GetChildrenType(this ModuleDefinition module, TypeReference type)
        {
            if (type.IsArray)
                return (module.ImportReference(type.GetElementType()), true);

            TypeReference getIEnumerableInterfaceChild(TypeReference typeReference)
            {
                if (typeReference is GenericInstanceType genericType)
                {
                    foreach (var item in genericType.GetGenericInstances())
                    {
                        // We have to make some exceptions to dictionaries
                        if (item.Resolve().AreEqual((TypeReference)BuilderTypes.IDictionary2.BuilderType.typeDefinition))
                            return (item as GenericInstanceType).GenericArguments[1];

                        // If we have more than 1 generic argument then we try to get a IEnumerable<>
                        // interface otherwise we just return the last argument in the list
                        if (item.Resolve().AreEqual((TypeReference)BuilderTypes.IEnumerable1.BuilderType.typeDefinition))
                            return (item as GenericInstanceType).GenericArguments[0];

                        // A Nullable special
                        if (item.Resolve().AreEqual((TypeReference)BuilderTypes.Nullable1.BuilderType.typeDefinition))
                            return (item as GenericInstanceType).GenericArguments[0];
                    }
                }
                else if (type.AreEqual((TypeReference)BuilderTypes.IEnumerable.BuilderType.typeDefinition))
                    return BuilderTypes.Object;

                return null;
            }

            var result = getIEnumerableInterfaceChild(type);

            if (result != null)
                try
                {
                    return (module.ImportReference(result), true);
                }
                catch (Exception e)
                {
                    throw new Exception($"An error has occured while trying to import the type '{result.FullName}'", e);
                }

            // We just don't know :(
            return (module.ImportReference(typeof(object)), false);
        }

        public static IReadOnlyDictionary<string, TypeReference> GetGenericResolvedTypeName(this GenericInstanceType type)
        {
            var genericArgumentNames = type.BetterResolve().GenericParameters.Select(x => x.FullName).ToArray();
            var genericArgumentsOfCurrentType = type.GenericArguments.ToArray();

            var result = new Dictionary<string, TypeReference>();

            for (int i = 0; i < genericArgumentNames.Length; i++)
                result.Add(genericArgumentNames[i], genericArgumentsOfCurrentType[i]);

            return new ReadOnlyDictionary<string, TypeReference>(result);
        }

        public static IReadOnlyDictionary<string, TypeReference> GetGenericResolvedTypeNames(this GenericInstanceType type)
        {
            var genericArgumentNames = type.BetterResolve().GenericParameters.Select(x => x.FullName).ToArray();
            var genericArgumentsOfCurrentType = type.GenericArguments.ToArray();
            var baseType = type as TypeReference;

            var result = new Dictionary<string, TypeReference>();

            while (baseType != null)
            {
                if (baseType.IsGenericInstance)
                {
                    if (baseType is GenericInstanceType genericType)
                    {
                        genericArgumentNames = genericType.BetterResolve().GenericParameters.Select(x => x.FullName).ToArray();
                        genericArgumentsOfCurrentType = genericType.GenericArguments.ToArray();

                        for (int i = 0; i < genericArgumentNames.Length; i++)
                        {
                            if (!result.ContainsKey(genericArgumentNames[i]))
                                result.Add(genericArgumentNames[i], genericArgumentsOfCurrentType[i]);
                        }
                    }
                }

                baseType = baseType.BetterResolve().BaseType;
            }

            return new ReadOnlyDictionary<string, TypeReference>(result);
        }

        public static IReadOnlyDictionary<string, TypeReference> GetGenericResolvedTypeNames(this GenericInstanceMethod method)
        {
            var genericArgumentNames = method.Resolve().GenericParameters.Select(x => x.FullName).ToArray();
            var genericArgumentsOfCurrentType = method.GenericArguments.ToArray();

            var result = new Dictionary<string, TypeReference>();
            var genericType = method as GenericInstanceMethod;

            genericArgumentNames = genericType.Resolve().GenericParameters.Select(x => x.FullName).ToArray();
            genericArgumentsOfCurrentType = genericType.GenericArguments.ToArray();

            for (int i = 0; i < genericArgumentNames.Length; i++)
            {
                if (!result.ContainsKey(genericArgumentNames[i]))
                    result.Add(genericArgumentNames[i], genericArgumentsOfCurrentType[i]);
            }

            return new ReadOnlyDictionary<string, TypeReference>(result);
        }

        public static IEnumerable<TypeReference> GetInterfaces(this TypeReference type)
        {
            var typeDef = type.BetterResolve();
            var result = new List<TypeReference>();

            while (true)
            {
                if (typeDef == null)
                    break;

                try
                {
                    if (typeDef.BaseType == null && (typeDef.Interfaces == null || typeDef.Interfaces.Count == 0))
                        break;

                    if (typeDef.Interfaces != null && typeDef.Interfaces.Count > 0)
                        result.AddRange(
                            type
                                .Recursive(x =>
                                    x.BetterResolve()?
                                        .Interfaces
                                        .Select(y => y.InterfaceType))
                                    .Select(x => x.ResolveType(type)));

                    type = typeDef.BaseType;
                    typeDef = type?.BetterResolve();
                }
                catch (Exception e)
                {
                    Builder.Current.Log(LogTypes.Info, $"Error in getting interfaces of '{type}'\r\n" + e.GetStackTrace());
                    break;
                }
            };

            return result.Where(x => x.Resolve()?.IsInterface ?? true);
        }

        public static MethodReference GetMethodReference(this TypeReference type, string methodName, int parameterCount)
        {
            var definition = type.BetterResolve();
            var result = definition
                .Methods
                .FirstOrDefault(x => x.Name == methodName && x.Parameters.Count == parameterCount);

            if (result != null)
                return result;

            throw new Exception($"Unable to proceed. The type '{type.FullName}' does not contain a method '{methodName}'");
        }

        public static MethodReference GetMethodReference(this TypeReference type, string methodName, Type[] parameterTypes)
        {
            var definition = type.BetterResolve();
            var result = definition
                .Methods
                .FirstOrDefault(x => x.Name == methodName && parameterTypes.Select(y => y.FullName).SequenceEqual(x.Parameters.Select(y => y.ParameterType.FullName)));

            if (result != null)
                return result;

            throw new Exception($"Unable to proceed. The type '{type.FullName}' does not contain a method '{methodName}'");
        }

        public static MethodReference GetMethodReference(this TypeReference type, string methodName, TypeReference[] parameterTypes)
        {
            var definition = type.BetterResolve();
            var result = definition
                .Methods
                .FirstOrDefault(x => x.Name == methodName && parameterTypes.Select(y => y.FullName).SequenceEqual(x.Parameters.Select(y => y.ParameterType.FullName)));

            if (result != null)
                return result;

            throw new Exception($"Unable to proceed. The type '{type.FullName}' does not contain a method '{methodName}'");
        }

        public static IEnumerable<MethodDefinitionAndReference> GetMethodReferences(this TypeReference type) => GetMethodReferences(type, false);

        public static IEnumerable<MethodDefinitionAndReference> GetMethodReferencesByInterfaces(this TypeReference type) => GetMethodReferences(type, true);

        public static BuilderType GetNestedTypeParent(this BuilderType builderType)
        {
            var pos = builderType.typeReference.FullName.IndexOf('/');
            return Builder.Current.GetType(builderType.typeReference.FullName.Substring(0, pos));
        }

        public static IEnumerable<TypeReference> GetNestedTypes(this TypeReference type)
        {
            var typeDef = type.BetterResolve();

            if (typeDef == null)
                return new TypeReference[0];

            if (typeDef.NestedTypes != null && typeDef.NestedTypes.Count > 0)
                return type.Recursive(x => x.BetterResolve().NestedTypes).Select(x => x.ResolveType(type));

            if (typeDef.BaseType != null)
                return GetNestedTypes(typeDef.BaseType);

            return new TypeReference[0];
        }

        public static SequencePoint GetSequencePoint(this MethodDefinition methodDefinition)
        {
            if (methodDefinition == null || methodDefinition.Body == null || methodDefinition.Body.Instructions == null)
                return null;

            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                var result = methodDefinition.DebugInformation.GetSequencePoint(instruction);
                if (result == null)
                    continue;

                return result;
            }

            return null;
        }

        public static string GetStackTrace(this Exception e)
        {
            var sb = new StringBuilder();
            var ex = e;

            do
            {
                sb.AppendLine("Exception Type: " + ex.GetType().Name);
                sb.AppendLine("Source: " + ex.Source);
                sb.AppendLine(ex.Message);
                sb.AppendLine("------------------------");
                sb.AppendLine(ex.StackTrace);
                sb.AppendLine("------------------------");

                ex = ex.InnerException;
            } while (ex != null);

            return sb.ToString();
        }

        public static TypeDefinition GetTypeDefinition(this Type type)
        {
            var result = WeaverBase.AllTypes.Get(type.FullName);

            if (result == null)
                throw new Exception($"Unable to proceed. The type '{type.FullName}' was not found.");

            return Builder.Current.Import(type).Resolve() ?? result;
        }

        public static string GetValidName(this string name) => name?
                .Replace('´', '_')
                .Replace('`', '_')
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace('.', '_')
                .Replace('/', '+')
                .Replace(',', '_');

        public static bool HasAttribute(this IEnumerable<CustomAttribute> collection, TypeDefinition typeDefinition)
        {
            foreach (var item in collection)
            {
                if (item.AttributeType.AreEqual(typeDefinition))
                    return true;
            }

            return false;
        }

        public static bool Implements(this TypeReference type, TypeReference @interface) => type.GetInterfaces().Any(x => x.AreEqual(@interface));

        public static int IndexOf(this IList<Instruction> instructions, int offset)
        {
            for (int i = 0; i < instructions.Count; i++)
                if (instructions[i].Offset == offset)
                    return i;

            return -1;
        }

        /// <summary>
        /// Returns true if the instruction defined by <paramref name="instruction"/> is enclosed by a try-catch-finally.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="instruction">The instruction to check</param>
        /// <returns>True if enclosed; otherwise false.</returns>
        public static bool IsInclosedInHandlers(this Method method, Instruction instruction)
        {
            foreach (var item in method.methodDefinition.Body.ExceptionHandlers)
            {
                if (item.TryStart.Offset <= instruction.Offset && item.TryEnd.Offset >= instruction.Offset)
                    return true;

                if (item.HandlerStart != null && item.HandlerStart.Offset <= instruction.Offset && item.HandlerEnd.Offset >= instruction.Offset)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the instruction defined by <paramref name="instruction"/> is enclosed by a try-catch-finally.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="instruction">The instruction to check</param>
        /// <param name="exceptionHandlerType">The type of handler that encloses the instruction.</param>
        /// <returns>True if enclosed; otherwise false.</returns>
        public static bool IsInclosedInHandlers(this Method method, Instruction instruction, ExceptionHandlerType exceptionHandlerType)
        {
            foreach (var item in method.methodDefinition.Body.ExceptionHandlers)
            {
                if (item.TryStart.Offset >= instruction.Offset && item.TryStart.Offset <= instruction.Offset && item.HandlerType == exceptionHandlerType)
                    return true;

                if (item.HandlerStart != null && item.HandlerStart.Offset >= instruction.Offset && item.HandlerEnd.Offset <= instruction.Offset && item.HandlerType == exceptionHandlerType)
                    return true;
            }

            return false;
        }

        public static bool IsSubclassOf(this BuilderType child, BuilderType parent) => !child.typeDefinition.AreEqual(parent.typeDefinition) && child.BaseClasses.Any(x => x.typeDefinition.AreEqual(parent.typeDefinition));

        public static bool IsSubclassOf(this TypeReference child, TypeReference parent) => !child.AreEqual(parent) && child.GetBaseClasses().Any(x => x.AreEqual(parent));

        public static void Log(this ICecilatorObject cecilatorObject, LogTypes logTypes, BuilderType type, object arg) => cecilatorObject.Log(logTypes, type.GetRelevantConstructors().FirstOrDefault() ?? type.Methods.FirstOrDefault(), arg);

        public static void Log(this ICecilatorObject cecilatorObject, LogTypes logTypes, Property property, object arg) => cecilatorObject.Log(logTypes, property.Getter ?? property.Setter, arg);

        public static void Log(this ICecilatorObject cecilatorObject, LogTypes logTypes, Method method, object arg) => cecilatorObject.Log(logTypes, method.methodDefinition.GetSequencePoint(), arg);

        public static void Log(this ICecilatorObject cecilatorObject, LogTypes logTypes, object arg) => cecilatorObject.Log(logTypes, sequencePoint: null, arg: arg);

        /// <summary>
        /// Creates a new coder.
        /// </summary>
        /// <param name="method">The coder.</param>
        /// <returns></returns>
        public static Coder NewCoder(this Method method) => new Coder(method);

        /// <summary>
        /// Creates a new coder.
        /// </summary>
        /// <param name="coder">The coder.</param>
        /// <returns></returns>
        public static CatchThrowerCoder NewCoder(this CatchThrowerCoder coder) => new CatchThrowerCoder(coder.instructions.associatedMethod);

        /// <summary>
        /// Creates a new coder.
        /// </summary>
        /// <param name="coder">The coder.</param>
        /// <returns></returns>
        public static Coder NewCoder(this CoderBase coder) => new Coder(coder.instructions.associatedMethod);

        public static Field Resolve(this Field field) => new Field(field.DeclaringType, field.fieldDef, field.fieldRef.CreateFieldReference());

        public static Field Resolve(this Field field, BuilderType type)
        {
            var result = field.CreateFieldReference(type);
            return new Field(field.DeclaringType, field.fieldDef, result);
        }

        public static Field Resolve(this Field field, Method method)
        {
            var result = field.CreateFieldReference(method);
            return new Field(field.DeclaringType, field.fieldDef, result);
        }

        public static GenericInstanceType ResolveGenericArguments(this GenericInstanceType self, GenericInstanceType inheritingOrImplementingType)
        {
            if (self.FullName == inheritingOrImplementingType.FullName)
                return self;

            var genericParameters = inheritingOrImplementingType.GetGenericResolvedTypeName();
            var genericArguments = new TypeReference[self.GenericArguments.Count];

            for (int i = 0; i < genericArguments.Length; i++)
                genericArguments[i] = genericParameters.ContainsKey(self.GenericArguments[i].FullName) ? genericParameters[self.GenericArguments[i].FullName] : self.GenericArguments[i];

            return self.BetterResolve().MakeGenericInstanceType(genericArguments);
        }

        public static object ThisOrNull(this Method method) => method.IsStatic ? null : CodeBlocks.This;

        public static T To<T>(this object o) where T : class => o as T ?? throw new InvalidCastException($"Unable to cast '{o?.GetType()}' to '{typeof(T).FullName}'");

        public static BuilderType ToBuilderType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() != type)
            {
                var builder = Builder.Current;
                var definition = type.GetGenericTypeDefinition();
                var typeDefinition = WeaverBase.AllTypes.Get(definition.FullName);
                var typeReference = typeDefinition.MakeGenericInstanceType(type.GetGenericArguments().Select(x => x.ToBuilderType().typeReference).ToArray());

                return new BuilderType(Builder.Current, typeReference);
            }

            try
            {
                var result = Builder.Current.Import(type);

                if (result != null)
                    return result.ToBuilderType();
            }
            catch
            {
            }

            return new BuilderType(Builder.Current, WeaverBase.AllTypes.Get(type.FullName));
        }

        public static BuilderType ToBuilderType(this TypeDefinition value) => new BuilderType(Builder.Current, value);

        public static BuilderType ToBuilderType(this TypeReference value) => new BuilderType(Builder.Current, value);

        public static Type ToType(this TypeReference typeReference) => Type.GetType(typeReference.FullName + ", " + typeReference.Module.Assembly.FullName);

        /// <summary>
        /// Creates a typeof() implementation for the given type <paramref name="type"/>
        /// </summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="type">The type to get the type from.</param>
        /// <returns>A collection of instructions that canbe added to the coder's instruction set.</returns>
        public static IEnumerable<Instruction> TypeOf(this ILProcessor processor, BuilderType type) =>
            processor.TypeOf(type.typeReference);

        /// <summary>
        /// Creates a typeof() implementation for the given type <paramref name="type"/>
        /// </summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="type">The type to get the type from.</param>
        /// <returns>A collection of instructions that canbe added to the coder's instruction set.</returns>
        public static IEnumerable<Instruction> TypeOf(this ILProcessor processor, TypeReference type)
        {
            return new Instruction[] {
                processor.Create(OpCodes.Ldtoken, Builder.Current.Import(type)),
                processor.Create(OpCodes.Call,
                    Builder.Current.Import(
                        typeof(Type).GetTypeDefinition()
                            .Methods.FirstOrDefault(x=>x.Name == "GetTypeFromHandle" && x.Parameters.Count == 1)))
            };
        }

        public static TNew With<TType, TNew>(this TType target, Func<TType, TNew> predicate) => predicate(target);

        internal static void Append(this ILProcessor processor, Instruction[] instructions) => processor.Append(instructions as IEnumerable<Instruction>);

        internal static void Append(this ILProcessor processor, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                processor.Append(instruction);
        }

        internal static FieldReference CreateFieldReference(this Field field, BuilderType type)
        {
            if (type.typeReference.IsGenericInstance && type.typeReference is GenericInstanceType genericInstanceType)
                return new FieldReference(field.Name, field.FieldType.typeReference, genericInstanceType);

            return field.fieldRef.CreateFieldReference();
        }

        internal static FieldReference CreateFieldReference(this Field field, Method method)
        {
            if (method.methodReference.ContainsGenericParameter)
            {
                var declaringType = new GenericInstanceType(field.DeclaringType.typeDefinition);

                if (method.methodReference.DeclaringType.IsGenericInstance && method.methodReference.DeclaringType is GenericInstanceType genericInstanceType)
                {
                    foreach (var parameter in genericInstanceType.GenericArguments)
                        declaringType.GenericArguments.Add(parameter);
                }

                foreach (var parameter in method.methodReference.GenericParameters)
                    declaringType.GenericArguments.Add(parameter);

                return new FieldReference(field.Name, field.FieldType.typeReference, declaringType);
            }

            return field.fieldRef.CreateFieldReference();
        }

        internal static FieldReference CreateFieldReference(this FieldDefinition field)
        {
            if (field.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(field.DeclaringType);

                foreach (var parameter in field.DeclaringType.GenericParameters)
                    declaringType.GenericArguments.Add(parameter);

                return new FieldReference(field.Name, field.FieldType, declaringType);
            }

            return field;
        }

        internal static FieldReference CreateFieldReference(this FieldReference field)
        {
            if (field.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(field.DeclaringType);

                foreach (var parameter in field.DeclaringType.GenericParameters)
                    declaringType.GenericArguments.Add(parameter);

                return new FieldReference(field.Name, field.FieldType, declaringType);
            }

            return field;
        }

        internal static MethodReference CreateMethodReference(this MethodDefinition method, TypeReference originType)
        {
            if (method.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(method.DeclaringType);
                return method.MakeHostInstanceGeneric(originType.GenericParameters.ToArray());
            }

            return method;
        }

        internal static MethodReference CreateMethodReference(this MethodDefinition method)
        {
            if (method.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(method.DeclaringType);
                return method.MakeHostInstanceGeneric(method.DeclaringType.GenericParameters.ToArray());
            }

            return method;
        }

        internal static T Display<T>(this T obj) where T : class
        {
            switch (obj)
            {
                case null: return obj;
                case IEnumerable enumerable:
                    foreach (var item in enumerable)
                    {
                        if (item is BuilderType builderType)
                            builderType.Display();
                        else
                            Builder.Current.Log(LogTypes.Info, $"Display: {item}");
                    }

                    return obj;

                default:
                    Builder.Current.Log(LogTypes.Info, $"Display: {obj}");
                    return obj;
            }
        }

        internal static void Display(this IEnumerable<ExceptionHandler> handlers)
        {
            foreach (var item in handlers)
            {
                Builder.Current.Log(LogTypes.Info, $"IL_{item.TryStart.Offset.ToString("X4")} <-> IL_{item.TryEnd.Offset.ToString("X4")} : {item.HandlerType}");

                if (item.HandlerStart != null)
                    Builder.Current.Log(LogTypes.Info, $"IL_{item.HandlerStart.Offset.ToString("X4")} <-> IL_{item.HandlerEnd.Offset.ToString("X4")}");
            }
        }

        internal static void Display(this Type type)
        {
            Builder.Current.Log(LogTypes.Info, $"### {type?.Module.Assembly.FullName} {type?.FullName}");
            Builder.Current.Log(LogTypes.Info, $"### {type?.Module.Assembly.FullName} {type?.FullName}");
        }

        internal static void Display(this BuilderType type) => type.typeReference.Display();

        internal static void Display(this TypeReference type)
        {
            Builder.Current.Log(LogTypes.Info, $"### {type?.Module.Assembly.FullName} {type?.FullName}");
            Builder.Current.Log(LogTypes.Info, $"### {type?.Resolve()?.Module.Assembly.FullName} {type?.Resolve()?.FullName}");
        }

        internal static void Display(this Method method) => Builder.Current.Log(LogTypes.Info, $"### {method}");

        internal static void Display(this Instruction instruction) =>
                Builder.Current.Log(LogTypes.Info, $"IL_{instruction.Offset.ToString("X4")}: {instruction.OpCode.ToString()} { (instruction.Operand is Instruction ? "IL_" + (instruction.Operand as Instruction).Offset.ToString("X4") : instruction.Operand?.ToString())} ");

        internal static void Display(this MethodBody body)
        {
            Builder.Current.Log(LogTypes.Info, $"### {body.Method.FullName}");
            body.ExceptionHandlers.Display();

            foreach (var item in body.Instructions)
                item.Display();
        }

        internal static Method GetAsyncMethod(this Builder builder, Method method)
        {
            var result = (builder as CecilatorObject).GetAsyncMethod(method.methodDefinition);

            if (result == null)
                return null;

            return new AsyncStateMachineMoveNextMethod(new BuilderType(builder, result.Item2), result.Item1, method);
        }

        /// <summary>
        /// Gets a specified length of bytes
        /// </summary>
        /// <param name="target">The Array that contains the data to copy.</param>
        /// <param name="startingPosition">
        /// A 32-bit integer that represents the index in the sourceArray at which copying begins.
        /// </param>
        /// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Parameter <paramref name="startingPosition"/> and <paramref name="length"/> are out of range
        /// </exception>
        internal static byte[] GetBytes(this byte[] target, int startingPosition, int length)
        {
            if (length + startingPosition > target.Length)
                throw new ArgumentOutOfRangeException("length", "Parameter startingPosition and length are out of range");

            byte[] value = new byte[length];

            Array.Copy(target, startingPosition, value, 0, length);
            return value;
        }

        internal static IEnumerable<Instruction> GetJumpSources(this IEnumerable<Instruction> body, Instruction jumpTarget)
        {
            foreach (var item in body)
            {
                if (item.Operand is Instruction target && target.Offset == jumpTarget.Offset)
                    yield return item;
            }
        }

        internal static ParameterReference GetParameter(this Method method, Instruction instruction)
            => method.methodReference.GetParameter(instruction);

        internal static ParameterReference GetParameter(this MethodReference methodReference, Instruction instruction)
        {
            var isStatic = methodReference.Resolve().IsStatic;

            if (instruction.OpCode == OpCodes.Ldarg_0)
                return isStatic ? methodReference.Parameters[0] : null;
            else if (instruction.OpCode == OpCodes.Ldarg_1)
                return methodReference.Parameters[isStatic ? 1 : 0];
            else if (instruction.OpCode == OpCodes.Ldarg_2)
                return methodReference.Parameters[isStatic ? 2 : 1];
            else if (instruction.OpCode == OpCodes.Ldarg_3)
                return methodReference.Parameters[isStatic ? 3 : 2];
            else
                return methodReference.Parameters[(int)instruction.Operand];
        }

        internal static TypeReference GetTypeOfValueInStack(this IEnumerable<Instruction> instructions, Method method)
        {
            TypeReference GetTypeOfValueInStack(Instruction ins)
            {
                if (ins.OpCode == OpCodes.Ldelem_Ref)
                {
                    var temp = instructions.ToArray();
                    return GetTypeOfValueInStack(temp[Array.IndexOf(temp, ins) - 2]);
                }

                if (ins.IsCallOrNew())
                    return (ins.Operand as MethodReference).With(x =>
                    {
                        return x.ReturnType.AreEqual(BuilderTypes.Void) ?
                            null :
                            x.ReturnType.ResolveType(x.DeclaringType, x);
                    });

                if (ins.IsLoadField())
                    return (ins.Operand as FieldReference).With(x => x.FieldType.ResolveType(x.DeclaringType));

                if (ins.IsLoadLocal())
                    return method.methodDefinition.GetVariable(ins).With(x =>
                    {
                        if (x == null)
                            return null;

                        return x.VariableType.ResolveType(method.DeclaringType.typeReference, method.methodReference);
                    });

                if (ins.IsComparer())
                    return BuilderTypes.Boolean;

                if (ins.OpCode == OpCodes.Isinst)
                    return (ins.Operand as TypeReference).With(x => x.ResolveType(method.DeclaringType.typeReference, method.methodReference));

                return ParametersCodeBlock.GetTargetTypeFromOpCode(method, ins).With(x =>
                {
                    if (x == null)
                        return null;

                    return x.typeReference.ResolveType(method.DeclaringType.typeReference, method.methodReference);
                });
            }

            if (instructions == null || !instructions.Any())
                return null;

            var instruction = instructions.Last();
            var result = GetTypeOfValueInStack(instruction);
            var array = instructions.ToArray();

            if (
                result == null &&
                instruction.OpCode != OpCodes.Unbox_Any &&
                instruction.OpCode != OpCodes.Unbox &&
                instruction.OpCode != OpCodes.Isinst &&
                instruction.OpCode != OpCodes.Castclass &&
                instruction.IsValueOpCode())
            {
                for (int i = array.Length - 2; i >= 0; i--)
                {
                    if (!array[i].IsValueOpCode())
                        break;

                    result = GetTypeOfValueInStack(array[i]);

                    if (result != null)
                        return result;
                }
            }

            return result;
        }

        internal static VariableDefinition GetVariable(this Method method, Instruction instruction)
            => method.methodDefinition.GetVariable(instruction);

        internal static VariableDefinition GetVariable(this MethodDefinition methodDefinition, Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ldloc_0)
                return methodDefinition.Body.Variables[0];
            else if (instruction.OpCode == OpCodes.Ldloc_1)
                return methodDefinition.Body.Variables[1];
            else if (instruction.OpCode == OpCodes.Ldloc_2)
                return methodDefinition.Body.Variables[2];
            else if (instruction.OpCode == OpCodes.Ldloc_3)
                return methodDefinition.Body.Variables[3];
            else
                return instruction.Operand as VariableDefinition;
        }

        internal static void InsertAfter(this ILProcessor processor, Instruction target, Instruction[] instructions) => processor.InsertAfter(target, instructions as IEnumerable<Instruction>);

        internal static void InsertAfter(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions)
        {
            var last = target;

            foreach (var instruction in instructions)
            {
                processor.InsertAfter(last, instruction);
                last = instruction;
            }
        }

        internal static void InsertAfter(this ILProcessor processor, int index, IEnumerable<Instruction> instructions)
        {
            var last = processor.Body.Instructions[index];

            foreach (var instruction in instructions)
            {
                processor.InsertAfter(last, instruction);
                last = instruction;
            }
        }

        internal static void InsertBefore(this ILProcessor processor, Instruction target, Instruction[] instructions) => processor.InsertBefore(target, instructions as IEnumerable<Instruction>);

        internal static void InsertBefore(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
                processor.InsertBefore(target, instruction);
        }

        internal static void InsertBefore(this ILProcessor processor, int index, IEnumerable<Instruction> instructions)
        {
            var target = processor.Body.Instructions[index];

            foreach (var instruction in instructions)
                processor.InsertBefore(target, instruction);
        }

        internal static bool IsCall(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Call ||
                opCode == OpCodes.Calli ||
                opCode == OpCodes.Callvirt;
        }

        internal static bool IsCallOrNew(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Call ||
                opCode == OpCodes.Calli ||
                opCode == OpCodes.Callvirt ||
                opCode == OpCodes.Newobj;
        }

        internal static bool IsComparer(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Ceq ||
                opCode == OpCodes.Cgt ||
                opCode == OpCodes.Cgt_Un ||
                opCode == OpCodes.Clt_Un ||
                opCode == OpCodes.Clt;
        }

        internal static bool IsDelegate(this TypeDefinition typeDefinition)
        {
            if (typeDefinition == null || typeDefinition.BaseType == null)
                return false;

            return typeDefinition.BaseType.FullName == "System.MulticastDelegate";
        }

        internal static bool IsDelegate(this TypeReference typeReference) => typeReference.BetterResolve().IsDelegate();

        internal static bool IsJumpReturnOrThrow(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Throw ||
                opCode == OpCodes.Leave ||
                opCode == OpCodes.Brfalse ||
                opCode == OpCodes.Brfalse_S ||
                opCode == OpCodes.Brtrue ||
                opCode == OpCodes.Brtrue_S ||
                opCode == OpCodes.Br_S ||
                opCode == OpCodes.Ret ||
                opCode == OpCodes.Br ||
                opCode == OpCodes.Break;
        }

        internal static bool IsLoadArgument(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Ldarg ||
                opCode == OpCodes.Ldarga ||
                opCode == OpCodes.Ldarga_S ||
                opCode == OpCodes.Ldarg_0 ||
                opCode == OpCodes.Ldarg_1 ||
                opCode == OpCodes.Ldarg_2 ||
                opCode == OpCodes.Ldarg_3;
        }

        internal static bool IsLoadField(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Ldsfld ||
                opCode == OpCodes.Ldsflda ||
                opCode == OpCodes.Ldfld ||
                opCode == OpCodes.Ldflda;
        }

        internal static bool IsLoadLocal(this Instruction instruction, VariableReference variableReference)
        {
            var opCode = instruction.OpCode;

            if (instruction.OpCode == OpCodes.Ldloc_0 ||
                instruction.OpCode == OpCodes.Ldloc_1 ||
                instruction.OpCode == OpCodes.Ldloc_2 ||
                instruction.OpCode == OpCodes.Ldloc_3 ||
                instruction.OpCode == OpCodes.Ldloc_S ||
                instruction.OpCode == OpCodes.Ldloca_S)
                return (int)instruction.Operand == variableReference.Index;

            return instruction.Operand as VariableReference == variableReference;
        }

        internal static bool IsLoadLocal(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Ldloc ||
                opCode == OpCodes.Ldloc_S ||
                opCode == OpCodes.Ldloca ||
                opCode == OpCodes.Ldloca_S ||
                opCode == OpCodes.Ldloc_0 ||
                opCode == OpCodes.Ldloc_1 ||
                opCode == OpCodes.Ldloc_2 ||
                opCode == OpCodes.Ldloc_3;
        }

        internal static bool IsPrivate(this PropertyDefinition property)
        {
            var getter = property.GetMethod?.Attributes.HasFlag(MethodAttributes.Private) ?? true;
            var setter = property.SetMethod?.Attributes.HasFlag(MethodAttributes.Private) ?? true;

            return getter | setter;
        }

        internal static bool IsStoreLocal(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                opCode == OpCodes.Stloc ||
                opCode == OpCodes.Stloc_S ||
                opCode == OpCodes.Stloc_0 ||
                opCode == OpCodes.Stloc_1 ||
                opCode == OpCodes.Stloc_2 ||
                opCode == OpCodes.Stloc_3;
        }

        internal static bool IsValueOpCode(this Instruction instruction)
        {
            var opCode = instruction.OpCode;
            return
                IsCallOrNew(instruction) ||
                IsLoadArgument(instruction) ||
                IsLoadLocal(instruction) ||
                IsLoadField(instruction) ||
                IsComparer(instruction) ||
                opCode == OpCodes.Isinst ||
                opCode == OpCodes.Dup ||
                opCode == OpCodes.Sizeof ||
                opCode == OpCodes.Ceq ||
                opCode == OpCodes.Xor ||
                opCode == OpCodes.Shl ||
                opCode == OpCodes.Shr ||
                opCode == OpCodes.Shr_Un ||
                opCode == OpCodes.Neg ||
                opCode == OpCodes.Or ||
                opCode == OpCodes.And ||
                opCode == OpCodes.Mul ||
                opCode == OpCodes.Mul_Ovf_Un ||
                opCode == OpCodes.Mul_Ovf ||
                opCode == OpCodes.Add ||
                opCode == OpCodes.Add_Ovf ||
                opCode == OpCodes.Add_Ovf_Un ||
                opCode == OpCodes.Sub ||
                opCode == OpCodes.Sub_Ovf ||
                opCode == OpCodes.Sub_Ovf_Un ||
                opCode == OpCodes.Castclass ||
                opCode == OpCodes.Conv_I ||
                opCode == OpCodes.Conv_I1 ||
                opCode == OpCodes.Conv_I2 ||
                opCode == OpCodes.Conv_I4 ||
                opCode == OpCodes.Conv_I8 ||
                opCode == OpCodes.Conv_Ovf_I4 ||
                opCode == OpCodes.Conv_Ovf_I ||
                opCode == OpCodes.Conv_Ovf_I1 ||
                opCode == OpCodes.Conv_Ovf_I4_Un ||
                opCode == OpCodes.Ldc_I4 ||
                opCode == OpCodes.Ldc_I4_0 ||
                opCode == OpCodes.Ldc_I4_1 ||
                opCode == OpCodes.Ldc_I4_2 ||
                opCode == OpCodes.Ldc_I4_3 ||
                opCode == OpCodes.Ldc_I4_4 ||
                opCode == OpCodes.Ldc_I4_5 ||
                opCode == OpCodes.Ldc_I4_6 ||
                opCode == OpCodes.Ldc_I4_7 ||
                opCode == OpCodes.Ldc_I4_8 ||
                opCode == OpCodes.Ldc_I4_S ||
                opCode == OpCodes.Ldc_I4_M1 ||
                opCode == OpCodes.Ldc_I8 ||
                opCode == OpCodes.Ldc_R4 ||
                opCode == OpCodes.Ldc_R8 ||
                opCode == OpCodes.Ldstr ||
                opCode == OpCodes.Unbox_Any ||
                opCode == OpCodes.Unbox ||
                opCode == OpCodes.Box;
        }

        internal static MethodReference MakeGeneric(this MethodReference method, TypeReference returnType, params TypeReference[] args)
        {
            if (args.Length == 0)
                return method;

            if (method.GenericParameters.Count != args.Length)
                throw new ArgumentException("Invalid number of generic type arguments supplied");

            var genericTypeRef = new GenericInstanceMethod(method);

            foreach (var arg in args)
                genericTypeRef.GenericArguments.Add(arg);

            if (returnType != null)
                genericTypeRef.ReturnType = returnType;

            return genericTypeRef;
        }

        internal static MethodReference MakeHostInstanceGeneric(this MethodReference self, IEnumerable<TypeReference> arguments) =>
            self.MakeHostInstanceGeneric(arguments.ToArray());

        internal static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] arguments)
        {
            // https://groups.google.com/forum/#!topic/mono-cecil/mCat5UuR47I by ShdNx

            var genericDeclaringType = self.DeclaringType.MakeGenericInstanceType(arguments);
            var reference = new MethodReference(self.Name, self.ReturnType, genericDeclaringType)
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType) { Name = parameter.Name });

            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

        internal static IEnumerable<T> Recursive<T>(this T root, Func<T, IEnumerable<T>> children)
        {
            // http://codereview.stackexchange.com/questions/5648/any-way-to-make-this-recursive-function-better-faster
            // Eric Lippert

            var stack = new Stack<T>();
            stack.Push(root);

            while (stack.Count != 0)
            {
                var current = stack.Pop();

                if (current == null)
                    Builder.Current.Log(LogTypes.Info, $"-------------------> '{root}'\r\n");

                var childrenOfCurrent = children(current);

                if (childrenOfCurrent != null)
                    foreach (var child in childrenOfCurrent)
                        stack.Push(child);

                yield return current;
            }
        }

        internal static MethodReference ResolveMethod(this MethodReference method, TypeReference declaringType)
        {
            if (method.ContainsGenericParameter && declaringType is GenericInstanceType)
            {
                var declaringTypeInstance = declaringType as GenericInstanceType;
                var genericParameters = declaringTypeInstance.GetGenericResolvedTypeName();

                var genericArguments = new TypeReference[declaringTypeInstance.GenericArguments.Count];

                for (int i = 0; i < genericArguments.Length; i++)
                    genericArguments[i] = genericParameters.ContainsKey(declaringTypeInstance.GenericArguments[i].FullName) ? genericParameters[declaringTypeInstance.GenericArguments[i].FullName] : declaringTypeInstance.GenericArguments[i];

                //if (method.ReturnType.FullName != "System.Void" && !genericParameters.ContainsKey(method.ReturnType.FullName))
                //    return method; // TODO - go from highest implemented interface / base class generic to lowest and parse thier names ... TItem in KeyedCollection<TKey, TItem> -> T in IList<T>

                return method.MakeHostInstanceGeneric(genericArguments);
            }
            else if (declaringType.HasGenericParameters)
                return method.Resolve().CreateMethodReference();
            else
                return method;
        }

        internal static TypeReference ResolveType(this TypeReference type, MethodReference ownerMethod = null)
        {
            try
            {
                TypeReference resolveType(IReadOnlyDictionary<string, TypeReference> genericParameters, TypeReference ptype)
                {
                    var genericType = !genericParameters.ContainsKey(ptype.FullName) ? ptype.ResolveType(ownerMethod) : genericParameters[ptype.FullName];

                    while (genericType.IsGenericParameter)
                    {
                        if (!genericParameters.TryGetValue(genericType.FullName, out genericType))
                            break;

                        if (!genericType.IsGenericParameter)
                            return genericType;
                    }

                    return genericParameters[ptype.FullName];
                }

                if (ownerMethod is GenericInstanceMethod genericInstanceMethod)
                {
                    if (type.IsGenericInstance)
                    {
                        var genericParameters = genericInstanceMethod.GetGenericResolvedTypeNames();
                        var genericTypeInstance = type as GenericInstanceType;

                        var result = new GenericInstanceType(genericTypeInstance.BetterResolve());

                        foreach (var item in genericTypeInstance.GenericArguments)
                            result.GenericArguments.Add(resolveType(genericParameters, item));

                        return result;
                    }

                    if (type.ContainsGenericParameter)
                    {
                        Builder.Current.Log(LogTypes.Info, $"--->  '{ownerMethod}' resolving '{type.FullName}'");
                        var genericParameters = genericInstanceMethod.GetGenericResolvedTypeNames();

                        if (type.GenericParameters.Count == 0 && type.IsGenericParameter)
                            return genericParameters[type.FullName];
                        else
                        {
                            var result = new GenericInstanceType(type);

                            foreach (var item in type.GenericParameters)
                                result.GenericArguments.Add(resolveType(genericParameters, item));

                            return result;
                        }
                    }

                    {
                        var genericParameters = genericInstanceMethod.GetGenericResolvedTypeNames();
                        return resolveType(genericParameters, type);
                    }
                }

                return type;
            }
            catch (Exception e)
            {
                throw new TypeResolveException($"Unable to resolve type '{type?.FullName ?? "Unknown"}'", e);
            }
        }

        internal static TypeReference ResolveType(this TypeReference type, TypeReference inheritingOrImplementingType, MethodReference ownerMethod = null)
        {
            if (inheritingOrImplementingType is GenericInstanceType castedInheritingOrImplementingType)
            {
                if (type.IsGenericParameter)
                {
                    var genericParameters = castedInheritingOrImplementingType.GetGenericResolvedTypeName();

                    if (genericParameters.ContainsKey(type.FullName))
                        return genericParameters[type.FullName];

                    genericParameters = castedInheritingOrImplementingType.GetGenericResolvedTypeNames();
                    var genericType = genericParameters[type.FullName];

                    while (genericType.IsGenericParameter)
                    {
                        genericType = genericParameters[genericType.FullName];

                        if (!genericType.IsGenericParameter)
                            return genericType;
                    }

                    return genericParameters[type.FullName];
                }

                if (type.HasGenericParameters)
                    return (type as GenericInstanceType ?? type.MakeGenericInstanceType(type.GenericParameters.ToArray())).ResolveGenericArguments(castedInheritingOrImplementingType);

                if (type.ContainsGenericParameter)
                {
                    if (!type.IsGenericInstance)
                        return type;

                    return (type as GenericInstanceType ?? type.MakeGenericInstanceType(type.GenericParameters.ToArray())).ResolveGenericArguments(castedInheritingOrImplementingType);
                }
            }

            if (type.HasGenericParameters)
                return type.MakeGenericInstanceType(type.GenericParameters.ToArray());

            if (ownerMethod != null)
                return type.ResolveType(ownerMethod);

            return type;
        }

        internal static MethodAttributes ToMethodAttributes(this Modifiers modifier)
        {
            var attributes = MethodAttributes.CompilerControlled;

            if (modifier.HasFlag(Modifiers.Private)) attributes |= MethodAttributes.Private;
            if (modifier.HasFlag(Modifiers.Static)) attributes |= MethodAttributes.Static;
            if (modifier.HasFlag(Modifiers.Public)) attributes |= MethodAttributes.Public;
            if (modifier.HasFlag(Modifiers.Protected)) attributes |= MethodAttributes.Family;
            if (modifier.HasFlag(Modifiers.Overrides)) attributes |= MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.NewSlot;
            if (modifier.HasFlag(Modifiers.Explicit)) attributes |= MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Private;

            return attributes;
        }

        private static bool AreEqual(this string assemblyA, string assemblyB)
        {
            if (assemblyA == assemblyB)
                return true;

            if (assemblyA == null || assemblyB == null)
                return false;

            if ((assemblyA == "System.Runtime" || assemblyA == "mscorlib") && (assemblyB == "System.Runtime" || assemblyB == "mscorlib"))
                return true;

            if ((assemblyA == "System.Runtime" || assemblyA == "netstandard") && (assemblyB == "System.Runtime" || assemblyB == "netstandard"))
                return true;

            return false;
        }

        private static Tuple<MethodDefinition, TypeReference> GetAsyncMethod(this CecilatorObject cecilatorObject, MethodDefinition method)
        {
            var asyncStateMachine = method.CustomAttributes.Get("System.Runtime.CompilerServices.AsyncStateMachineAttribute");

            if (asyncStateMachine != null)
            {
                var asyncType = asyncStateMachine.ConstructorArguments[0].Value as TypeReference;
                var asyncTypeMethod = asyncType.Resolve().Methods.Get("MoveNext");

                if (asyncTypeMethod == null)
                {
                    cecilatorObject.Log(LogTypes.Error, method, "Unable to find the method MoveNext of async method " + method.Name);
                    throw new Exception("Unable to find the method MoveNext of async method " + method.Name);
                }

                return new Tuple<MethodDefinition, TypeReference>(asyncTypeMethod, asyncType);
            }

            return null;
        }

        private static IEnumerable<TypeReference> GetGenericInstances(this GenericInstanceType type)
        {
            var result = new List<TypeReference>
            {
                type
            };

            var resolved = type.BetterResolve();
            var genericArgumentsNames = resolved.GenericParameters.Select(x => x.FullName).ToArray();
            var genericArguments = type.GenericArguments.ToArray();

            if (resolved.BaseType != null)
                result.AddRange(resolved.BaseType.GetGenericInstances(genericArgumentsNames, genericArguments));

            if (resolved.Interfaces != null && resolved.Interfaces.Count > 0)
            {
                foreach (var item in resolved.Interfaces)
                    result.AddRange(item.InterfaceType.GetGenericInstances(genericArgumentsNames, genericArguments));
            }

            return result;
        }

        private static IEnumerable<TypeReference> GetGenericInstances(this TypeReference type, string[] genericArgumentNames, TypeReference[] genericArgumentsOfCurrentType)
        {
            var resolvedBase = type.BetterResolve();

            if (resolvedBase.HasGenericParameters)
            {
                var genericArguments = new List<TypeReference>();

                foreach (var arg in (type as GenericInstanceType).GenericArguments)
                {
                    var t = genericArgumentNames.FirstOrDefault(x => x == arg.FullName);

                    if (t == null)
                        genericArguments.Add(arg);
                    else
                        genericArguments.Add(genericArgumentsOfCurrentType[Array.IndexOf(genericArgumentNames, t)]);
                }

                var genericType = resolvedBase.MakeGenericInstanceType(genericArguments.ToArray());
                return genericType.GetGenericInstances();
            }

            return new TypeReference[0];
        }

        private static IEnumerable<MethodDefinitionAndReference> GetMethodReferences(this TypeReference type, bool byInterfaces)
        {
            var result = new List<MethodDefinitionAndReference>();
            GetMethodReferences(type, result, byInterfaces);

            return result.Where(x =>
            {
                if (!x.reference.DeclaringType.IsGenericInstance)
                    return true;

                //var genericArgument = (x.reference.DeclaringType as GenericInstanceType).GenericArguments;
                //for (int i = 0; i < genericArgument.Count; i++)
                //{
                //    if (genericArgument[i].IsGenericParameter)
                //        return false;
                //}

                return true;
            });
        }

        private static void GetMethodReferences(TypeReference typeToInspect, List<MethodDefinitionAndReference> result, bool byInterfaces)
        {
            if (typeToInspect == null)
                return;

            if (typeToInspect.IsGenericInstance)
            {
                if (typeToInspect is GenericInstanceType genericType)
                {
                    var instances = genericType.GetGenericInstances().Where(x => (!x.BetterResolve().IsInterface || byInterfaces));
                    foreach (var item in instances)
                    {
                        var methods = item
                            .BetterResolve()
                            .Methods
                            .Select(x => new MethodDefinitionAndReference
                            {
                                reference = x.MakeHostInstanceGeneric((item as GenericInstanceType).GenericArguments.ToArray()),
                                definition = x
                            });

                        if (methods.Any())
                            result.AddRange(methods);
                    }
                }
            }
            else if (typeToInspect.IsGenericParameter)
            {
                // Do nothing
            }
            else
            {
                var methods = typeToInspect
                    .BetterResolve()
                    .Methods
                    .Select(x => new MethodDefinitionAndReference
                    {
                        reference = x.CreateMethodReference(),
                        definition = x
                    });
                if (methods.Any())
                    result.AddRange(methods);
            }

            var typeReference = typeToInspect.BetterResolve();

            if (typeToInspect.IsGenericParameter || typeReference == null)
                return;

            if (byInterfaces)
            {
                for (int i = 0; i < typeReference.Interfaces.Count; i++)
                    GetMethodReferences(typeReference.Interfaces[i].InterfaceType, result, true);
            }
            else if (typeReference.BaseType != null)
                GetMethodReferences(typeReference.BaseType, result, false);
        }
    }
}