
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PA.Utilities
{
    public static class ObjectExtensions
    {
        public static T ParseTo<T, U>(this U value, Type type = null)
        {
			var log = Common.Logging.LogManager.GetLogger(typeof(ObjectExtensions));

            Type t = type ?? typeof(T);

            if (!typeof(T).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
            {
                throw new InvalidCastException("Cannot cast <" + type.FullName + "> to <T>");
            }

            if (t.GetTypeInfo().IsEnum)
            {
                return (T)Enum.Parse(t, value.ToString(), true);
            }
            else
            {
                T o = default(T);

                try
                {
                    o = (T)Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    log.Debug(e.Message + "\n" + e.StackTrace);
                }



                if ((T)o == null)
                {
                    ConstructorInfo ci = t.GetTypeInfo().DeclaredConstructors.FirstOrDefault(
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
                            log.Debug(e.Message + "\n" + e.StackTrace);
                        }
                    }
                }

                if ((T)o == null)
                {
                    MethodInfo mi = t.GetTypeInfo().GetDeclaredMethods("Parse").FirstOrDefault(
                                        m => m.GetParameters().Count() == 1 &&
                                        m.GetParameters().First().ParameterType == typeof(string)
                                    );

                    if (mi != null && mi.IsStatic)
                    {
                        try
                        {
                            o = (T)mi.Invoke(null, new object[] { value });
                        }
                        catch (Exception e)
                        {
                            log.Debug(e.Message + "\n" + e.StackTrace);
                        }
                    }
                }

                if ((T)o == null)
                {
                    MethodInfo mi = t.GetTypeInfo().GetDeclaredMethods("CreateFrom").FirstOrDefault(
                                        m => m.GetParameters().Count() == 1 &&
                                        m.GetParameters().First().ParameterType == typeof(string)
                                    );

                    if (mi != null && mi.IsStatic)
                    {
                        try
                        {
                            o = (T)mi.Invoke(null, new object[] { value });
                        }
                        catch (Exception e)
                        {
                           log.Debug(e.Message + "\n" + e.StackTrace);
                        }
                    }
                }

                return o;
            }
        }

        public static IEnumerable<T> ParseTo<T, U>(this IEnumerable<U> value, Type type = null)
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
