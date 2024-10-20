using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace WorkSpacePatcher
{
    public class WorkSpacePatcher : ResoniteMod
    {
        public override string Author => "989onan";
        public override string Link => "https://github.com/989onan/WorkSpacePatcher";
        public override string Name => "WorkSpacePatcher";
        public override string Version => "1.0.0";


        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");

            //patch can update to work outside of user space
            Msg("Loading method info CanChange");
            MethodInfo methodInfo = typeof(FrooxEngine.Workspace).GetProperty("CanUpdate", AccessTools.all).GetGetMethod(true);
            Msg("Loaded method info for CanChange");
            Msg("PATCHING CanChange");
            harmony.Patch(methodInfo, null, null, typeof(PatchWorkSpacePatcher).Method("getpatch"), null);
            Msg("patched CanChange!");

            Msg("Loading method info CanUseOverridenUser");
            MethodInfo methodInfo2 = typeof(FrooxEngine.Workspace).GetMethod("CanUseOverridenUser", AccessTools.all);
            Msg("Loaded method info for CanUseOverridenUser");
            Msg("PATCHING CanUseOverridenUser");
            harmony.Patch(methodInfo2, null, null, typeof(PatchWorkSpacePatcher).Method("getpatch2"), null);
            Msg("patched CanUseOverridenUser!");
            harmony.PatchAll();
        }

        public class PatchWorkSpacePatcher
        {
            public static IEnumerable<CodeInstruction> getpatch(IEnumerable<CodeInstruction> instructions)
            {
                //get rid of the checking for user space, with some extra stuff to make sure it doesn't get the wrong garbage and break the code.

                List<CodeInstruction> codes = instructions.ToList();

                int start_of_removal = codes.FindIndex(o => o.opcode == OpCodes.Call && o.operand.ToString().Contains("get_World"))-1;
                Msg("start of il instructions:"+ start_of_removal.ToString());
                int end_of_removal = codes.FindIndex(o => o.opcode == OpCodes.Ret);
                Msg("end of il instructions:" + end_of_removal.ToString());
                for (int i = start_of_removal; i < end_of_removal+1; i++)
                {
                    codes[i].opcode = OpCodes.Nop;
                    codes[i].operand = OperandType.InlineNone;
                }

                instructions = codes.AsEnumerable();

                return instructions;
            }

            public static IEnumerable<CodeInstruction> getpatch2(IEnumerable<CodeInstruction> instructions)
            {
                //get rid of the checking for user space, with some extra stuff to make sure it doesn't get the wrong garbage and break the code.

                List<CodeInstruction> codes = instructions.ToList();

                int start_of_removal = codes.FindIndex(o => o.opcode == OpCodes.Call && o.operand.ToString().Contains("get_Cloud")) - 1;
                Msg("start of il instructions:" + start_of_removal.ToString());
                int end_of_removal = codes.FindIndex(o => o.opcode == OpCodes.Call && o.operand.ToString().Contains("IsPrimaryGroup"))+1;
                Msg("end of il instructions:" + end_of_removal.ToString());
                for (int i = start_of_removal; i < end_of_removal + 1; i++)
                {
                    codes[i].opcode = OpCodes.Nop;
                    codes[i].operand = OperandType.InlineNone;
                }

                instructions = codes.AsEnumerable();

                return instructions;
            }
        }
    }
}