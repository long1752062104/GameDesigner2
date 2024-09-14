#if SERVICE
using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Net.Event;

namespace Net.Helper
{
    /// <summary>
    /// MySql数据库帮助类
    /// </summary>
    public class MySQLHelper
    {
        /// <summary>
        /// 注入MySql.dll 修改dll指令，新增NonQueryHandler事件在MySqlCommand类上，批量执行只需要返回首影响行数的SQL语句使用
        /// </summary>
        /// <returns>是否注入成功</returns>
        public static bool Inject() => Inject(AppDomain.CurrentDomain.BaseDirectory + "/MySql.Data.dll");

        /// <summary>
        /// 注入MySql.dll 修改dll指令，新增NonQueryHandler事件在MySqlCommand类上，批量执行只需要返回首影响行数的SQL语句使用
        /// </summary>
        /// <param name="dllPath">mysql.dll文件路径</param>
        /// <returns></returns>
        public static bool Inject(string dllPath)
        {
            try
            {
                var moduleContext = new ModuleContext();
                var assemblyResolver = new AssemblyResolver(moduleContext);
                var resolver = new Resolver(assemblyResolver);
                moduleContext.AssemblyResolver = assemblyResolver;
                moduleContext.Resolver = resolver;
                assemblyResolver.DefaultModuleContext = moduleContext;
                var module = ModuleDefMD.Load(File.ReadAllBytes(dllPath), moduleContext);
                if (module.Assembly.Version >= new Version(8, 0, 11, 0) && module.Assembly.Version <= new Version(8, 0, 32, 1)) //只有8.0.11.0 -> 8.0.32.1版本IL指令一样
                {
                    var dllPaths = AssemblyHelper.GetAssembliePaths();
                    foreach (var item in dllPaths)
                        assemblyResolver.PreSearchPaths.Add(item);

                    var mySqlCommandType = module.Find("MySql.Data.MySqlClient.MySqlCommand", false);
                    var nonQueryHandler = mySqlCommandType.FindField("NonQueryHandler");
                    if (nonQueryHandler != null)
                        return true;

                    var actionType = module.Import(typeof(Action<int, int>)).ToTypeSig();
                    nonQueryHandler = new FieldDefUser("NonQueryHandler", new FieldSig(actionType), FieldAttributes.Public);
                    mySqlCommandType.Fields.Add(nonQueryHandler);

                    var driver = module.Find("MySql.Data.MySqlClient.Driver", false);
                    var cmd = new FieldDefUser("cmd", new FieldSig(mySqlCommandType.ToTypeSig()), FieldAttributes.Public);
                    var indexField = new FieldDefUser("index", new FieldSig(module.CorLibTypes.Int32), FieldAttributes.Public);
                    driver.Fields.Add(cmd);
                    driver.Fields.Add(indexField);

                    var mySqlDataReaderType = module.Find("MySql.Data.MySqlClient.MySqlDataReader", false);
                    var ctor = mySqlDataReaderType.FindConstructors().First();
                    var ctorBody = ctor.Body;
                    var ctorInstructions = ctorBody.Instructions;

                    var ret = ctorInstructions[ctorInstructions.Count - 1];
                    ctorInstructions.RemoveAt(ctorInstructions.Count - 1);

                    var instructionNew = new Instruction(OpCodes.Ldarg_0);
                    var IL_0065 = instructionNew;
                    ctorInstructions.Add(instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldfld, mySqlDataReaderType.FindField("driver"));
                    ctorInstructions.Add(instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldarg_1);
                    ctorInstructions.Add(instructionNew);

                    instructionNew = new Instruction(OpCodes.Stfld, driver.FindField("cmd"));
                    ctorInstructions.Add(instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldarg_0);
                    ctorInstructions.Add(instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldfld, mySqlDataReaderType.FindField("driver"));
                    ctorInstructions.Add(instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldc_I4_0);
                    ctorInstructions.Add(instructionNew);

                    instructionNew = new Instruction(OpCodes.Stfld, driver.FindField("index"));
                    ctorInstructions.Add(instructionNew);

                    ctorInstructions.Add(ret);

                    ctorInstructions[31].Operand = IL_0065;
                    ctorInstructions[35].Operand = IL_0065;

                    ctorBody.UpdateInstructionOffsets();

                    var nextResult = driver.FindMethod("NextResult");
                    var nextResultBody = nextResult.Body;
                    var nextResultInstructions = nextResultBody.Instructions;

                    nextResultBody.Variables.Add(new Local(module.CorLibTypes.Int32));

                    int index = 30;
                    var IL_0064 = nextResultInstructions[index];

                    instructionNew = new Instruction(OpCodes.Ldarg_0);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldfld, driver.FindField("cmd"));
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldfld, nonQueryHandler);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Brfalse_S, IL_0064); //跳转 IL_0064
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldarg_0);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldfld, driver.FindField("cmd"));
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldfld, nonQueryHandler);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldarg_0);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldarg_0);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldfld, driver.FindField("index"));
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Stloc_3);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldloc_3);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldc_I4_1);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Add);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Stfld, driver.FindField("index"));
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldloc_3);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Ldloc_0);
                    nextResultInstructions.Insert(index++, instructionNew);

                    instructionNew = new Instruction(OpCodes.Callvirt, module.Import(typeof(Action<int, int>).GetMethod("Invoke")));
                    nextResultInstructions.Insert(index++, instructionNew);

                    nextResultInstructions[27].Operand = nextResultInstructions[30];
                    nextResultBody.UpdateInstructionOffsets();
                    module.Write(dllPath);
                    module.Dispose();
                    return true;
                }
                NDebug.LogError("此版本不在注入支持的范围内，请使用8.0.11.0 到 8.0.32.1之间的任意版本，或者联系作者提供新的其他版本注入指令!");
                return false;
            }
            catch (Exception ex)
            {
                NDebug.LogError("注入MySql出错, 在Main执行，必须提前于xxDB.Initxx，否则会出现写入失败! :" + ex);
            }
            return false;
        }

        public static bool CheckInject()
        {
            try
            {
                var dllPath = AppDomain.CurrentDomain.BaseDirectory + "/MySql.Data.dll";
                var module = ModuleDefMD.Load(File.ReadAllBytes(dllPath));

                var mySqlCommandType = module.Find("MySql.Data.MySqlClient.MySqlCommand", false);
                var driver = module.Find("MySql.Data.MySqlClient.Driver", false);

                var mySqlDataReaderType = module.Find("MySql.Data.MySqlClient.MySqlDataReader", false);
                var ctor = mySqlDataReaderType.FindConstructors().First();
                var ctorBody = ctor.Body;
                var ctorInstructions = ctorBody.Instructions;

                var nextResult = driver.FindMethod("NextResult");
                var nextResultBody = nextResult.Body;
                var nextResultInstructions = nextResultBody.Instructions;

                return true;
            }
            catch (Exception ex)
            {
                NDebug.LogError("注入MySql出错, 在Main执行，必须提前于xxDB.Initxx，否则会出现写入失败! :" + ex);
            }
            return false;
        }
    }
}
#endif