using System;
using System.Collections.Generic;
using System.Threading;
using FenUISharp;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using FenUISharp.WinFeatures;

namespace DynamicWin;

public class DragDropShakeListener
{
	private bool isMouseDown;
	private bool explorerDrag;

	private bool isDragging;

	private bool isShaking;

	private Vector2 startPosition;

	private Vector2 lastShakePos;

	private DateTime lastShakePosCaptureTime;

	private List<(DateTime time, float deltaX)> shakeHistory = new List<(DateTime, float)>();

	private List<DateTime> flipHistory = new List<DateTime>();

	private float lastDeltaX;

	private DateTime timeSinceLast;

	private Action? OnDragStop { get; set; }

	public DragDropShakeListener()
	{
		GlobalHooks globalHooks = WindowFeatures.GlobalHooks;
		globalHooks.OnMouseAction = (Action<MouseInputCode>)Delegate.Combine(globalHooks.OnMouseAction, new Action<MouseInputCode>(OnGlobalMouseAction));
		GlobalHooks globalHooks2 = WindowFeatures.GlobalHooks;
		globalHooks2.OnMouseMove = (Action<Vector2>)Delegate.Combine(globalHooks2.OnMouseMove, new Action<Vector2>(OnGlobalMouseMove));
	}

	private void OnGlobalMouseMove(Vector2 vector)
	{
		if (explorerDrag && Settings.GetSetting("ft.quick-drop", defaultValue: true) && isMouseDown)
		{
			Vector2 mousePosition = GlobalHooks.MousePosition;
			if ((mousePosition - startPosition).Magnitude > 50f)
			{
				isDragging = true;
			}
			if (isDragging && !isShaking)
			{
				UpdateShakeDetection(mousePosition);
			}
		}
	}

	private void UpdateShakeDetection(Vector2 currentPos)
	{
		float num = currentPos.x - lastShakePos.x;
		float num2 = (float)(DateTime.Now - lastShakePosCaptureTime).TotalSeconds;
		if (num2 < 0.05f)
		{
			return;
		}
		float num3 = num / num2;
		if (Math.Abs(num3) > 430f)
		{
			if (Math.Sign(num3) != Math.Sign(lastDeltaX) && Math.Sign(lastDeltaX) != 0)
			{
				flipHistory.Add(DateTime.Now);
				FLogger.Log("Flip detected. Total: " + flipHistory.Count);
			}
			lastDeltaX = num3;
			shakeHistory.Add((DateTime.Now, num3));
		}
		shakeHistory.RemoveAll(((DateTime time, float deltaX) entry) => (DateTime.Now - entry.time).TotalMilliseconds > 500.0);
		flipHistory.RemoveAll((DateTime entry) => (DateTime.Now - entry).TotalMilliseconds > 500.0);
		if (flipHistory.Count >= 5)
		{
			FLogger.Log("Mouse Shake Detected");
			shakeHistory.Clear();
			isShaking = true;
			CreateDummyWindowAndTestForDragDrop();
		}
		lastShakePos = currentPos;
		lastShakePosCaptureTime = DateTime.Now;
	}

	private void OnGlobalMouseAction(MouseInputCode code)
	{
		Console.WriteLine(code);
		if (code.button == MouseInputButton.Left)
		{
			explorerDrag = code.state == MouseInputState.Down && ForegroundPolicy.ExplorerIsForeground();
			flipHistory.Clear();
			shakeHistory.Clear();
			if (code.state == MouseInputState.Up && isDragging)
			{
				OnDragStop?.Invoke();
				OnDragStop = null;
			}
			startPosition = GlobalHooks.MousePosition;
			isMouseDown = code.state == MouseInputState.Down;
			isDragging = false;
			isShaking = false;
		}
	}

	private void CreateDummyWindowAndTestForDragDrop()
	{
		if (!explorerDrag || !ForegroundPolicy.ExplorerIsForeground() || ForegroundPolicy.ShouldYield()) return;
		if ((DateTime.Now - timeSinceLast).TotalSeconds <= 2.0)
		{
			return;
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			if (!ForegroundPolicy.ExplorerIsForeground() || ForegroundPolicy.ShouldYield()) return;
			FTransparentWindow window = new FTransparentWindow("Drag Drop Tester", "dragDropTester", GlobalHooks.MousePosition - new Vector2(-500f, 500f), new Vector2(250f, 250f));
			window.Properties.ShowWithoutActivation = true;
			window.Properties.AlwaysOnTop = true;
			window.Properties.VisibleInTaskbar = false;
			window.Loop.PauseUpdateLoopWhenHidden = false;
			window.WithView(new DropObstructorView());
			window.LogicDispatcher.Invoke(delegate
			{
				if (window != null && window.DropTarget != null)
				{
					if (ForegroundPolicy.ShouldYield()) { window.Dispose(); return; }
					window.DropTarget.dropEffect.SetValue(this, DROPEFFECT.Copy | DROPEFFECT.Move, 99f);
					DragDropHandler? dropTarget = window.DropTarget;
					dropTarget.dragEnter = (Action<FDropData>)Delegate.Combine(dropTarget.dragEnter, (Action<FDropData>)delegate
					{
						CreateDropWindow();
						FLogger.Log("User drop action detected!");
						window.LogicDispatcher?.Invoke(delegate
						{
							window?.Dispose();
							window = null;
						});
					});
					window.Shape.Position = GlobalHooks.MousePosition - new Vector2(125f, 125f);
				}
			});
			window.LogicDispatcher.InvokeLater(delegate
			{
				window?.Dispose();
				window = null;
			}, 1f);
			window.Properties.IsWindowVisible = true;
			window.BeginWindowLoop();
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		timeSinceLast = DateTime.Now;
	}

	private void CreateDropWindow()
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			FTransparentWindow window = new FTransparentWindow("Quick Drop Area", "quickDropArea", GlobalHooks.MousePosition + new Vector2(75f, -160f), new Vector2(145f, 145f));
			window.Properties.ShowWithoutActivation = true;
			window.Properties.AlwaysOnTop = true;
			window.Properties.VisibleInTaskbar = false;
			window.WithView(new QuickDropView());
			OnDragStop = (Action)Delegate.Combine(OnDragStop, (Action)delegate
			{
				QuickDrop.Destroy();
				window.LogicDispatcher.InvokeLater(window.Dispose, 1f);
			});
			window.Properties.IsWindowVisible = true;
			window.BeginWindowLoop();
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
	}
}

