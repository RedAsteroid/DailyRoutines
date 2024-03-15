using System;
using ClickLib;
using DailyRoutines.Clicks;
using DailyRoutines.Infos;
using DailyRoutines.Managers;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace DailyRoutines.Modules;

[ModuleDescription("AutoCACTitle", "AutoCACDescription", ModuleCategories.GoldSaucer)]
public class AutoPunchingMachine : DailyModuleBase
{
    public override void Init()
    {
        TaskManager ??= new TaskManager { AbortOnTimeout = true, TimeLimitMS = 10000, ShowDebug = false };
        Service.Framework.Update += OnUpdate;
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "PunchingMachine", OnAddonSetup);
    }

    public override void ConfigUI()
    {
        ImGui.Text($"{Service.Lang.GetText("ConflictKey")}: {Service.Config.ConflictKey}");
    }

    private void OnUpdate(IFramework framework)
    {
        if (!TaskManager.IsBusy) return;

        if (Service.KeyState[Service.Config.ConflictKey])
        {
            TaskManager.Abort();
            P.PluginInterface.UiBuilder.AddNotification(Service.Lang.GetText("ConflictKey-InterruptMessage"),
                                                        "Daily Routines", NotificationType.Success);
        }
    }

    private void OnAddonSetup(AddonEvent type, AddonArgs args)
    {
        TaskManager.Enqueue(WaitSelectStringAddon);
        TaskManager.Enqueue(ClickGameButton);
    }

    private static unsafe bool? WaitSelectStringAddon()
    {
        if (TryGetAddonByName<AddonSelectString>("SelectString", out var addon) && IsAddonReady(&addon->AtkUnitBase))
            return Click.TrySendClick("select_string1");

        return false;
    }

    private unsafe bool? ClickGameButton()
    {
        if (TryGetAddonByName<AtkUnitBase>("PunchingMachine", out var addon) && IsAddonReady(addon))
        {
            var button = addon->GetButtonNodeById(23);
            if (button == null || !button->IsEnabled) return false;

            addon->IsVisible = false;

            var handler = new ClickPunchingMachineDR();
            handler.Play(new Random().Next(1700, 1999));

            TaskManager.Enqueue(StartAnotherRound);
            return true;
        }

        return false;
    }

    private static unsafe bool? StartAnotherRound()
    {
        if (IsOccupied()) return false;
        var machineTarget = Service.Target.PreviousTarget;
        var machine = machineTarget.Name.ExtractText().Contains("重击伽美什") ? (GameObject*)machineTarget.Address : null;

        if (machine != null)
        {
            TargetSystem.Instance()->InteractWithObject(machine);
            return true;
        }

        return false;
    }

    public override void Uninit()
    {
        Service.Framework.Update -= OnUpdate;
        Service.AddonLifecycle.UnregisterListener(OnAddonSetup);

        base.Uninit();
    }
}
