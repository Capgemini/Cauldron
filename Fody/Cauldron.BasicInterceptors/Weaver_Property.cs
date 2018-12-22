using Cauldron.Interception.Cecilator;
using Cauldron.Interception.Cecilator.Coders;
using Cauldron.Interception.Fody;
using Cauldron.Interception.Fody.HelperTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public static class Weaver_Property
{
    public static string Name = "Property Interceptors";

    public static int Priority = 1;

    static Weaver_Property()
    {
        PropertyInterceptingAttributes = Builder.Current.FindAttributesByInterfaces(
            "Cauldron.Interception.IPropertyInterceptor",
            "Cauldron.Interception.IPropertyGetterInterceptor",
            "Cauldron.Interception.IPropertySetterInterceptor");
    }

    internal static IEnumerable<BuilderType> PropertyInterceptingAttributes { get; private set; }

    [Display("Type-Wide Property Interception")]
    public static void ImplementTypeWidePropertyInterception(Builder builder)
    {
        if (!PropertyInterceptingAttributes.Any())
            return;

        var types = builder
            .FindTypesByAttributes(PropertyInterceptingAttributes)
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

            foreach (var property in type.Key.Properties)
            {
                for (int i = 0; i < type.Item.Length; i++)
                    property.CustomAttributes.Copy(type.Item[i].Attribute);
            }

            for (int i = 0; i < type.Item.Length; i++)
                type.Item[i].Remove();
        }
    }

    [Display("Property Interception")]
    public static void InterceptProperties(Builder builder)
    {
        if (!PropertyInterceptingAttributes.Any())
            return;

        var propertyInterceptionInfo = BuilderTypes.PropertyInterceptionInfo;

        var properties = builder
            .FindPropertiesByAttributes(PropertyInterceptingAttributes)
            .GroupBy(x => x.Property)
            .Select(x => new PropertyBuilderInfo(x.Key, x.Select(y => new PropertyBuilderInfoItem(y, y.Property,
                     y.Attribute.Type.Implements(BuilderTypes.IPropertyGetterInterceptor) ? BuilderTypes.IPropertyGetterInterceptor : null,
                     y.Attribute.Type.Implements(BuilderTypes.IPropertySetterInterceptor) ? BuilderTypes.IPropertySetterInterceptor : null,
                     y.Attribute.Type.Implements(BuilderTypes.IPropertyInterceptorInitialize) ? BuilderTypes.IPropertyInterceptorInitialize : null))))
            .ToArray();

        foreach (var member in properties)
        {
            if (member.Property.IsAbstract)
                continue;

            builder.Log(LogTypes.Info, $"Implementing property interceptors: {member.Property.DeclaringType.Name.PadRight(40, ' ')} {member.Property.Name} {member.Property.ReturnType.Name}");

            if (!member.HasGetterInterception && !member.HasSetterInterception && !member.HasInitializer)
                continue;

            var propertyField = member.Property.CreateField(BuilderTypes.PropertyInterceptionInfo.BuilderType, $"<{member.Property.Name}>p__propertyInfo");
            propertyField.CustomAttributes.AddNonSerializedAttribute();

            var actionObjectCtor = builder.Import(typeof(Action<object>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
            var propertySetter = member.Property.Setter == null ?
                null :
                member.Property.OriginType.CreateMethod(member.Property.Modifiers.GetPrivate(), $"<{member.Property.Name}>m__setterMethod", builder.GetType(typeof(object)));

            if (propertySetter != null)
                CreatePropertySetterDelegate(builder, member, propertySetter);

            var indexer = 0;
            var interceptorFields = member.InterceptorInfos.ToDictionary(x => x.Attribute.Identification,
                x =>
                {
                    if (x.InterfaceInitializer == null && x.InterceptorInfo.AlwaysCreateNewInstance)
                        return null;
                    else
                    {
                        var field = member.Property.OriginType.CreateField(x.Property.Modifiers.GetPrivate(), x.Attribute.Attribute.Type,
                               $"<{x.Property.Name}>_attrib{indexer++}_{x.Attribute.Identification}");

                        field.CustomAttributes.AddNonSerializedAttribute();
                        return field;
                    }
                });

            if (member.HasInitializer)
                AddPropertyInitializeInterception(builder, BuilderTypes.PropertyInterceptionInfo, member, propertyField, actionObjectCtor, propertySetter, interceptorFields);

            if (member.HasGetterInterception && member.Property.Getter != null)
                AddPropertyGetterInterception(builder, propertyInterceptionInfo, member, propertyField, actionObjectCtor, propertySetter, interceptorFields);

            if (member.HasSetterInterception && member.Property.Setter != null)
                AddPropertySetterInterception(builder, propertyInterceptionInfo, member, propertyField, actionObjectCtor, propertySetter, interceptorFields);

            // Do this at the end to ensure that syncroot init is always on the top
            if (member.RequiresSyncRootField)
            {
                if (member.SyncRoot.IsStatic)
                    member.Property.OriginType.CreateStaticConstructor().NewCoder()
                        .SetValue(member.SyncRoot, x => x.NewObj(builder.GetType(typeof(object)).Import().ParameterlessContructor))
                        .Insert(InsertionPosition.Beginning);
                else
                    foreach (var ctors in member.Property.OriginType.GetRelevantConstructors().Where(x => x.Name == ".ctor"))
                        ctors.NewCoder().SetValue(member.SyncRoot, x => x.NewObj(builder.GetType(typeof(object)).Import().ParameterlessContructor)).Insert(InsertionPosition.Beginning);
            }

            // Also remove the compilergenerated attribute
            member.Property.Getter?.CustomAttributes.Remove(typeof(CompilerGeneratedAttribute));
            member.Property.Setter?.CustomAttributes.Remove(typeof(CompilerGeneratedAttribute));
        }
    }

    private static void AddPropertyGetterInterception(
        Builder builder,
        BuilderTypePropertyInterceptionInfo propertyInterceptionInfo,
        PropertyBuilderInfo member,
        Field propertyField,
        Method actionObjectCtor,
        Method propertySetter,
        Dictionary<string, Field> interceptorFields)
    {
        var syncRoot = BuilderTypes.ISyncRoot;
        var legalGetterInterceptors = member.InterceptorInfos.Where(x => x.InterfaceGetter != null).ToArray();

        member.Property.Getter
            .NewCoder()
                .Context(context =>
                {
                    if (member.HasInitializer)
                        return context;

                    for (int i = 0; i < legalGetterInterceptors.Length; i++)
                    {
                        var item = legalGetterInterceptors[i];
                        var alwaysCreateNewInstance = item.InterceptorInfo.AlwaysCreateNewInstance && item.InterfaceInitializer == null;
                        var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ??
                            context.AssociatedMethod.GetOrCreateVariable(item.Attribute.Attribute.Type, item.Attribute.Identification);

                        Coder implementInterceptorInitialization(Coder coder)
                        {
                            coder.SetValue(fieldOrVariable, x => x.NewObj(item.Attribute));

                            if (item.HasSyncRootInterface)
                                coder.Load<ICasting>(fieldOrVariable).As(syncRoot.BuilderType.Import()).To<ICallMethod<CallCoder>>().Call(syncRoot.GetMethod_set_SyncRoot(), member.SyncRoot);

                            ModuleWeaver.ImplementAssignMethodAttribute(builder, legalGetterInterceptors[i].AssignMethodAttributeInfos, fieldOrVariable, item.Attribute.Attribute.Type, coder);
                            return coder;
                        }

                        if (alwaysCreateNewInstance)
                            implementInterceptorInitialization(context);
                        else
                            context.If(x => x.Load<IRelationalOperators>(fieldOrVariable).IsNull(), then => implementInterceptorInitialization(then));

                        item.Attribute.Remove();
                    }

                    return context.If(x => x.Load(propertyField).IsNull(), then =>
                          then.SetValue(propertyField, x =>
                              x.NewObj(propertyInterceptionInfo.GetMethod_ctor(),
                                  member.Property.Getter,
                                  member.Property.Setter,
                                  member.Property.Name,
                                  member.Property.ReturnType,
                                  CodeBlocks.This,
                                  member.Property.ReturnType.IsArray || member.Property.ReturnType.Implements(typeof(IEnumerable)) ? member.Property.ReturnType.ChildType : null,
                                  propertySetter == null ? null : then.NewCoder().NewObj(actionObjectCtor, propertySetter.ThisOrNull(), propertySetter))));
                })
                .Try(@try =>
                {
                    if (member.Property.BackingField == null)
                    {
                        var returnValue = @try.GetOrCreateReturnVariable();
                        @try.SetValue(returnValue, x => x.OriginalBody(true));

                        for (int i = 0; i < legalGetterInterceptors.Length; i++)
                        {
                            var item = legalGetterInterceptors[i];
                            var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ?? @try.AssociatedMethod.GetVariable(item.Attribute.Identification);
                            @try.Load<ICasting>(fieldOrVariable).As(item.InterfaceGetter)
                                    .To<ICallMethod<CallCoder>>()
                                    .Call(item.InterfaceGetter.GetMethod_OnGet(), propertyField, returnValue);
                        }

                        @try.Load(returnValue).Return();
                    }
                    else
                    {
                        for (int i = 0; i < legalGetterInterceptors.Length; i++)
                        {
                            var item = legalGetterInterceptors[i];
                            var alwaysCreateNewInstance = item.InterceptorInfo.AlwaysCreateNewInstance && item.InterfaceInitializer == null;
                            var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ?? @try.AssociatedMethod.GetVariable(item.Attribute.Identification);
                            @try.Load<ICasting>(fieldOrVariable).As(item.InterfaceGetter)
                                    .To<ICallMethod<CallCoder>>()
                                    .Call(item.InterfaceGetter.GetMethod_OnGet(), propertyField, member.Property.BackingField);
                        }

                        @try.OriginalBody();
                    }

                    return @try;
                })
                .Catch(typeof(Exception), (ex, e) =>
                {
                    return ex.If(x => x.Or(legalGetterInterceptors, (coder, y, i) =>
                    {
                        var fieldOrVariable = interceptorFields[y.Attribute.Identification] as CecilatorBase ?? ex.AssociatedMethod.GetVariable(y.Attribute.Identification);
                        return coder.Load<ICasting>(fieldOrVariable)
                             .As(y.InterfaceGetter)
                             .To<ICallMethod<BooleanExpressionCallCoder>>()
                             .Call(y.InterfaceGetter.GetMethod_OnException(), e());
                    }).Is(true), then => ex.NewCoder().Rethrow())
                      .DefaultValue()
                      .Return();
                })
                .Finally(x =>
                {
                    for (int i = 0; i < legalGetterInterceptors.Length; i++)
                    {
                        var item = legalGetterInterceptors[i];
                        var alwaysCreateNewInstance = item.InterceptorInfo.AlwaysCreateNewInstance && item.InterfaceInitializer == null;
                        var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ?? x.AssociatedMethod.GetVariable(item.Attribute.Identification);
                        x.Load<ICasting>(fieldOrVariable)
                            .As(legalGetterInterceptors[i].InterfaceGetter)
                            .To<ICallMethod<CallCoder>>()
                            .Call(legalGetterInterceptors[i].InterfaceGetter.GetMethod_OnExit());
                    }

                    return x;
                })
                .EndTry()
                .Return()
            .Replace();
    }

    private static void AddPropertyInitializeInterception(
        Builder builder,
        BuilderTypePropertyInterceptionInfo propertyInterceptionInfo,
        PropertyBuilderInfo member,
        Field propertyField,
        Method actionObjectCtor,
        Method propertySetter,
        Dictionary<string, Field> interceptorFields)
    {
        var declaringType = member.Property.OriginType;
        var syncRoot = BuilderTypes.ISyncRoot;
        var legalInitInterceptors = member.InterceptorInfos.Where(x => x.InterfaceInitializer != null).ToArray();
        var relevantCtors = member.Property.IsStatic ? new Method[] { declaringType.StaticConstructor } : declaringType.GetRelevantConstructors().Where(x => x.Name != ".cctor");

        foreach (var ctor in relevantCtors)
        {
            ctor.NewCoder()
                .Context(context =>
                {
                    for (int i = 0; i < legalInitInterceptors.Length; i++)
                    {
                        var item = legalInitInterceptors[i];
                        var field = interceptorFields[item.Attribute.Identification];

                        context.SetValue(field, x => x.NewObj(item.Attribute));

                        if (item.HasSyncRootInterface)
                            context.Load(field).As(BuilderTypes.ISyncRoot).Call(syncRoot.GetMethod_set_SyncRoot(), member.SyncRoot);

                        ModuleWeaver.ImplementAssignMethodAttribute(builder, legalInitInterceptors[i].AssignMethodAttributeInfos, field, item.Attribute.Attribute.Type, context);
                    }

                    context.SetValue(propertyField, x =>
                            x.NewObj(propertyInterceptionInfo.GetMethod_ctor(),
                                member.Property.Getter,
                                member.Property.Setter,
                                member.Property.Name,
                                member.Property.ReturnType,
                                CodeBlocks.This,
                                member.Property.ReturnType.IsArray || member.Property.ReturnType.Implements(typeof(IEnumerable)) ? member.Property.ReturnType.ChildType : null,
                                propertySetter == null ? null : x.NewCoder().NewObj(actionObjectCtor, propertySetter.ThisOrNull(), propertySetter)));

                    for (int i = 0; i < legalInitInterceptors.Length; i++)
                    {
                        var item = legalInitInterceptors[i];
                        var field = interceptorFields[item.Attribute.Identification];
                        context.Load(field).As(item.InterfaceInitializer)
                            .Call(item.InterfaceInitializer.GetMethod_OnInitialize(), propertyField, member.Property.BackingField);
                    }

                    return context;
                })
                .Insert(InsertionPosition.Beginning);
        }
    }

    private static void AddPropertySetterInterception(
        Builder builder,
        BuilderTypePropertyInterceptionInfo propertyInterceptionInfo,
        PropertyBuilderInfo member,
        Field propertyField,
        Method actionObjectCtor,
        Method propertySetter,
        Dictionary<string, Field> interceptorFields)
    {
        var syncRoot = BuilderTypes.ISyncRoot;
        var legalSetterInterceptors = member.InterceptorInfos.Where(x => x.InterfaceSetter != null).ToArray();

        member.Property.Setter
            .NewCoder()
                .Context(context =>
                {
                    if (member.HasInitializer)
                        return context;

                    for (int i = 0; i < legalSetterInterceptors.Length; i++)
                    {
                        var item = legalSetterInterceptors[i];
                        var alwaysCreateNewInstance = item.InterceptorInfo.AlwaysCreateNewInstance && item.InterfaceInitializer == null;
                        var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ??
                            context.AssociatedMethod.GetOrCreateVariable(item.Attribute.Attribute.Type, item.Attribute.Identification);

                        Coder implementInterceptorInitialization(Coder coder)
                        {
                            coder.SetValue(fieldOrVariable, x => x.NewObj(item.Attribute));

                            if (item.HasSyncRootInterface)
                                coder.Load<ICasting>(fieldOrVariable).As(syncRoot.BuilderType.Import()).To<ICallMethod<CallCoder>>().Call(syncRoot.GetMethod_set_SyncRoot(), member.SyncRoot);

                            ModuleWeaver.ImplementAssignMethodAttribute(builder, legalSetterInterceptors[i].AssignMethodAttributeInfos, fieldOrVariable, item.Attribute.Attribute.Type, coder);

                            return coder;
                        }

                        if (alwaysCreateNewInstance)
                            implementInterceptorInitialization(context);
                        else
                            context.If(x => x.Load<IRelationalOperators>(fieldOrVariable).IsNull(), then => implementInterceptorInitialization(then));

                        item.Attribute.Remove();
                    }

                    return context.If(x => x.Load(propertyField).IsNull(), then =>
                         then.SetValue(propertyField, x =>
                             x.NewObj(propertyInterceptionInfo.GetMethod_ctor(),
                                 member.Property.Getter,
                                 member.Property.Setter,
                                 member.Property.Name,
                                 member.Property.ReturnType,
                                 CodeBlocks.This,
                                 member.Property.ReturnType.IsArray || member.Property.ReturnType.Implements(typeof(IEnumerable)) ? member.Property.ReturnType.ChildType : null,
                                 propertySetter == null ? null : x.NewCoder().NewObj(actionObjectCtor, propertySetter.ThisOrNull(), propertySetter))));
                })
                .Try(@try =>
                {
                    if (member.Property.BackingField == null)
                    {
                        // If we don't have a backing field, we will try getting the value from the
                        // getter itself... But in this case we have to watch out that we don't accidentally code a
                        // StackOverFlow
                        var oldvalue = member.Property.Getter == null ? null : member.Property.Setter.GetOrCreateVariable(member.Property.ReturnType);

                        if (oldvalue != null)
                        {
                            var getter = member.Property.Getter.Copy();
                            @try.SetValue(oldvalue, y => y.Call(getter));
                        }

                        for (int i = 0; i < legalSetterInterceptors.Length; i++)
                        {
                            var item = legalSetterInterceptors[i];
                            var alwaysCreateNewInstance = item.InterceptorInfo.AlwaysCreateNewInstance && item.InterfaceInitializer == null;
                            var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ?? @try.AssociatedMethod.GetVariable(item.Attribute.Identification);
                            @try.If(x =>
                               x.Load<ICasting>(fieldOrVariable)
                                   .As(legalSetterInterceptors[i].InterfaceSetter)
                                   .To<ICallMethod<BooleanExpressionCallCoder>>()
                                   .Call(item.InterfaceSetter.GetMethod_OnSet(), propertyField, oldvalue, CodeBlocks.GetParameter(0))
                                   .To<IRelationalOperators>()
                                   .Is(false), then => then.OriginalBody(true));
                        }
                    }
                    else
                    {
                        @try.If(x => x.And(legalSetterInterceptors,
                            (coder, item, i) =>
                            {
                                var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ?? @try.AssociatedMethod.GetVariable(item.Attribute.Identification);
                                return coder.Load<ICasting>(fieldOrVariable)
                                      .As(legalSetterInterceptors[i].InterfaceSetter)
                                      .To<ICallMethod<BooleanExpressionCallCoder>>()
                                      .Call(item.InterfaceSetter.GetMethod_OnSet(), propertyField, member.Property.BackingField, CodeBlocks.GetParameter(0));
                            }).Is(false), then => then.OriginalBody());
                    }

                    return @try;
                })
                .Catch(typeof(Exception), (ex, e) =>
                {
                    return ex.If(x => x.Or(legalSetterInterceptors,
                        (coder, y, i) =>
                        {
                            var fieldOrVariable = interceptorFields[y.Attribute.Identification] as CecilatorBase ?? ex.AssociatedMethod.GetVariable(y.Attribute.Identification);
                            return coder.Load<ICasting>(fieldOrVariable)
                                .As(legalSetterInterceptors[i].InterfaceSetter)
                                .To<ICallMethod<BooleanExpressionCallCoder>>()
                                .Call(legalSetterInterceptors[i].InterfaceSetter.GetMethod_OnException(), e());
                        }).Is(true), then => ex.NewCoder().Rethrow())
                    .DefaultValue()
                    .Return();
                })
                .Finally(x =>
                {
                    for (int i = 0; i < legalSetterInterceptors.Length; i++)
                    {
                        var item = legalSetterInterceptors[i];
                        var fieldOrVariable = interceptorFields[item.Attribute.Identification] as CecilatorBase ?? x.AssociatedMethod.GetVariable(item.Attribute.Identification);
                        x.Load<ICasting>(fieldOrVariable)
                            .As(legalSetterInterceptors[i].InterfaceSetter)
                            .To<ICallMethod<CallCoder>>()
                            .Call(legalSetterInterceptors[i].InterfaceSetter.GetMethod_OnExit());
                    }

                    return x;
                })
                .EndTry()
                .Return()
            .Replace();
    }

    private static void CreatePropertySetterDelegate(Builder builder, PropertyBuilderInfo member, Method propertySetter)
    {
        // If we don't have a backing field and we don't have a setter and getter
        // don't bother creating a setter delegate
        if (member.Property.BackingField == null && propertySetter == null)
            return;

        if (member.Property.BackingField == null && member.Property.Getter != null && member.Property.Setter != null)
        {
            // The copies are used to access the property as fake backing fields to avoid
            // stack overflows by getter and setter calling each other until ethernity
            member.Property.Getter.Copy();
            member.Property.Setter.Copy();

            CreateSetterDelegate(builder, propertySetter, member.Property.ReturnType, member.Property);
        }
        else if (member.Property.BackingField != null && !member.Property.BackingField.FieldType.IsGenericType)
        {
            CreateSetterDelegate(builder, propertySetter, member.Property.BackingField.FieldType, member.Property.BackingField);
        }
        else if (member.Property.BackingField == null && member.Property.Setter != null)
        {
            var methodSetter = member.Property.Setter.Copy();
            propertySetter.NewCoder().Call(methodSetter, CodeBlocks.GetParameter(0)).Return().Replace();
        }
        else if (member.Property.BackingField == null && member.Property.Getter != null)
        {
            // This shouldn't be a thing
        }
        else
            propertySetter.NewCoder().SetValue(member.Property.BackingField, CodeBlocks.GetParameter(0)).Return().Replace();
    }

    private static void CreateSetterDelegate(Builder builder, Method setterDelegateMethod, BuilderType propertyType, object value)
    {
        var setterCode = setterDelegateMethod.NewCoder();

        T CodeMe<T>(Func<Field, T> fieldCode, Func<Property, T> propertyCode) where T : class
        {
            switch (value)
            {
                case Field field: return fieldCode(field);
                case Property property: return propertyCode(property);
                default: return null;
            }
        }

        if (propertyType.ParameterlessContructor != null && propertyType.ParameterlessContructor.IsPublic)
            CodeMe(
                field => setterCode.If(x => x.Load(field).IsNull(), then => then.SetValue(field, x => x.NewObj(propertyType.ParameterlessContructor))),
                property => setterCode.If(x => x.Call(property.Getter).IsNull(), then => then.Call(property.Setter, x => x.NewObj(propertyType.ParameterlessContructor))));

        // Only this if the property implements idisposable
        if (propertyType.Implements(typeof(IDisposable)))
            CodeMe(
                field => setterCode.Call(BuilderTypes.ExtensionsInterception.GetMethod_TryDisposeInternal(), x => x.Load(field)),
                property => setterCode.Call(BuilderTypes.ExtensionsInterception.GetMethod_TryDisposeInternal(), x => x.Call(property.Getter)));

        setterCode.If(x => x.Load(CodeBlocks.GetParameter(0)).IsNull(), then =>
        {
            // Just clear if its clearable
            if (propertyType.Implements(BuilderTypes.IList))
                CodeMe(
                    field => setterCode.Load(field).Call(BuilderTypes.IList.GetMethod_Clear()).Return(),
                    property => setterCode.Call(property.Getter).Call(BuilderTypes.IList.GetMethod_Clear()).Return());
            // Otherwise if the property is not a value type and nullable
            else if (!propertyType.IsValueType || propertyType.IsNullable || propertyType.IsArray)
                CodeMe<CoderBase>(
                    field => setterCode.SetValue(field, null).Return(),
                    property => setterCode.Call(property.Setter, null).Return());
            else // otherwise... throw an exception
                then.ThrowNew(typeof(NotSupportedException), "Value types does not accept null values.");

            return then;
        });

        setterCode.If(x => CodeMe(
            field => x.Load(CodeBlocks.GetParameter(0)).Is(field.FieldType),
            property => x.Load(CodeBlocks.GetParameter(0)).Is(property.ReturnType)), then =>
            CodeMe(
                   field => then.SetValue(field, CodeBlocks.GetParameter(0)).Return(),
                   property => then.Call(property.Setter, CodeBlocks.GetParameter(0)).Return()));

        if (propertyType.Implements(BuilderTypes.IList))
        {
            var add = propertyType.GetMethod("Add", 1);
            var array = setterDelegateMethod.GetOrCreateVariable(propertyType.ChildType.MakeArray());
            setterCode.SetValue(array, CodeBlocks.GetParameter(0));
            setterCode.For(array, (x, item, indexer) => CodeMe(
                field => x.Load(field).Call(add, item()),
                property => x.Call(property.Getter).Call(add, item())));
            if (!add.ReturnType.IsVoid)
                setterCode.Pop();
        }
        else if (propertyType.Implements(typeof(IEnumerable)) || propertyType.IsArray)
            setterCode.If(x => x.Load(CodeBlocks.GetParameter(0)).Is(typeof(IEnumerable)), then =>
               CodeMe(
                   field => then.SetValue(field, CodeBlocks.GetParameter(0)).Return(),
                   property => then.Call(property.Setter, CodeBlocks.GetParameter(0)).Return()))
               .ThrowNew(typeof(NotSupportedException), "Value does not inherits from IEnumerable");
        else if (propertyType.IsEnum)
        {
            // Enums requires special threatment
            setterCode.If(x => x.Load(CodeBlocks.GetParameter(0)).Is(typeof(string)),
                then =>
                {
                    var stringVariable = setterDelegateMethod.GetOrCreateVariable(typeof(string));
                    then.SetValue(stringVariable, CodeBlocks.GetParameter(0));
                    CodeMe( // Cecilator automagically implements a convertion for this
                        field => then.SetValue(field, stringVariable).Return(),
                        property => then.Call(property.Setter, stringVariable).Return());
                    return then;
                });

            CodeMe<CoderBase>(
                field => setterCode.SetValue(field, CodeBlocks.GetParameter(0)),
                property => setterCode.Call(property.Setter, CodeBlocks.GetParameter(0)));
        }
        else
            CodeMe<CoderBase>(
                field => setterCode.SetValue(field, CodeBlocks.GetParameter(0)),
                property => setterCode.Call(property.Setter, CodeBlocks.GetParameter(0)));

        setterCode.Return().Replace();
    }
}