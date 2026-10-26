using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using FenUISharp;
using FenUISharp.AnimatedVectors;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects;
using FenUISharp.Objects.Buttons;
using FenUISharp.Objects.Text;
using FenUISharp.Objects.Text.Layout;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace DynamicWin;

public class OnboardingView : View
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass18_0
	{
		public DistortionPane distPane;

		public AnimatorComponent animatorComponent;

		internal Vector2 _003CAddDistortionPane_003Eb__0()
		{
			return new Vector2(distPane.Shape.GlobalBounds.Width / 2f, 0f);
		}

		internal void _003CAddDistortionPane_003Eb__1(float x)
		{
			distPane.BulgeRadius.SetStaticState(RMath.Remap(x, 0f, 1f, 10f, 3250f));
			distPane.BulgeStrength.SetStaticState(RMath.Remap(Easing.EaseOutQuint(animatorComponent.UneasedTime), 0f, 1f, 0.1f, 0f));
		}

		internal void _003CAddDistortionPane_003Eb__2()
		{
			FContext.GetCurrentDispatcher().Invoke(distPane.Dispose);
		}
	}

	private Action? _onEndOnboarding;

	private SimpleVideoPlayer? videoPlayer;

	private float _screenFadeOpacity;

	private float _videoPlayerOpacity = 1f;

	private FPanel welcomePanel;

	private FPanel welcomePanelContentPane;

	private List<View> _onboardingViews = new List<View>
	{
		new OnboardingFirstLook(),
		new OnboardingMonitorSelect(),
		new OnboardingCalendar()
	};

	private int currentOnboardingView;

	private ModelViewPane onboardingViewPane;

	public OnboardingView(Action? onEnd = null)
	{
		_onEndOnboarding = onEnd;
	}

	public override List<UIObject> Create()
	{
		FContext.GetCurrentWindow().Properties.TransparentIgnoreClientInteractions = true;
		FPanel fadePanel = new FPanel(() => Vector2.Zero, () => Vector2.Zero, 0f, () => SKColors.Black);
		fadePanel.ImageEffects.Opacity.SetResponsiveState(() => _screenFadeOpacity);
		fadePanel.ImageEffects.Opacity.SetProcessor((float x) => RMath.Lerp(fadePanel.ImageEffects.Opacity.CachedValue, x, FContext.DeltaTime * 3f));
		fadePanel.Layout.StretchHorizontal.SetStaticState(value: true);
		fadePanel.Layout.StretchVertical.SetStaticState(value: true);
		string fileName = "DWV2LogoAnimOut.webm";
		videoPlayer = new SimpleVideoPlayer(GetVideoStream(fileName));
		videoPlayer.Transform.Size.SetStaticState(new Vector2(640f, 640f));
		videoPlayer.ImageEffects.Opacity.SetResponsiveState(() => _videoPlayerOpacity);
		videoPlayer.ImageEffects.Opacity.SetProcessor((float x) => RMath.Lerp(videoPlayer.ImageEffects.Opacity.CachedValue, x, FContext.DeltaTime * 3f));
		welcomePanel = new FPanel(null, () => new Vector2(500f, 350f), null, () => new SKColor(15, 15, 15));
		welcomePanel.Visible.SetStaticState(value: false);
		FContext.GetCurrentDispatcher().InvokeLater(PlayIntro, 1f);
		FContext.GetCurrentDispatcher().InvokeLater(OpenWelcomeMenu, 4.5f);
		return new List<UIObject> { fadePanel, videoPlayer, welcomePanel };
	}

	private void PlayIntro()
	{
		_screenFadeOpacity = 0.5f;
		videoPlayer?.Play();
	}

	private void OpenWelcomeMenu()
	{
		FContext.GetCurrentWindow().Properties.TransparentIgnoreClientInteractions = true;
		welcomePanelContentPane = new FPanel(null, null, null, () => SKColors.Transparent);
		welcomePanelContentPane.SetParent(welcomePanel);
		welcomePanelContentPane.Layout.StretchHorizontal.SetStaticState(value: true);
		welcomePanelContentPane.Layout.StretchVertical.SetStaticState(value: true);
		FPanel toolbar = new FPanel(() => new Vector2(0f, 15f), () => new Vector2(35f, 35f), null, () => SKColors.Transparent);
		toolbar.ImageEffects.Opacity.SetResponsiveState(() => (!toolbar.InteractiveSurface.IsMouseHovering) ? 0.5f : 1f);
		toolbar.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		toolbar.InteractiveSurface.MouseInteractionCallbackOnChildMouseInteraction = true;
		toolbar.ImageEffects.Opacity.SetProcessor((float x) => RMath.Lerp(toolbar.ImageEffects.Opacity.CachedValue, x, FContext.DeltaTime * 5f));
		toolbar.Layout.StretchHorizontal.SetStaticState(value: true);
		toolbar.Layout.Alignment.SetStaticState(new Vector2(0.5f, 0f));
		toolbar.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 0f));
		toolbar.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(15f, 15f));
		toolbar.SetParent(welcomePanelContentPane);
		FImage fImage = new FImage(() => Resources.GetImage("v2logo"), drawBackground: false, dynamicColor: false, () => new Vector2(5f, 0f), () => new Vector2(30f, 30f));
		fImage.SetParent(toolbar);
		fImage.Layout.Alignment.SetStaticState(new Vector2(0f, 0.5f));
		fImage.Layout.AlignmentAnchor.SetStaticState(new Vector2(0f, 0.5f));
		FText fText = new FText(TextModelFactory.CreateBasic("Dynamic Win", 14f, bold: false, italic: false, underlined: false, null, null, new TextAlign
		{
			HorizontalAlign = TextAlign.AlignType.Start,
			VerticalAlign = TextAlign.AlignType.Middle
		}), null, () => new Vector2(0f, 30f));
		fText.SetParent(toolbar);
		fText.Layout.StretchHorizontal.SetStaticState(value: true);
		fText.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(40f, 100f));
		IconButton iconButton = new IconButton("", () => Resources.GetImage("x"), delegate
		{
			Environment.Exit(0);
		}, 30f);
		iconButton.SetParent(toolbar);
		iconButton.Transform.LocalPosition.SetStaticState(new Vector2(-5f, 0f));
		iconButton.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		iconButton.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		_screenFadeOpacity = 0f;
		_videoPlayerOpacity = 0f;
		AnimatorComponent obj = new AnimatorComponent(welcomePanel, Easing.EaseOutExpo)
		{
			Duration = 1f
		};
		obj.OnValueUpdate = (Action<float>)Delegate.Combine(obj.OnValueUpdate, (Action<float>)delegate(float x)
		{
			welcomePanel.ImageEffects.Opacity.SetStaticState(x);
			welcomePanel.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(x, 0f, 1f, 0.9f, 1f));
		});
		obj.Start();
		FContext.GetCurrentDispatcher().InvokeLater(delegate
		{
			FContext.GetCurrentWindow().Shape.AddOrUpdateWindowRegion(this, () => welcomePanel.Shape.GlobalBounds);
		}, 1f);
		welcomePanel.Visible.SetStaticState(value: true);
		AddContentToWelcomeMenu();
	}

	private void AddContentToWelcomeMenu()
	{
		FAVDisplay welcomeDisplay = new FAVDisplay(() => Resources.GetFAV("hello"), dynamicColor: false, null, () => new Vector2(175f, 65f));
		welcomeDisplay.PlayAnimation("in", delegate
		{
			AnimatorComponent animatorComponent = new AnimatorComponent(welcomePanelContentPane, Easing.CombineInOut(Easing.EaseInExpo, Easing.EaseOutExpo))
			{
				Duration = 1f
			};
			animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
			{
				welcomeDisplay.ImageEffects.Opacity.SetStaticState(1f - x);
				welcomeDisplay.Transform.Scale.SetStaticState(Vector2.One * RMath.Remap(x, 0f, 1f, 1f, 0.85f));
			});
			animatorComponent.OnComplete = (Action)Delegate.Combine(animatorComponent.OnComplete, (Action)delegate
			{
				FContext.GetCurrentDispatcher().Invoke(welcomeDisplay.Dispose);
				BeginOnboardingMenus();
			});
			FContext.GetCurrentDispatcher().InvokeLater(animatorComponent.Start, 0.35f);
		});
		welcomeDisplay.SetParent(welcomePanelContentPane);
	}

	private void BeginOnboardingMenus()
	{
		FPanel bottomBar = new FPanel(() => new Vector2(0f, -10f), () => new Vector2(35f, 35f), null, () => SKColors.Transparent);
		bottomBar.ImageEffects.Opacity.SetResponsiveState(() => (!bottomBar.InteractiveSurface.IsMouseHovering) ? 0.5f : 1f);
		bottomBar.InteractiveSurface.EnableMouseActions.SetStaticState(value: true);
		bottomBar.InteractiveSurface.MouseInteractionCallbackOnChildMouseInteraction = true;
		bottomBar.ImageEffects.Opacity.SetProcessor((float x) => RMath.Lerp(bottomBar.ImageEffects.Opacity.CachedValue, x, FContext.DeltaTime * 5f));
		bottomBar.Layout.StretchHorizontal.SetStaticState(value: true);
		bottomBar.Layout.Alignment.SetStaticState(new Vector2(0.5f, 1f));
		bottomBar.Layout.AlignmentAnchor.SetStaticState(new Vector2(0.5f, 1f));
		bottomBar.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(15f, 15f));
		bottomBar.SetParent(welcomePanelContentPane);
		FSimpleButton nextButton = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Next")), null, null, 40f);
		nextButton.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		nextButton.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		nextButton.SetParent(bottomBar);
		FSimpleButton fSimpleButton = new FSimpleButton(new FText(TextModelFactory.CreateBasic("Previous")));
		fSimpleButton.Layout.Alignment.SetStaticState(new Vector2(1f, 0.5f));
		fSimpleButton.Layout.AlignmentAnchor.SetStaticState(new Vector2(1f, 0.5f));
		fSimpleButton.Transform.LocalPosition.SetStaticState(new Vector2(-65f, 0f));
		fSimpleButton.SetParent(bottomBar);
		nextButton.InteractiveSurface.IgnoreInteractions.SetResponsiveState(() => currentOnboardingView == _onboardingViews.Count);
		FSimpleButton fSimpleButton2 = nextButton;
		fSimpleButton2.OnClick = (Action)Delegate.Combine(fSimpleButton2.OnClick, (Action)delegate
		{
			OnViewButtonPresed(1);
			if (currentOnboardingView + 1 >= _onboardingViews.Count)
			{
				nextButton.Label.Model = TextModelFactory.CreateBasic("Finish");
			}
			else
			{
				nextButton.Label.Model = TextModelFactory.CreateBasic("Next");
			}
		});
		fSimpleButton.InteractiveSurface.IgnoreInteractions.SetResponsiveState(() => currentOnboardingView <= 0);
		fSimpleButton.OnClick = (Action)Delegate.Combine(fSimpleButton.OnClick, (Action)delegate
		{
			OnViewButtonPresed(-1);
		});
		onboardingViewPane = new ModelViewPane(null);
		onboardingViewPane.AnimInDuration = 0.25f;
		onboardingViewPane.AnimOutDuration = 0.4f;
		onboardingViewPane.ViewModel = _onboardingViews[0];
		onboardingViewPane.Layout.StretchHorizontal.SetStaticState(value: true);
		onboardingViewPane.Layout.StretchVertical.SetStaticState(value: true);
		onboardingViewPane.Layout.AbsoluteMarginHorizontal.SetStaticState(new Vector2(25f, 25f));
		onboardingViewPane.Layout.AbsoluteMarginVertical.SetStaticState(new Vector2(55f, 55f));
		onboardingViewPane.SetParent(welcomePanelContentPane);
	}

	private void OnViewButtonPresed(int i)
	{
		currentOnboardingView += i;
		currentOnboardingView = RMath.Clamp(currentOnboardingView, 0, _onboardingViews.Count);
		if (currentOnboardingView == _onboardingViews.Count)
		{
			AnimateOutSequence();
		}
		else
		{
			onboardingViewPane.ViewModel = _onboardingViews[currentOnboardingView];
		}
	}

	private void AnimateOutSequence()
	{
		FContext.GetCurrentWindow().Shape.DissolveWindowRegion(this);
		welcomePanelContentPane.Enabled.SetStaticState(value: false);
		Func<float, float> easing = BezierEasing.CreateEasing(0.1f, 0f, 0.83f, -0.85f);
		Vector2 size = welcomePanel.Transform.Size.CachedValue;
		AnimatorComponent welcomePaneAnimator = new AnimatorComponent(welcomePanel, easing);
		AnimatorComponent animatorComponent = welcomePaneAnimator;
		animatorComponent.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent.OnValueUpdate, (Action<float>)delegate(float x)
		{
			welcomePanel.Transform.LocalPosition.SetStaticState(new Vector2(0f, x * (0f - (FContext.GetCurrentWindow().Shape.Bounds.Height / 2f + 500f))));
			welcomePanel.Transform.Size.SetStaticState(Vector2.Lerp(size, new Vector2(50f, 50f), Easing.EaseOutExpo(welcomePaneAnimator.UneasedTime)));
		});
		AnimatorComponent animatorComponent2 = welcomePaneAnimator;
		animatorComponent2.OnComplete = (Action)Delegate.Combine(animatorComponent2.OnComplete, (Action)delegate
		{
			AddDistortionPane();
			AddProxyNotch();
			FContext.GetCurrentDispatcher().InvokeLater(delegate
			{
				_onEndOnboarding?.Invoke();
			}, 1.5f);
		});
		welcomePaneAnimator.Start();
	}

	private void AddProxyNotch()
	{
		Notch uiObject = new Notch(isDemoNotch: true);
		AddUIObjectToRegisteredList(uiObject);
	}

	private void AddDistortionPane()
	{
		_003C_003Ec__DisplayClass18_0 _003C_003Ec__DisplayClass18_ = new _003C_003Ec__DisplayClass18_0();
	}

	public override void OnViewDestroyed()
	{
		base.OnViewDestroyed();
		FContext.GetCurrentWindow().Shape.DissolveWindowRegion(this);
	}

	private Stream GetVideoStream(string fileName)
	{
		return Assembly.GetExecutingAssembly().GetManifestResourceStream("DynamicWinV2.resources.manifest." + fileName) ?? throw new ArgumentException("Embedded resource does not exist.");
	}
}
