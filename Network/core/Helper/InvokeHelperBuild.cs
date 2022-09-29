using Net.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Net.Helper
{
    public abstract class InvokePTR
    {
        public object target;
        public abstract object GetValue();
        public abstract void SetValue(object value);
    }

    public class InvokePTR<T, V> : InvokePTR
    {
        public Func<T, V> func;
        public Action<T, V> action;

        public InvokePTR(Func<T, V> func, Action<T, V> action)
        {
            this.func = func;
            this.action = action;
        }

        public override object GetValue()
        {
            return func((T)target);
        }

        public override void SetValue(object value)
        {
            action((T)target, (V)value);
        }
    }
    
    public static class InvokeHelper
    {
        public static Dictionary<Type, Dictionary<string, InvokePTR>> Cache = new Dictionary<Type, Dictionary<string, InvokePTR>>();
    }

    public static class InvokeHelperBuild
    {
        public static string Build()
        {
            var str = @"using Net.Helper;
using System.Collections.Generic;

public static class InvokeHelperGenerate
{
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA
    [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
    internal static void Init()
    {
        InvokeHelper.Cache.Clear();
--
        InvokeHelper.Cache.Add(typeof(TARGETTYPE), new Dictionary<string, InvokePTR>() {
--
            { ""FIELDNAME"", new InvokePTR<TARGETTYPE, FIELDTYPE>(FIELDNAME, FIELDNAME) },
--
        });
--
    }
--
    internal static void FIELDNAME(this TARGETTYPE self, FIELDTYPE FIELDNAME) 
    {
        self.FIELDNAME = FIELDNAME;
    }
--
    internal static RETURNTYPE FIELDNAME(this TARGETTYPE self)
    {
        return self.FIELDNAME;
    }
--
}";

            var codes = str.Split(new string[] { "--" }, 0);
            var sb = new StringBuilder();
            var setgetSb = new StringBuilder();
            sb.Append(codes[0]);
            foreach (var assemblies in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assemblies.GetTypes().Where(t=> !t.IsGenericType & !t.IsAbstract & !t.IsInterface))
                {
                    var dictSB = new StringBuilder();
                    foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        if (member.MemberType == MemberTypes.Field | member.MemberType == MemberTypes.Property)
                        {
                            var syncVar = member.GetCustomAttribute<SyncVar>();
                            if (syncVar != null)
                            {
                                Type ft = null;
                                var fieldType = "";
                                var fieldName = "";
                                if (member is FieldInfo field)
                                {
                                    if (field.IsPrivate)
                                        continue;
                                    ft = field.FieldType;
                                    fieldType = field.FieldType.FullName;
                                    fieldName = field.Name;
                                }
                                else if (member is PropertyInfo property)
                                {
                                    if (!property.CanRead | !property.CanWrite)
                                        continue;
                                    ft = property.PropertyType;
                                    fieldType = property.PropertyType.FullName;
                                    fieldName = property.Name;
                                }
                                if (fieldType.Contains("`"))
                                {
                                    var fff = fieldType.Split('`');
                                    fff[0] += "<";
                                    foreach (var item in ft.GenericTypeArguments)
                                    {
                                        fff[0] += $"{item.FullName},";
                                    }
                                    fff[0] = fff[0].TrimEnd(',');
                                    fff[0] += ">";
                                    fieldType = fff[0];
                                }
                                var code = codes[2].Replace("TARGETTYPE", type.FullName);
                                code = code.Replace("FIELDTYPE", fieldType);
                                code = code.Replace("FIELDNAME", fieldName);
                                dictSB.Append(code);

                                code = codes[5].Replace("TARGETTYPE", type.FullName);
                                code = code.Replace("FIELDTYPE", fieldType);
                                code = code.Replace("FIELDNAME", fieldName);
                                setgetSb.Append(code);

                                code = codes[6].Replace("TARGETTYPE", type.FullName);
                                code = code.Replace("RETURNTYPE", fieldType);
                                code = code.Replace("FIELDNAME", fieldName);
                                setgetSb.Append(code);
                            }
                        }
                    }
                    if (dictSB.Length > 0)
                    {
                        dictSB.Append(codes[3]);

                        var dictSB1 = new StringBuilder();
                        var code = codes[1].Replace("TARGETTYPE", type.FullName);
                        dictSB1.Append(code);
                        dictSB1.Append(dictSB);

                        sb.Append(dictSB1.ToString());
                    }
                }
            }
            sb.Append(codes[4]);
            sb.Append(setgetSb.ToString());
            sb.Append(codes[7]);
            var text = sb.ToString();
            return text;
        }
    }
}
