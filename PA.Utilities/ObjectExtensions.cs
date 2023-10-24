
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PA.Utilities
{
    public static class ObjectExtensions
    {

        public static T ParseTo<T, U>(this U value, Type? type = null)
            where U : class
            where T : class
        {


            Type t = type ?? typeof(T);

            if (!typeof(T).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
            {
                throw new InvalidCastException("Cannot cast <" + t.FullName + "> to <" + typeof(T) + ">");
            }

            if (t.GetTypeInfo().IsEnum)
            {
                return (T)Enum.Parse(t, value.ToString() ?? string.Empty, true);
            }
            else
            {
                T? o = default(T);

                if (o == null)
                {
                    try
                    {
                        o = (T?)Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        //throw new InvalidCastException("Cannot convert <" + t.FullName + "> to <" + typeof(T) + ">", e);
                    }
                }

                if (o == null)
                {
                    ConstructorInfo? ci = t.GetTypeInfo().DeclaredConstructors.FirstOrDefault(
                                             c => c.GetParameters().Count() == 1 &&
                                             c.GetParameters().First().ParameterType == typeof(U)
                                         );

                    if (ci != null)
                    {
                        try
                        {
                            o = (T)ci.Invoke(new object[] { value });
                        }
                        catch (Exception e)
                        {
                            //throw new InvalidCastException("Cannot convert <" + t.FullName + "> to <" + typeof(T) + ">", e);
                        }
                    }
                }

                if (o == null)
                {
                    MethodInfo? mi = t.GetTypeInfo().GetDeclaredMethods("Parse").FirstOrDefault(
                                        m => m.GetParameters().Count() == 1 &&
                                        m.GetParameters().First().ParameterType == typeof(string)
                                    );

                    if (mi != null && mi.IsStatic)
                    {
                        try
                        {
                            o = (T?)mi.Invoke(null, new object[] { value });
                        }
                        catch (Exception e)
                        {
                            // throw new InvalidCastException("Cannot convert <" + t.FullName + "> to <" + typeof(T) + ">", e);
                        }
                    }
                }

                if (o == null)
                {
                    MethodInfo? mi = t.GetTypeInfo().GetDeclaredMethods("CreateFrom").FirstOrDefault(
                                        m => m.GetParameters().Count() == 1 &&
                                        m.GetParameters().First().ParameterType == typeof(string)
                                    );

                    if (mi != null && mi.IsStatic)
                    {
                        try
                        {
                            o = (T?)mi.Invoke(null, new object[] { value });
                        }
                        catch (Exception e)
                        {
                            // throw new InvalidCastException("Cannot convert <" + t.FullName + "> to <" + typeof(T) + ">", e);
                        }
                    }
                }

                return o;
            }
        }

        public static IEnumerable<T> ParseTo<T, U>(this IEnumerable<U> value, Type? type = null)
            where U : class
            where T : class
        {
            foreach (U v in value)
            {
                yield return v.ParseTo<T, U>(type);
            }
        }

        public static Array ToArray<T>(this IEnumerable<T> value, Type type)
        {

            Array source = value.Where(
                               s => s != null && type.GetTypeInfo().IsAssignableFrom(s.GetType().GetTypeInfo())
                           ).ToArray();

            Array destination = Array.CreateInstance(type, source.Length);
            Array.Copy(source, destination, source.Length);
            return destination;
        }
    }
}
