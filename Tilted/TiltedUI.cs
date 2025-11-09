using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

namespace Tilted
{
  public class TiltedUI : Window, IDisposable
  {
    private readonly ConfigurationMKII configuration;

    public TiltedUI(ConfigurationMKII configuration)
      : base(
        "Tilted##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.configuration = configuration;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
      base.OnClose();
      configuration.IsVisible = false;
      configuration.Save();
    }

    private void DrawSectionMasterEnable()
    {
      // can't ref a property, so use a local copy
      var enabled = configuration.MasterEnable;
      if (ImGui.Checkbox("总开关", ref enabled))
      {
        configuration.MasterEnable = enabled;
        configuration.Save();
      }
      var enabledInGpose = configuration.EnableInGpose;
      if (ImGui.Checkbox("拍照模式启用", ref enabledInGpose))
      {
        configuration.EnableInGpose = enabledInGpose;
        configuration.Save();
      }
    }

    private void DrawCheckbox(string label, string key, Func<bool> getter, Action<bool> setter)
    {
      ImGui.TextWrapped(label);
      var value = getter();
      if (ImGui.Checkbox(key, ref value))
      {
        setter(value);
      }
    }

    private void DrawTriggersSection()
    {
      if (ImGui.CollapsingHeader("触发条件"))
      {
        ImGui.Indent();

        if (ImGui.CollapsingHeader("副本"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "进入副本时启用，如迷宫和讨伐\n离开副本后禁用",
            "启用##EnabledInDuties",
            () => configuration.EnableInDuty,
            (value) =>
            {
              configuration.EnableInDuty = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("战斗"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "进入战斗时启用\n战斗结束后等待“超时”秒再禁用\n点击“设置”复制自动收刀计时",
            "启用##EnabledInCombat",
            () => configuration.EnableInCombat,
            (value) =>
            {
              configuration.EnableInCombat = value;
              configuration.Save();
            }
          );

          var combatTimeout = configuration.CombatTimeoutSeconds;
          if (ImGui.Button("设置##SetCombatTimeout"))
          {
            combatTimeout = TiltedHelper.GetWeaponAutoPutAwayTime();
          }
          ImGui.SameLine();
          if (ImGui.InputFloat("超时##CombatTmeout", ref combatTimeout, 0.1f, 1.0f))
          {
            combatTimeout = Math.Clamp(combatTimeout, 0f, 10f);
            configuration.CombatTimeoutSeconds = combatTimeout;
            configuration.Save();
          }
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("收刀状态"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "拔刀时启用\n收刀时禁用",
            "启用##EnabledUnsheathed",
            () => configuration.EnableUnsheathed,
            (value) =>
            {
              configuration.EnableUnsheathed = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("骑乘"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "骑乘坐骑时启用\n下坐骑时禁用",
            "启用##EnabledWhileMounted",
            () => configuration.EnableMounted,
            (value) =>
            {
              configuration.EnableMounted = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("飞行"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "骑乘坐骑飞行时启用\n着陆后禁用",
            "启用##EnabledWhileFlying",
            () => configuration.EnableFlying,
            (value) =>
            {
              configuration.EnableFlying = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("镜头靠近"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "缩放超过设定阈值时启用\n缩放回去时禁用",
            "启用##EnabledZoomed",
            () => configuration.EnableZoomed,
            (value) =>
            {
              configuration.EnableZoomed = value;
              configuration.Save();
            }
          );

          var triggerDistance = configuration.ZoomedTriggerDistance;
          if (ImGui.Button("设置##TriggerDistance"))
          {
            triggerDistance = TiltedHelper.GetActiveCameraDistance();
          }
          ImGui.SameLine();
          if (ImGui.InputFloat("启用阈值##TriggerDistance", ref triggerDistance, 0.1f, 1.0f))
          {
            triggerDistance = Math.Clamp(triggerDistance, 1.5f, 20f);
          }

          if (triggerDistance != configuration.ZoomedTriggerDistance)
          {
            configuration.ZoomedTriggerDistance = triggerDistance;
            configuration.Save();
          }

          ImGui.Unindent();
        }

        ImGui.Unindent();
      }
    }

    private void DrawTweaksSection()
    {
      if (ImGui.CollapsingHeader("调整选项"))
      {
        ImGui.Indent();

        DrawTiltAngleSection();
        DrawCameraDistanceSection();

        ImGui.Unindent();
      }
    }

    private void DrawTiltAngleSection()
    {
      if (ImGui.CollapsingHeader("倾斜角度"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("这些值会修改角色设置中的“第三人称镜头角度”\n点击“设置”复制当前镜头倾斜角度");

        var tiltEnabled = configuration.EnableTweakingCameraTilt;
        if (ImGui.Checkbox("启用##TweakCameraTilt", ref tiltEnabled))
        {
          configuration.EnableTweakingCameraTilt = tiltEnabled;
          configuration.Save();
        }

        int inTilt = (int)configuration.CameraTiltWhenEnabled;
        if (ImGui.Button("设置##InTilt"))
        {
          inTilt = (int)TiltedHelper.GetTiltOffset();
        }
        ImGui.SameLine();
        if (ImGui.InputInt("启用时##EnabledTilt", ref inTilt))
        {
          inTilt = Math.Clamp(inTilt, 0, 100);
        }

        if (inTilt != configuration.CameraTiltWhenEnabled)
        {
          configuration.CameraTiltWhenEnabled = (uint)inTilt;
          configuration.Save();
        }

        int outTilt = (int)configuration.CameraTiltWhenDisabled;
        if (ImGui.Button("设置##OutTilt"))
        {
          outTilt = (int)TiltedHelper.GetTiltOffset();
        }
        ImGui.SameLine();
        if (ImGui.InputInt("禁用时##DisabledTilt", ref outTilt))
        {
          outTilt = Math.Clamp(outTilt, 0, 100);
        }

        if (outTilt != configuration.CameraTiltWhenDisabled)
        {
          configuration.CameraTiltWhenDisabled = (uint)outTilt;
          configuration.Save();
        }

        var smoothing = configuration.EnableCameraTiltSmoothing;
        if (ImGui.Checkbox("平滑##SmoothingTilt", ref smoothing))
        {
          configuration.EnableCameraTiltSmoothing = smoothing;
          configuration.Save();
        }

        var mapping = configuration.EnableDistanceToTiltMapping;
        if (ImGui.Checkbox("按距离插值##MappingTilt", ref mapping))
        {
          configuration.EnableDistanceToTiltMapping = mapping;
          configuration.Save();
        }

        float maximumDistance = configuration.MaximumCameraDistance;
        if (ImGui.Button("设置##MaximumDistance"))
        {
          maximumDistance = TiltedHelper.GetActiveCameraDistance();
          configuration.MaximumCameraDistance = maximumDistance;
          configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("最大距离##MaximumDistance", ref maximumDistance))
        {
          maximumDistance = Math.Clamp(maximumDistance, 1.5f, 20.0f);
          configuration.MaximumCameraDistance = maximumDistance;
          configuration.Save();
        }

        float minimumDistance = configuration.MinimumCameraDistance;
        if (ImGui.Button("设置##MinimumDistance"))
        {
          minimumDistance = TiltedHelper.GetActiveCameraDistance();
          configuration.MinimumCameraDistance = minimumDistance;
          configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("最小距离##MinimumDistance", ref minimumDistance))
        {
          minimumDistance = Math.Clamp(minimumDistance, 1.5f, 20.0f);
          configuration.MinimumCameraDistance = minimumDistance;
          configuration.Save();
        }

        ImGui.Indent();
        ImGui.TextWrapped("启用后，镜头倾斜角会在“启用时”和“禁用时”的值之间，按镜头与角色的距离进行插值"
          + "\n开启该项时，触发器和平滑效果不生效"
          );
        ImGui.Unindent();

        ImGui.Unindent();
      }
    }

    private void DrawCameraDistanceSection()
    {
      if (ImGui.CollapsingHeader("镜头距离"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("调整镜头距离（缩放）"
          + "\n只在状态切换时生效"
          + "\n始终应用平滑"
          + "\n点击“设置”复制当前镜头距离"
          + "\n使用“镜头靠近”触发时禁用");

        var distanceEnabled = configuration.EnableCameraDistanceTweaking;
        if (ImGui.Checkbox("启用##TweakCameraDistance", ref distanceEnabled))
        {
          configuration.EnableCameraDistanceTweaking = distanceEnabled;
          configuration.Save();
        }

        var inDistance = configuration.CameraDistanceWhenEnabled;
        if (ImGui.Button("设置##InDistance"))
        {
          inDistance = TiltedHelper.GetActiveCameraDistance();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("启用时##EnabledDistance", ref inDistance, 0.1f, 1.0f))
        {
          inDistance = Math.Clamp(inDistance, 1.5f, 20f);
        }

        if (inDistance != configuration.CameraDistanceWhenEnabled)
        {
          configuration.CameraDistanceWhenEnabled = inDistance;
          configuration.Save();
        }

        var outDistance = configuration.CameraDistanceWhenDisabled;
        if (ImGui.Button("设置##OutDistance"))
        {
          outDistance = TiltedHelper.GetActiveCameraDistance();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("禁用时##DisabledDistance", ref outDistance, 0.1f, 1.0f))
        {
          outDistance = Math.Clamp(outDistance, 1.5f, 20f);
        }

        if (outDistance != configuration.CameraDistanceWhenDisabled)
        {
          configuration.CameraDistanceWhenDisabled = outDistance;
          configuration.Save();
        }
        ImGui.Unindent();
      }
    }

    public void DrawDebugSection()
    {
      if (ImGui.CollapsingHeader("调试选项"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("调试选项\n用于测试你的设置");
        var forceEnabled = configuration.DebugForceEnabled;
        if (ImGui.Checkbox("强制启用", ref forceEnabled))
        {
          configuration.DebugForceEnabled = forceEnabled;
          configuration.Save();
        }

        ImGui.Unindent();
      }
    }

    public override void Draw()
    {
      DrawSectionMasterEnable();

      ImGui.Separator();

      DrawTriggersSection();

      ImGui.Separator();

      DrawTweaksSection();

      ImGui.Separator();

      DrawDebugSection();
    }
  }
}
