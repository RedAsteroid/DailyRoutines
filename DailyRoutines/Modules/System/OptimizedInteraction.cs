using DailyRoutines.Infos;
using DailyRoutines.Managers;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace DailyRoutines.Modules;

[ModuleDescription("OptimizedInteractionTitle", "OptimizedInteractionDescription", ModuleCategories.系统)]
public unsafe class OptimizedInteraction : DailyModuleBase
{
    // 当前位置无法进行该操作
    private delegate bool CameraObjectBlockedDelegate(nint a1, nint a2, nint a3);
    [Signature("E8 ?? ?? ?? ?? 84 C0 75 ?? B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? EB ?? 40 B7", DetourName = nameof(CameraObjectBlockedDetour))]
    private static Hook<CameraObjectBlockedDelegate>? CameraObjectBlockedHook;

    // 目标处于视野之外
    private delegate bool IsObjectInViewRangeDelegate(TargetSystem* system, GameObject* gameObject);
    [Signature("E8 ?? ?? ?? ?? 84 C0 75 ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 52 ?? 48 8B C8 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? E9", DetourName = nameof(IsObjectInViewRangeHookDetour))]
    private static Hook<IsObjectInViewRangeDelegate>? IsObjectInViewRangeHook;

    // 跳跃中无法进行该操作 / 飞行中无法进行该操作
    private delegate bool InteractCheck0Delegate(nint a1, nint a2, nint a3, nint a4, bool a5);
    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 49 8B 00 49 8B C8", DetourName = nameof(InteractCheck0Detour))]
    private static Hook<InteractCheck0Delegate>? InteractCheck0Hook;

    // 跳跃中无法进行该操作
    private delegate bool IsPlayerOnJumpingDelegate(nint a1);
    [Signature("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 48 8D 8D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 85", 
               DetourName = nameof(IsPlayerOnJumpingDetour))]
    private static Hook<IsPlayerOnJumpingDelegate>? IsPlayerOnJumping0Hook;
    [Signature("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 48 8D 8D ?? ?? ?? ?? 48 89 9C 24", DetourName = nameof(IsPlayerOnJumpingDetour))]
    private static Hook<IsPlayerOnJumpingDelegate>? IsPlayerOnJumping1Hook;

    // 检查目标距离 / 高低
    private delegate bool CheckTargetPositionDelegate(nint a1, nint a2, nint a3, byte a4, byte a5);
    [Signature("40 53 57 41 56 48 83 EC ?? 48 8B 02", DetourName = nameof(CheckTargetPositionDetour))]
    private static Hook<CheckTargetPositionDelegate>? CheckTargetPositionHook;

    // 检查目标是否位于同一环境
    private delegate bool IsTargetInSameEnvironmentDelegate(nint a1);
    [Signature("E8 ?? ?? ?? ?? 3A D8 74 ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 0F 84", 
               DetourName = nameof(IsTargetInSameEnvironmentDetour))]
    private static Hook<IsTargetInSameEnvironmentDelegate>? IsTargetInSameEnvironmentHook;

    // 玩家当前是否正在潜水
    private delegate bool IsPlayerOnDivingDelegate(nint a1);
    [Signature("E8 ?? ?? ?? ?? 84 C0 74 ?? F3 0F 10 35 ?? ?? ?? ?? F3 0F 10 3D ?? ?? ?? ?? F3 44 0F 10 05")]
    private static IsPlayerOnDivingDelegate? IsPlayerOnDiving;

    public override void Init()
    {
        Service.Hook.InitializeFromAttributes(this);
        CameraObjectBlockedHook.Enable();
        IsObjectInViewRangeHook.Enable();
        InteractCheck0Hook.Enable();
        IsPlayerOnJumping0Hook.Enable();
        IsPlayerOnJumping1Hook.Enable();
        CheckTargetPositionHook.Enable();
        IsTargetInSameEnvironmentHook.Enable();
    }

    private static bool CameraObjectBlockedDetour(nint a1, nint a2, nint a3)
    {
        // var original = CameraObjectBlockedHook.Original(a1, a2, a3);
        return true;
    }

    private static bool IsObjectInViewRangeHookDetour(TargetSystem* system, GameObject* gameObject)
    {
        // var original = IsObjectInViewRangeHook.Original(system, gameObject);
        return true;
    }

    private static bool InteractCheck0Detour(nint a1, nint a2, nint a3, nint a4, bool a5)
    {
        // var original = InteractCheck0Hook.Original(a1, a2, a3, a4, false);
        return true;
    }

    private static bool IsPlayerOnJumpingDetour(nint a1)
    {
        return false;
    }

    private static bool CheckTargetPositionDetour(nint a1, nint a2, nint a3, byte a4, byte a5)
    {
        // var original = CheckTargetPositionHook.Original(a1, a2, a3, a4, a5);
        return true;
    }

    private static bool IsTargetInSameEnvironmentDetour(nint a1)
    {
        // var original = IsTargetInSameEnvironmentHook.Original(a1);
        return IsPlayerOnDiving(Service.ClientState.LocalPlayer.Address + 528);
    }
}
