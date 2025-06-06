using CringePlugins.Abstractions;
using ImGuiNET;
using System.Numerics;
using VRage;
using VRageRender;

namespace CringePlugins.Ui;
public sealed class NotificationsComponent : IRenderComponent
{
    private static int _globalNotificationId;
    private const float TransitionTime = .5f;
    private const float YPadding = 5f;

    private static Vector2 _notificationSize = new(300, 75);
    private static readonly List<NotificationInstruction> Notifications = [];

    private static Size WindowSize => ((Form)MyVRage.Platform.Windows.Window).Size;

    private static float _time;

    public void OnFrame()
    {
        var y = 0f;

        var lastY = _notificationSize.Y;
        var viewportPos = ImGui.GetMainViewport().Pos;

        //todo: consider adding a limit to the number of messages that can be displayed at once
        for (var i = Notifications.Count; i-- > 0;)
        {
            var lerpMult = 0f;
            var notification = Notifications[i];
            var startDelta = _time - notification.StartTime;
            var endDelta = _time - notification.HideTime;

            var inTransition = true;
            if (startDelta is > 0f and < TransitionTime)
            {
                lerpMult = startDelta / TransitionTime;
            }
            else if (endDelta is > 0f and < TransitionTime)
            {
                lerpMult = 1f - (endDelta / TransitionTime);
            }
            else if (startDelta > 0f && endDelta < 0f)
            {
                lerpMult = 1f;
                inTransition = false;
            }

            y += (lastY + YPadding) * EaseInOutCubic(lerpMult);
            if (!inTransition && y < _notificationSize.Y + YPadding)
                y = _notificationSize.Y + YPadding;

            ImGui.SetNextWindowPos(new Vector2(WindowSize.Width, y * EaseInOutCubic(lerpMult)) - _notificationSize + viewportPos);
            ImGui.SetNextWindowSize(new Vector2(_notificationSize.X, 0f));

            Vector2 lastWinSize = Vector2.Zero;

            if (ImGui.Begin($"notification-{notification.GlobalId}", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.Tooltip))
            {
                notification.RenderCallback?.Invoke();

                //todo: add indicator for when message will expire. probaby just want a rectangle at the bottom of the message
                //var fraction = Math.Min(1f, (_time - notification.StartTime) / (notification.HideTime - notification.StartTime));
                //ImGui.ProgressBar(fraction, new Vector2(_notificationSize.X, 10));

                lastWinSize = ImGui.GetWindowSize();

                ImGui.End();
            }

            lastY = lastWinSize.Y;
        }


        Notifications.RemoveAll(x => x.IsGarbage);

        _time += MyCommon.GetLastFrameDelta();
    }
    public static void SpawnNotification(float showTime, Action renderCallback)
        => Notifications.Add(new NotificationInstruction(_globalNotificationId++, renderCallback, _time, _time + showTime));
    public static void SpawnNotification(float showTime, string text)
        => Notifications.Add(new NotificationInstruction(_globalNotificationId++, () => ImGui.TextWrapped(text), _time, _time + showTime));

    private static float EaseInOutCubic(float x) => x < 0.5f
        ? 4f * x * x * x
        : 1f - MathF.Pow((-2f * x + 2f), 3f) / 2f;

    private readonly struct NotificationInstruction(int id, Action renderCallback, float startTime, float hideTime)
    {
        public readonly int GlobalId = id;
        public readonly Action RenderCallback = renderCallback;
        public readonly float StartTime = startTime;
        public readonly float HideTime = hideTime;

        public readonly bool IsGarbage
            => _time > HideTime + TransitionTime;
    }
}
