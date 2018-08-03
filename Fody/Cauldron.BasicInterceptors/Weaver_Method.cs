﻿using Cauldron.Interception.Cecilator;
using Cauldron.Interception.Cecilator.Coders;
using Cauldron.Interception.Fody;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Weaver_Method
{
    public static string Name = "Method Interceptors";
    public static int Priority = 0;
    private static IEnumerable<BuilderType> methodInterceptionAttributes;

    static Weaver_Method()
    {
        methodInterceptionAttributes =
            Builder.Current.FindAttributesByInterfaces("Cauldron.Interception.IMethodInterceptor")
            .Concat(Builder.Current.FindAttributesByInterfaces("Cauldron.Interception.ISimpleMethodInterceptor"));
    }

    [Display("Type-Wide Method Interception")]
    public static void ImplementTypeWideMethodInterception(Builder builder) => ImplementTypeWideMethodInterception(builder, methodInterceptionAttributes);

    [Display("Method Interception")]
    public static void InterceptMethods(Builder builder)
    {
        if (!methodInterceptionAttributes.Any())
            return;

        var methods = builder
            .FindMethodsByAttributes(methodInterceptionAttributes)
            .Where(x => !x.Method.IsPropertyGetterSetter)
            .GroupBy(x => new MethodKey(x.Method, x.AsyncMethod))
            .Select(x => new MethodBuilderInfo<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>>(x.Key,
                x.Select(y => new MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>(y, BuilderTypes.IMethodInterceptor, BuilderTypes.ISimpleMethodInterceptor))))
            .OrderBy(x => x.Key.Method.DeclaringType.Fullname)
            .ToArray();

        foreach (var method in methods)
        {
            if (method.Items == null || method.Items.Length == 0 || method.Key.Method.IsAbstract)
                continue;

            builder.Log(LogTypes.Info, $"Implementing method interceptors: {method.Key.Method.DeclaringType.Name.PadRight(40, ' ')} {method.Key.Method.Name}({string.Join(", ", method.Key.Method.Parameters.Select(x => x.Name))})");

            var targetedMethod = method.Key.AsyncMethod ?? method.Key.Method;
            var attributedMethod = method.Key.Method;
            var hasSimpleMethodInterceptors = method.Items.Any(x => x.HasInterfaceB);
            var hasFullMethodInterceptors = method.Items.Any(x => x.HasInterfaceA);

            if (method.RequiresSyncRootField)
            {
                if (method.SyncRoot.IsStatic)
                    method.Key.Method.DeclaringType.CreateStaticConstructor().NewCoder()
                        .SetValue(method.SyncRoot, x => x.NewObj(builder.GetType(typeof(object)).Import().ParameterlessContructor))
                        .Insert(InsertionPosition.Beginning);
                else
                    foreach (var ctors in method.Key.Method.DeclaringType.GetRelevantConstructors().Where(x => x.Name == ".ctor"))
                        ctors.NewCoder().SetValue(method.SyncRoot, x => x.NewObj(builder.GetType(typeof(object)).Import().ParameterlessContructor))
                            .Insert(InsertionPosition.Beginning);
            }

            var coder = targetedMethod
                .NewCoder()
                .Context(x =>
                {
                    for (int i = 0; i < method.Items.Length; i++)
                    {
                        var item = method.Items[i];
                        var alwaysCreateNewInstance = item.InterceptorInfo.AlwaysCreateNewInstance;
                        var methodCoder = method.Key.Method.IsAsync ? method.Key.Method.NewCoder() : x;

                        Coder codeInterceptorInstance(Coder interceptorInstanceCoder)
                        {
                            interceptorInstanceCoder.SetValue(item.Interceptor, z => z.NewObj(item.Attribute));
                            if (item.HasSyncRootInterface)
                                interceptorInstanceCoder.Load<ICasting>(item.Interceptor).As(BuilderTypes.ISyncRoot).To<ICallMethod<CallCoder>>().Call(BuilderTypes.ISyncRoot.GetMethod_set_SyncRoot(), method.SyncRoot);

                            ModuleWeaver.ImplementAssignMethodAttribute(builder, method.Items[i].AssignMethodAttributeInfos, item.FieldOrVariable, item.Attribute.Attribute.Type, interceptorInstanceCoder);
                            return interceptorInstanceCoder;
                        }

                        if (alwaysCreateNewInstance)
                            codeInterceptorInstance(methodCoder);
                        else
                            methodCoder.If(y => y.Load<IRelationalOperators>(item.Interceptor).IsNull(), y => codeInterceptorInstance(y));

                        if (method.Key.Method.IsAsync) methodCoder.Insert(InsertionPosition.Beginning);
                        item.Attribute.Remove();
                    }

                    return x;
                });

            if (hasSimpleMethodInterceptors)
            {
                var simpleMethodInterceptors = method.Items.Where(x => x.HasInterfaceB).ToArray();
                coder.Context(x =>
                {
                    if (method.Key.AsyncMethod != null)
                        x.If(num => num.Load(method.Key.Method.AsyncMethodHelper.StateVariable).Is(-1), then =>
                            ImplementOnEnterMethod(coder, x, then, method, attributedMethod, simpleMethodInterceptors, @interface => @interface.InterfaceB.GetMethod_OnEnter()));
                    else

                        ImplementOnEnterMethod(coder, x, method, attributedMethod, simpleMethodInterceptors, @interface => @interface.InterfaceB.GetMethod_OnEnter());

                    if (!hasFullMethodInterceptors)
                        return x.OriginalBody();

                    return x;
                });
            }

            var fullMethodInterceptors = method.Items.Where(x => x.HasInterfaceA).ToArray();
            if (hasFullMethodInterceptors)
            {
                if (method.Key.AsyncMethod != null)
                    coder.Context(x => x.If(num => num.Load(method.Key.Method.AsyncMethodHelper.StateVariable).Is(-1), then =>
                         ImplementOnEnterMethod(coder, x, then, method, attributedMethod, fullMethodInterceptors, @interface => @interface.InterfaceA.GetMethod_OnEnter()))
                        .OriginalBody());
                else
                {
                    coder.Try(x =>
                    {
                        ImplementOnEnterMethod(coder, x, method, attributedMethod, fullMethodInterceptors, @interface => @interface.InterfaceA.GetMethod_OnEnter());
                        return x.OriginalBody();
                    })
                    .Catch(BuilderTypes.Exception.BuilderType, (eCoder, e) => eCoder.If(x =>
                    {
                        var or = x.Load<ICallMethod<BooleanExpressionCallCoder>>(fullMethodInterceptors[0].FieldOrVariable).Call(fullMethodInterceptors[0].InterfaceA.GetMethod_OnException(), e());
                        for (int i = 1; i < fullMethodInterceptors.Length; i++)
                            or.Or(y => y.Load<ICallMethod<CallCoder>>(fullMethodInterceptors[i].FieldOrVariable).Call(fullMethodInterceptors[i].InterfaceA.GetMethod_OnException(), e()));

                        return or.Is(true);
                    }, then => eCoder.NewCoder().Rethrow()).DefaultValue().Return())
                    .Finally(x => ImplementOnExitMethod(x, method, fullMethodInterceptors))
                    .EndTry();
                }
            }

            coder.Return().Replace();

            if (method.Key.AsyncMethod != null && hasFullMethodInterceptors)
            {
                // Special case for async methods
                var exceptionBlock = method.Key.Method.AsyncMethodHelper.GetAsyncStateMachineExceptionBlock();
                targetedMethod
                    .NewCoder().Context(context =>
                    {
                        var exceptionVariable = method.Key.Method.AsyncMethodHelper.GetAsyncStateMachineExceptionVariable();

                        return context.If(x =>
                         {
                             var or = x.Load(fullMethodInterceptors[0].FieldOrVariable as Field).Call(fullMethodInterceptors[0].InterfaceA.GetMethod_OnException(), exceptionVariable);

                             for (int i = 1; i < fullMethodInterceptors.Length; i++)
                                 or.Or(y => y.Load(fullMethodInterceptors[i].FieldOrVariable as Field).Call(fullMethodInterceptors[i].InterfaceA.GetMethod_OnException(), exceptionVariable));

                             return or.Is(false);
                         }, x => x.Jump(exceptionBlock.Item1.End));
                    })
                    .Insert(InsertionAction.After, exceptionBlock.Item1.Beginning);

                targetedMethod
                    .NewCoder().Context(x => ImplementOnExitMethod(x, method, fullMethodInterceptors))
                        .Insert(InsertionAction.Before, exceptionBlock.Item1.End.Previous);

                targetedMethod
                    .NewCoder().Context(context => ImplementOnExitMethod(context, method, fullMethodInterceptors))
                    .Insert(InsertionAction.After, exceptionBlock.Item1.End);
            }
        };
    }

    internal static void ImplementTypeWideMethodInterception(Builder builder, IEnumerable<BuilderType> attributes)
    {
        if (!methodInterceptionAttributes.Any())
            return;

        var types = builder
            .FindTypesByAttributes(methodInterceptionAttributes)
            .GroupBy(x => x.Type)
            .Select(x => new
            {
                x.Key,
                Item = x.ToArray()
            })
            .ToArray();

        foreach (var type in types)
        {
            builder.Log(LogTypes.Info, $"Implementing interceptors in type {type.Key.Fullname}");

            foreach (var method in type.Key.Methods)
            {
                if (method.IsConstructor || method.IsPropertyGetterSetter)
                    continue;

                for (int i = 0; i < type.Item.Length; i++)
                    method.CustomAttributes.Copy(type.Item[i].Attribute);
            }

            for (int i = 0; i < type.Item.Length; i++)
                type.Item[i].Remove();
        }
    }

    private static Coder ImplementOnEnterMethod(
       Coder coder,
       MethodBuilderInfo<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>> method,
       Method attributedMethod,
       MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>[] fullMethodInterceptors,
       Func<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>, Method> onEnterMethod)
       => ImplementOnEnterMethod(coder, coder, coder, method, attributedMethod, fullMethodInterceptors, onEnterMethod);

    private static Coder ImplementOnEnterMethod(
       Coder originCoder,
       Coder coder,
       MethodBuilderInfo<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>> method,
       Method attributedMethod,
       MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>[] fullMethodInterceptors,
       Func<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>, Method> onEnterMethod)
       => ImplementOnEnterMethod(originCoder, coder, coder, method, attributedMethod, fullMethodInterceptors, onEnterMethod);

    private static Coder ImplementOnEnterMethod(
        Coder originCoder,
        Coder outerCoder,
        Coder innerCoder,
        MethodBuilderInfo<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>> method,
        Method attributedMethod,
        MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>[] fullMethodInterceptors,
        Func<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>, Method> onEnterMethod)
    {
        for (int i = 0; i < fullMethodInterceptors.Length; i++)
            innerCoder.Load<ICallMethod<CallCoder>>(fullMethodInterceptors[i].FieldOrVariable).Call(onEnterMethod(fullMethodInterceptors[i]), attributedMethod.OriginType, originCoder.AssociatedMethod.AsyncMethodHelper.Instance, attributedMethod,
                method.Key.Method.Parameters.Length > 0 ? outerCoder.GetParametersArray() : null);

        return innerCoder;
    }

    private static Coder ImplementOnExitMethod(
        Coder coder,
        MethodBuilderInfo<MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>> method,
        MethodBuilderInfoItem<BuilderTypeIMethodInterceptor, BuilderTypeISimpleMethodInterceptor>[] fullMethodInterceptors)
    {
        for (int i = 0; i < fullMethodInterceptors.Length; i++)
        {
            if (fullMethodInterceptors[i].HasOnExitInterface)
            {
                if (method.Key.Method.ReturnType == BuilderTypes.Void)
                {
                    if (fullMethodInterceptors[i].FieldOrVariable is Field field)
                        coder.Load(field)
                            .As(BuilderTypes.IMethodInterceptorOnExit)
                            .Call(BuilderTypes.IMethodInterceptorOnExit.GetMethod_OnExit(), BuilderTypes.Void, null)
                            .Pop();
                    else if (fullMethodInterceptors[i].FieldOrVariable is LocalVariable localVariable)
                        coder.Load(localVariable)
                            .As(BuilderTypes.IMethodInterceptorOnExit)
                            .Call(BuilderTypes.IMethodInterceptorOnExit.GetMethod_OnExit(), BuilderTypes.Void, null)
                            .Pop();
                }
                else
                    coder.SetValue(coder.GetOrCreateReturnVariable(), y =>
                    {
                        if (fullMethodInterceptors[i].FieldOrVariable is Field field)
                            y.Load(field)
                                .As(BuilderTypes.IMethodInterceptorOnExit)
                                .Call(BuilderTypes.IMethodInterceptorOnExit.GetMethod_OnExit(), method.Key.Method.ReturnType, y.GetOrCreateReturnVariable());
                        else if (fullMethodInterceptors[i].FieldOrVariable is LocalVariable localVariable)
                            y.Load(localVariable)
                                .As(BuilderTypes.IMethodInterceptorOnExit)
                                .Call(BuilderTypes.IMethodInterceptorOnExit.GetMethod_OnExit(), method.Key.Method.ReturnType, y.GetOrCreateReturnVariable());

                        return y;
                    });
            }
            else
                coder.Load<ICallMethod<CallCoder>>(fullMethodInterceptors[i].FieldOrVariable).Call(fullMethodInterceptors[i].InterfaceA.GetMethod_OnExit());
        }

        return coder;
    }
}